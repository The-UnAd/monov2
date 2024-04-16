using Microsoft.EntityFrameworkCore;
using UnAd.Data.Products;
using UnAd.Data.Products.Models;

namespace ProductApi.Models;

public class SubscriptionType : ObjectType<PlanSubscription> {
    protected override void Configure(IObjectTypeDescriptor<PlanSubscription> descriptor) {
        descriptor.Field(f => f.Id).ID();
        descriptor
            .ImplementsNode()
            .IdField(f => f.Id)
            .ResolveNode(async (context, id) => {
                var factory = context.Service<IDbContextFactory<ProductDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                var result = await dbContext.PlanSubscriptions.FindAsync(id);
                return result;
            });
    }
}



