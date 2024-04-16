using UnAd.Data.Products;
using UnAd.Data.Products.Models;

namespace ProductApi;

public class Mutation(ILogger<Mutation> logger) {
    public MutationResult<Plan> CreatePlan(ProductDbContext context, CreatePlanInput createPlanInput) {
        var newPlan = context.Plans.Add(new Plan {
            Name = createPlanInput.Name,
            Description = createPlanInput.Description,
        });
        context.SaveChanges();
        return newPlan.Entity;
    }
    public MutationResult<PriceTier> CreatePriceTier(ProductDbContext context, CreatePriceTierInput priceTierInput) {
        var newTier = context.PriceTiers.Add(new PriceTier {
            Name = priceTierInput.Name,
            Price = priceTierInput.Price,
            Duration = priceTierInput.Duration
        });
        context.SaveChanges();
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

public record CreatePlanInput(string Name, string Description);

public record CreatePriceTierInput(string Name, decimal Price, TimeSpan Duration);

public record PriceTierNotFoundError(int PriceTierId) {
    public string Message => $"Price Tier with ID {PriceTierId} not found";
}

public class MutationType : ObjectType<Mutation> {
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor) =>
        descriptor
            .Field(f => f.DeletePriceTier(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(PriceTier)))
            .UseMutationConvention();
}
