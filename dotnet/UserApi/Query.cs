using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UserApi;

public class Query {
    public async Task<Client?> GetClient(UserDbContext context, Guid id) {
        var user = await context.Clients.FindAsync(id);
        return user;
    }

    public IQueryable<Client> GetClients(UserDbContext context) {
        return context.Clients;
    }
}

public sealed class QueryType : ObjectType<Query> {
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
        descriptor.Field(f => f.GetClient(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Client)))
            .Type<ObjectType<Client>>();
        descriptor.Field(f => f.GetClients(default!))
            //.UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
    }
}



