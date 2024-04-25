
using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using UnAd.Data.Products;
using UnAd.Data.Products.Models;

namespace ProductApi;

public class Query {
    public async Task<Plan?> GetPlan(ProductDbContext context, int id) => await context.Plans.FindAsync(id);
    public IQueryable<Plan> GetPlans(ProductDbContext context) => context.Plans;
    public IQueryable<PriceTier> GetPriceTiers(ProductDbContext context) => context.PriceTiers;
    public async Task<PriceTier?> GetPriceTier(ProductDbContext context, int id) => await context.PriceTiers.FindAsync(id);
    public IQueryable<PlanSubscription> GetPlanSubscriptions(ProductDbContext context) => context.PlanSubscriptions;
    public ValueTask<PlanSubscription?> GetPlanSubcription(ProductDbContext context, Guid id) => context.PlanSubscriptions.FindAsync(id);
    public Task<int> TotalPlans(ProductDbContext context) => context.Plans.CountAsync();
}

public sealed class QueryType : ObjectType<Query> {
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
        descriptor.Field(f => f.GetPlan(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Plan)))
            .Type<PlanType>();
        descriptor.Field(f => f.GetPriceTier(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(PriceTier)))
            .Type<PriceTierType>();
        descriptor.Field(f => f.GetPlanSubcription(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(PlanSubscription)))
            .Type<PlanSubscriptionType>();
        descriptor.Field(f => f.GetPlans(default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
        descriptor.Field(f => f.GetPlanSubscriptions(default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
    }
}



