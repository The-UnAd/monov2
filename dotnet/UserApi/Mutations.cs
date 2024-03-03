using StackExchange.Redis;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using UnAd.Redis;

namespace UserApi;

public class Mutation {
    public MutationResult<Client, ClientNotFoundError> DeleteClient(UserDbContext context, IConnectionMultiplexer redis, Guid id) {
        var client = context.Clients.Find(id);
        if (client is null) {
            return new ClientNotFoundError(id);
        }
        redis.GetDatabase().DeleteClientProductLimits(client.PhoneNumber);
        context.Clients.Remove(client);
        context.SaveChanges();
        return client;
    }
}

public record ClientNotFoundError(Guid ClientId) {
    public string Message => $"Client with ID {ClientId} not found";
}

public class MutationType : ObjectType<Mutation> {
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor) =>
        descriptor
            .Field(f => f.DeleteClient(default, default, default))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Client)))
            .UseMutationConvention();
}
