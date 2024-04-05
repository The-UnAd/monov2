using HotChocolate.Types;
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

    public string GetSubscribeLink([Parent] Client client, IConfiguration config) {
        var baseUrl = config.GetSubscribeHost();
        return $"{baseUrl}/{client.Id}";
    }

    public int GetSubscriberCount([Parent] Client client) =>
        client.SubscriberPhoneNumbers.Count;

    public IQueryable<Subscriber> GetSubscribers([Parent] Client client, UserDbContext dbContext) =>
        dbContext.Entry(client).Collection(c => c.SubscriberPhoneNumbers).Query();
}

public class SubscriberResolvers {
    public int GetSubscriptionCount([Parent] Subscriber subscriber) => subscriber.Clients.Count;
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
            .ResolveWith<ClientResolvers>(r => r.GetSubscription(default!, default!));
        descriptor.Field(f => f.SubscriberPhoneNumbers).Ignore();
        descriptor.Field("subscribers")
            .ResolveWith<ClientResolvers>(r => r.GetSubscribers(default!, default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
        descriptor.Field("subscribeLink").Resolve(context => {
            var client = context.Parent<Client>();
            var baseUrl = context.Service<IConfiguration>().GetSubscribeHost();
            return $"{baseUrl}/{client.Id}";
        });
        descriptor.Field("subscriberCount")
            .ResolveWith<ClientResolvers>(r => r.GetSubscribers(default!, default!))
            .Type<NonNullType<IntType>>();
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
        descriptor.Field("subscriptionCount")
            .ResolveWith<SubscriberResolvers>(r => r.GetSubscriptionCount(default!));
    }
}



