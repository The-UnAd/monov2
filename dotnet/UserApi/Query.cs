using System.Security.Claims;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UserApi;

public class Query {
    // TODO: move to Gateway
    public User GetMe(UserDbContext dbContext, IResolverContext resolverContext)
    {
        var userId = resolverContext.GetUser()?.FindFirst("userId")?.Value!;
        var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
        return user;
    }

    public async Task<User?> GetUser(UserDbContext context, string id) {
        var user = await context.Users.FindAsync(id);
        return user;
    }

    public async Task<Role?> GetRole(UserDbContext context, string id) {
        var user = await context.Roles.FindAsync(id);
        return user;
    }
}

public sealed class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor.Field(f => f.GetMe(default!, default!))
            .Type<ObjectType<User>>()
            .Authorize();
        descriptor.Field(f => f.GetUser(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID())
            .Type<ObjectType<User>>()
            .Authorize();
        descriptor.Field(f => f.GetRole(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID())
            .Type<ObjectType<Role>>()
            .Authorize();
    }
}



