using Confluent.Kafka;
using UnAd.Data.Products;
using UnAd.Data.Products.Models;

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

    public async Task<MutationResult<PlanSubscription, KafkaProduceError>> SubscribeToPlan(ProductDbContext context, IProducer<string, string> producer, SubscribeToPlanInput input, IIdSerializer idSerializer, CancellationToken cancellationToken) {
        var newTier = context.PlanSubscriptions.Add(new PlanSubscription {
            PriceTierId = input.PriceTierId,
            ClientId = input.ClientId, // TODO: validate client ID somehow
            PaymentConfirmationId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(1),
        });
        await context.SaveChangesAsync(cancellationToken);

        try {
            var id = idSerializer.Serialize(null, nameof(PlanSubscription), newTier.Entity.Id)
                ?? throw new InvalidOperationException("Failed to serialize ID");
            await producer.ProduceAsync("subscriptions", new Message<string, string> {
                Key = id,
                Value = newTier.Entity.EndDate.ToString("u")
            }, cancellationToken);
            logger.LogKafkaMessageSent(id, "subcriptions");
        } catch (ProduceException<string, string> e) {
            logger.LogException(e);
            return new KafkaProduceError(e.Message);
        } catch (InvalidOperationException e) {
            logger.LogException(e);
        }

        return newTier.Entity;
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

public record SubscribeToPlanInput(int PriceTierId, Guid ClientId);

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
            .Field(f => f.CreatePriceTier(default!, default!))
            .Argument("input", a => a.Type<NonNullType<CreatePriceTierInputType>>());
    }
}
