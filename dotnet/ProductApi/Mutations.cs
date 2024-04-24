using Confluent.Kafka;
using HotChocolate.Types.Relay;
using UnAd.Data.Products;
using UnAd.Data.Products.Models;
using UnAd.Kafka;

namespace ProductApi;

public class Mutation(ILogger<Mutation> logger) {
    public MutationResult<Plan> CreatePlan(ProductDbContext context, CreatePlanInput input) {
        var newPlan = context.Plans.Add(new Plan {
            Name = input.Name,
            Description = input.Description,
        });
        logger.LogPlanCreated(newPlan.Entity.Id, newPlan.Entity.Name);
        context.SaveChanges();
        return newPlan.Entity;
    }
    public MutationResult<PriceTier> CreatePriceTier(ProductDbContext context, CreatePriceTierInput input) {
        var newTier = context.PriceTiers.Add(new PriceTier {
            Name = input.Name,
            Price = input.Price,
            Duration = input.Duration,
            PlanId = input.PlanId
        });
        context.SaveChanges();
        return newTier.Entity;
    }

    public async Task<MutationResult<PlanSubscription, KafkaProduceError>> SubscribeToPlan(SubscribeToPlanInput input,
                                                                                           ProductDbContext context,
                                                                                           IIdSerializer idSerializer,
                                                                                           INotificationProducer notificationProducer,
                                                                                           CancellationToken cancellationToken) {

        var newTier = context.PlanSubscriptions.Add(new PlanSubscription {
            PriceTierId = input.PriceTierId,
            ClientId = input.ClientId, // TODO: validate client ID somehow
            PaymentConfirmationId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(1),
        });
        await context.SaveChangesAsync(cancellationToken);

        try {
            var id = idSerializer.Serialize(null, nameof(PlanSubscription), newTier.Entity.Id)
                ?? throw new InvalidOperationException("Failed to serialize node ID");
            await notificationProducer.ProduceStartSubscriptionNotification(id, cancellationToken);
        } catch (ProduceException<string, string> e) {
            logger.LogException(e);
            return new KafkaProduceError(e.Message);
        } catch (InvalidOperationException e) {
            logger.LogException(e);
        }
        return newTier.Entity;
    }

    public async Task<MutationResult<PlanSubscription, KafkaProduceError, PlanSubscriptionNotFoundError>> EndSubscription(EndSubscriptionInput input,
                                                                                                                          ProductDbContext context,
                                                                                                                          IIdSerializer idSerializer,
                                                                                                                          INotificationProducer notificationProducer,
                                                                                                                          CancellationToken cancellationToken) {
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // TODO: WHY THE FUCK ISN'T THIS EXECUTING?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?!?
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        var planSubscription = await context.PlanSubscriptions.FindAsync([input.Id], cancellationToken);
        if (planSubscription is null) {
            return new PlanSubscriptionNotFoundError(input.Id);
        }
        // TODO: additional validation?  Maybe check if it's already ended?
        planSubscription.EndDate = DateTime.UtcNow;
        planSubscription.Status = "ENDED";
        await context.SaveChangesAsync(cancellationToken);

        try {
            var id = idSerializer.Serialize(null, nameof(PlanSubscription), planSubscription.Id)
                ?? throw new InvalidOperationException("Failed to serialize node ID");
            await notificationProducer.ProduceEndSubscriptionNotification(id, cancellationToken);
        } catch (ProduceException<string, string> e) {
            logger.LogException(e);
            return new KafkaProduceError(e.Message);
        } catch (InvalidOperationException e) {
            logger.LogException(e);
        }

        return planSubscription;
    }
    public MutationResult<PriceTier, PriceTierNotFoundError> DeletePriceTier(ProductDbContext context, int id) {
        var priceTier = context.PriceTiers.Find(id);
        if (priceTier is null) {
            return new PriceTierNotFoundError(id);
        }
        context.PriceTiers.Remove(priceTier);
        context.SaveChanges();
        return priceTier;
    }
}

public record KafkaProduceError(string Message) {
    public string Type => "KafkaProduceError";
}

public record SubscribeToPlanInput(int PriceTierId, Guid ClientId, Guid PaymentConfirmationId);

public record EndSubscriptionInput(Guid Id);
public class EndSubscriptionInputType : InputObjectType<EndSubscriptionInput> {
    protected override void Configure(IInputObjectTypeDescriptor<EndSubscriptionInput> descriptor) {
        descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().ID(nameof(PlanSubscription));
    }
}

public class SubscribeToPlanInputType : InputObjectType<SubscribeToPlanInput> {
    protected override void Configure(IInputObjectTypeDescriptor<SubscribeToPlanInput> descriptor) {
        descriptor.Field(f => f.PriceTierId).Type<NonNullType<IdType>>().ID(nameof(PriceTier));
        descriptor.Field(f => f.ClientId).Type<NonNullType<IdType>>().ID("Client");
    }
}

public record CreatePlanInput(string Name, string Description);

public record CreatePriceTierInput(string Name, decimal Price, TimeSpan Duration, int PlanId);
public class CreatePriceTierInputType : InputObjectType<CreatePriceTierInput> {
    protected override void Configure(IInputObjectTypeDescriptor<CreatePriceTierInput> descriptor) {
        descriptor.Field(f => f.PlanId).Type<NonNullType<IdType>>().ID(nameof(Plan));
    }
}

public record PriceTierNotFoundError(int PriceTierId) {
    public string Message => $"Price Tier with ID {PriceTierId} not found";
}

public record PlanSubscriptionNotFoundError(Guid Id) {
    public string Message => $"Plan Subscription with ID {Id} not found";
}

public class MutationType : ObjectType<Mutation> {
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor) {
        descriptor
            .Field(f => f.DeletePriceTier(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(PriceTier)))
            .UseMutationConvention();
        descriptor
            .Field(f => f.SubscribeToPlan(default!, default!, default!, default!, default!))
            .Argument("input", a => a.Type<NonNullType<SubscribeToPlanInputType>>());
        descriptor
            .Field(f => f.EndSubscription(default!, default!, default!, default!, default!))
            .Argument("input", a => a.Type<NonNullType<EndSubscriptionInputType>>());
        descriptor
            .Field(f => f.CreatePriceTier(default!, default!))
            .Argument("input", a => a.Type<NonNullType<CreatePriceTierInputType>>());
    }
}
