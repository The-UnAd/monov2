using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using UserApi.Models;

namespace UserApi;

public class ClientResolvers {
    // TODO: Find out how to make HotChocolate load the SubscripionId when I ask for the subscription field in the client type
    public async Task<StripeSubscription?> GetSubscription([Parent] Client client, Stripe.IStripeClient stripeClient) {
        var service = new Stripe.SubscriptionService(stripeClient);
        if (client.SubscriptionId is null) {
            return null;
        }
        var stripeSubscription = await service.GetAsync(client.SubscriptionId);
        return stripeSubscription.ToSubscriptionType();
    }
}

public class ClientTypeExtensions : ObjectTypeExtension<Client> {
    protected override void Configure(IObjectTypeDescriptor<Client> descriptor) {
        descriptor
            .ImplementsNode()
            .IdField(f => f.Id)
            .ResolveNode(async (context, id) => {
                var factory = context.Service<IDbContextFactory<UserDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                var result = await dbContext.Clients.FindAsync(id.ToString());
                return result;
            });
        descriptor.Field("subscription")
            .ResolveWith<ClientResolvers>(r => r.GetSubscription(default!, default!))
            .Type<StripeSubscriptionType>();
        descriptor.Field(f => f.SubscriberPhoneNumbers)
            .Name("subscribers");
    }
}

public class SubscriberTypeExtensions : ObjectTypeExtension<Subscriber> {
    protected override void Configure(IObjectTypeDescriptor<Subscriber> descriptor) {
        descriptor
            .ImplementsNode()
            .IdField(f => f.PhoneNumber)
            .ResolveNode(async (context, id) => {
                var factory = context.Service<IDbContextFactory<UserDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                var result = await dbContext.Subscribers.FindAsync(id);
                return result;
            });
    }
}



