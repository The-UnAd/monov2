using Microsoft.EntityFrameworkCore;
using UnAd.Data.Products;
using UnAd.Data.Products.Models;

namespace ProductApi.Models;

public class PlanSubscriptionType : ObjectType<PlanSubscription> {
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
        descriptor.Field(f => f.PriceTierId).ID(nameof(PriceTier));
        descriptor.Field(f => f.ClientId).ID("Client");
        descriptor.Field("client").Type<ClientType>().Resolve(new Client(Guid.Empty));
    }
}

public record Client(Guid Id);

public class ClientType : ObjectType<Client> {
    protected override void Configure(IObjectTypeDescriptor<Client> descriptor) {
        descriptor.Field(f => f.Id).ID();
    }
}



