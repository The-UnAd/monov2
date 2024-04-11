using StackExchange.Redis;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using UnAd.Redis;

namespace UserApi;

public class Mutation(ILogger<Mutation> logger) {
    public MutationResult<Client, ClientNotFoundError> DeleteClient(UserDbContext context, IConnectionMultiplexer redis, Guid id) {
        var client = context.Clients.Find(id);
        if (client is null) {
            return new ClientNotFoundError(id);
        }
        redis.GetDatabase().DeleteClientPriceLimits(client.PhoneNumber);
        context.Clients.Remove(client);
        context.SaveChanges();
        return client;
    }


    public async Task<MutationResult<SendMessagePayload, SendMessageFailed>> SendMessage(UserDbContext context, IMessageSender messageSender, SendMessageInput input) {

        var receivers = input.Audience switch {
            Audience.Clients => context.Clients.WithActiveSubscriptions().Select(c => c.PhoneNumber),
            Audience.Subscribers => context.Subscribers.Select(s => s.PhoneNumber),
            _ => Enumerable.Empty<string>()
        };

        var errors = new List<SendMessageFailed>();
        var sent = 0;
        foreach (var receiver in receivers.ToArray()) {
            try {
                var result = await messageSender.Send(receiver, input.Message);
                logger.LogMessageSend(result);
                if (result.ErrorCode is not null) {
                    logger.LogMessageSendError(result.ErrorMessage);
                    errors.Add(new SendMessageFailed(result.ErrorMessage, result.Sid));
                } else {
                    sent++;
                }
            } catch (Exception e) {
                logger.LogException(e);
                errors.Add(new SendMessageFailed(e.Message));
            }
        }

        if (sent == 0) {
            return new MutationResult<SendMessagePayload, SendMessageFailed>(errors);
        }
        return new SendMessagePayload(sent);

    }
}

public record SendMessageInput(Audience Audience, string Message);
public record SendMessagePayload(int Sent);
public record SendMessageFailed(string Message, string? Sid = default);

[Flags]
public enum Audience {
    Clients = 1 << 0,
    Subscribers = 1 << 1
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
