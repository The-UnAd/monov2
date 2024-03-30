using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UserApi;

public class Query {
    public async Task<Client?> GetClient(UserDbContext context, Guid id) {
        var user = await context.Clients.FindAsync(id);
        return user;
    }

    // TODO: how do I get just a count of clients?
    public IQueryable<Client> GetClients(UserDbContext context) => context.Clients;
    public Task<int> CountClients(UserDbContext context) => context.Clients.CountAsync();
    public Task<int> CountSubscribers(UserDbContext context) => context.Subscribers.CountAsync();
}

public sealed class QueryType : ObjectType<Query> {
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
        descriptor.Field(f => f.GetClient(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Client)))
            .Type<ObjectType<Client>>();
        descriptor.Field(f => f.GetClients(default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
    }
}



