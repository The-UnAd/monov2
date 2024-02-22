using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UserApi;

public class ClientTypeExtensions : ObjectTypeExtension<Client> {
    protected override void Configure(IObjectTypeDescriptor<Client> descriptor) {
        descriptor.ImplementsNode()
            .IdField(f => f.Id)
            .ResolveNode(async (context, id) => {
                var factory = context.Service<IDbContextFactory<UserDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                var result = await dbContext.Clients.FindAsync(id.ToString());
                return result;
            });
    }
}



