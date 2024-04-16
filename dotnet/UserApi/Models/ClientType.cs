using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UserApi.Models;

public class ClientResolvers {

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
        descriptor.Field("subscriberCount")
            .ResolveWith<ClientResolvers>(r => r.GetSubscriberCount(default!, default!));
        descriptor.Field(f => f.SubscriberPhoneNumbers).Ignore();
        descriptor.Field("subscribers")
            .ResolveWith<ClientResolvers>(r => r.GetSubscribers(default!, default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
        descriptor.Field("subscribeLink")
            .Type<NonNullType<StringType>>()
            .Resolve(context => {
                var client = context.Parent<Client>();
                var baseUrl = context.Service<IConfiguration>().GetSubscribeHost();
                return $"{baseUrl}/{client.Slug}";
            });
        descriptor.Field(f => f.PhoneNumber)
            .Type<NonNullType<StringType>>()
            .Resolve(context =>
                Util.MaskString(context.Parent<Client>().PhoneNumber, 4));
    }
}



