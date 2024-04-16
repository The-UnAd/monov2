using Microsoft.EntityFrameworkCore;
using UnAd.Data.Products;
using UnAd.Data.Products.Models;

namespace PaymentApi.Models;

public class PlanType : ObjectType<Plan>
{
    protected override void Configure(IObjectTypeDescriptor<Plan> descriptor)
    {
        descriptor.Field(f => f.Id).ID();
        descriptor
            .ImplementsNode()
            .IdField(f => f.Id)
            .ResolveNode(async (context, id) =>
            {
                var factory = context.Service<IDbContextFactory<ProductDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                var result = await dbContext.Plans.FindAsync(id);
                return result;
            });
    }
}



