using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using UserApi.Models;

namespace UserApi;

public class Query {
    public async Task<Client?> GetClient(UserDbContext context, Guid id) {
        var user = await context.Clients.FindAsync(id);
        return user;
    }
    public IQueryable<Client> GetClients(UserDbContext context) => context.Clients;
    public Task<int> TotalClients(UserDbContext context) => context.Clients.CountAsync();
    public async Task<Subscriber?> GetSubscriber(UserDbContext context, string id) =>
        await context.Subscribers.FindAsync(id);
    public IQueryable<Subscriber> GetSubscribers(UserDbContext context) => context.Subscribers;
    public Task<int> TotalSubscribers(UserDbContext context) => context.Subscribers.CountAsync();
}

public record User(string Id);

public class UserType : ObjectType<User> { }

public sealed class QueryType : ObjectType<Query> {
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
        descriptor.Field(f => f.GetClient(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Client)))
            .Type<ClientType>();
        descriptor.Field(f => f.GetClients(default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();

        descriptor.Field(f => f.GetSubscriber(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Subscriber)))
            .Type<SubscriberType>();
        descriptor.Field(f => f.GetSubscribers(default!))
            .Type<SubscriberType>()
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();

        descriptor.Field("viewer")
            .Type<UserType>()
            .Resolve(context => {
                var user = context.GetUser();
                if (user?.FindFirst("username")?.Value is string userId) {
                    return new User(userId);
                }
                return null;
            });
    }
}



