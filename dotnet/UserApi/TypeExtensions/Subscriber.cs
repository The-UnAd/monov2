using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UserApi.TypeExtensions;

public class SubscriberResolvers {
    public int GetSubscriptionCount([Parent] Subscriber subscriber) => subscriber.Clients.Count;
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



