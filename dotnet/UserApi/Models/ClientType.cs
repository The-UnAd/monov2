using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UserApi.Models;

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

    public async Task<int> GetSubscriberCount([Parent] Client client, UserDbContext dbContext) =>
        await dbContext.Entry(client).Collection(c => c.SubscriberPhoneNumbers).Query().CountAsync();

    public async Task<IQueryable<Subscriber>> GetSubscribers([Parent] Client client, UserDbContext dbContext) {
        var collection = dbContext.Entry(client).Collection(c => c.SubscriberPhoneNumbers);
        await collection.LoadAsync();
        return collection.Query();
    }
}

public class ClientType : ObjectType<Client> {
    protected override void Configure(IObjectTypeDescriptor<Client> descriptor) {
        descriptor.Field(f => f.Id).ID();
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
        descriptor.Field("subscriberCount")
            .ResolveWith<ClientResolvers>(r => r.GetSubscriberCount(default!, default!));
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
            return $"{baseUrl}/{client.Slug}";
        });
        descriptor.Field("maskedPhone").Resolve(context =>
            Util.MaskString(context.Parent<Client>().PhoneNumber, 4));
    }
}



