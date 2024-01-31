using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace UserApi;

public class RoleTypeExtensions : ObjectTypeExtension<Role> {
    protected override void Configure(IObjectTypeDescriptor<Role> descriptor) {
        descriptor.Field(f => f.Users)
            .Ignore();
    }
}

public class UserTypeExtensions : ObjectTypeExtension<User> {
    protected override void Configure(IObjectTypeDescriptor<User> descriptor) {
        descriptor.ImplementsNode()
            .IdField(f => f.Id)
            .ResolveNode(async (context, id) => {
                var factory = context.Service<IDbContextFactory<UserDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                var result = await dbContext.Users.FindAsync(id);
                return result;
            });
        descriptor.Field(f => f.Id)
            .Name("id")
            .Type<NonNullType<IdType>>()
            .ID();
    }
}



