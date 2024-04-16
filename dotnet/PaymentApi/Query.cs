
using Microsoft.EntityFrameworkCore;
using PaymentApi.Models;
using UnAd.Data.Products;
using UnAd.Data.Products.Models;

namespace PaymentApi;

public class Query
{
    public async Task<Plan?> GetPlan(ProductDbContext context, int id)
    {
        var user = await context.Plans.FindAsync(id);
        return user;
    }
    public IQueryable<Plan> GetPlans(ProductDbContext context) => context.Plans;
    public IQueryable<PriceTier> GetPriceTiers(ProductDbContext context) => context.PriceTiers;
    public IQueryable<PlanSubscription> GetPlanSubcriptions(ProductDbContext context) => context.PlanSubscriptions;
    public Task<int> TotalPlans(ProductDbContext context) => context.Plans.CountAsync();
}

public sealed class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor.Field(f => f.GetPlan(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Plan)))
            .Type<PlanType>();
        descriptor.Field(f => f.GetPlans(default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
    }
}



