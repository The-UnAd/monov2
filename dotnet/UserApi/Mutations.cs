using HotChocolate.Subscriptions;
using StackExchange.Redis;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using UnAd.Redis;

namespace UserApi;

public class Mutation(ILogger<Mutation> logger) {
    // TODO: look into whether making these all async actually makes a real difference
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

    public MutationResult<Client, ClientNotFoundError, SubscriberNotFoundError, AlreadySubscribedError> SubscribeToClient(UserDbContext context, Guid clientId, string subscriberId) {
        var client = context.Clients.Find(clientId);
        if (client is null) {
            return new ClientNotFoundError(clientId);
        }
        var subscriber = context.Subscribers.Find(subscriberId);
        if (subscriber is null) {
            return new SubscriberNotFoundError(subscriberId);
        }

        if (client.SubscriberPhoneNumbers.Contains(subscriber)) {
            return new AlreadySubscribedError(subscriberId);
        }

        client.SubscriberPhoneNumbers.Add(subscriber);
        context.SaveChanges();

        return client;
    }

    public async Task<MutationResult<Client, ClientNotFoundError, NotSubscribedError>> UnsubscribeFromClient(UserDbContext context, Guid clientId, string subscriberId, ITopicEventSender sender) {
        var client = context.Clients.Find(clientId);
        if (client is null) {
            return new ClientNotFoundError(clientId);
        }
        context.Entry(client).Collection(c => c.SubscriberPhoneNumbers).Load();
        var subscriber = client.SubscriberPhoneNumbers.FirstOrDefault(s => s.PhoneNumber == subscriberId);
        if (subscriber is null) {
            return new NotSubscribedError(subscriberId);
        }

        client.SubscriberPhoneNumbers.Remove(subscriber);
        await context.SaveChangesAsync();

        await sender.SendAsync(Subscriptions.Events.SubscriberUnsubscribed, subscriber);
        return client;
    }

    public MutationResult<Subscriber, SubscriberNotFoundError> DeleteSubscriber(UserDbContext context, string id) {
        var subscriber = context.Subscribers.Find(id);
        if (subscriber is null) {
            return new SubscriberNotFoundError(id);
        }
        context.Subscribers.Remove(subscriber);
        context.SaveChanges();
        return subscriber;
    }

    public async Task<MutationResult<Subscriber, SubscriberExistsError>> AddSubscriber(UserDbContext context, AddSubscriberInput input, ITopicEventSender sender) {
        var existing = context.Subscribers.Find(input.PhoneNumber);
        if (existing is not null) {
            return new SubscriberExistsError(input.PhoneNumber);
        }
        var newRecord = context.Subscribers.Add(new Subscriber {
            PhoneNumber = input.PhoneNumber,
            Locale = input.Locale
        });
        await context.SaveChangesAsync();
        await sender.SendAsync(Subscriptions.Events.SubscriberSubscribed, newRecord.Entity);
        return newRecord.Entity;
    }

    public async Task<MutationResult<SendMessagePayload, SendMessageFailed>> SendMessage(UserDbContext context, IMessageSender messageSender, SendMessageInput input) {

        var receivers = input.Audience switch {
            Audience.AllClients =>
                context.Clients
                    .Select(c => c.PhoneNumber),
            Audience.ActiveClients =>
                context.Clients
                    .WithActiveSubscriptions()
                    .Select(c => c.PhoneNumber),
            Audience.Subscribers =>
                context.Subscribers
                    .Select(s => s.PhoneNumber),
            Audience.ActiveClientsWithoutSubscribers =>
                context.Clients
                    .WithActiveSubscriptions()
                    .WithNoSubscribers()
                    .Select(c => c.PhoneNumber),
            Audience.Everyone =>
                context.Clients
                    .Select(c => c.PhoneNumber)
                    .Union(context.Subscribers.Select(s => s.PhoneNumber)),
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
        return new SendMessagePayload(sent, errors.Count);

    }
}

public record SendMessageInput(Audience Audience, string Message);

public record AddSubscriberInput(string PhoneNumber, string Locale);
public record SendMessagePayload(int Sent, int Failed);
public record SendMessageFailed(string Message, string? Sid = default);

[Flags]
public enum Audience {
    AllClients = 1 << 0,
    ActiveClients = 1 << 1,
    Subscribers = 1 << 2,
    ActiveClientsWithoutSubscribers = 1 << 3,
    Everyone = AllClients | Subscribers
}



public record ClientNotFoundError(Guid ClientId) {
    public string Message => $"Client with ID {ClientId} not found";
}

public record SubscriberNotFoundError(string PhoneNumber) {
    public string Message => $"Subscriber with phone number {PhoneNumber} not found";
}

public record SubscriberExistsError(string PhoneNumber) {
    public string Message => $"Subscriber with phone number {PhoneNumber} already exists";
}

public record AlreadySubscribedError(string PhoneNumber) {
    public string Message => $"Subscriber with phone number {PhoneNumber} is already subscribed to client";
}

public record NotSubscribedError(string PhoneNumber) {
    public string Message => $"Subscriber with phone number {PhoneNumber} is not subscribed to client";
}

public class MutationType : ObjectType<Mutation> {
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor) {
        descriptor
            .Field(f => f.DeleteClient(default!, default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Client)))
            .UseMutationConvention();
        descriptor
            .Field(f => f.DeleteSubscriber(default!, default!))
            .Argument("id", a => a.Type<NonNullType<IdType>>().ID(nameof(Subscriber)))
            .UseMutationConvention();
        descriptor
            .Field(f => f.SubscribeToClient(default!, default!, default!))
            .Argument("clientId", a => a.Type<NonNullType<IdType>>().ID(nameof(Client)))
            .Argument("subscriberId", a => a.Type<NonNullType<IdType>>().ID(nameof(Subscriber)))
            .UseMutationConvention();
        descriptor
            .Field(f => f.UnsubscribeFromClient(default!, default!, default!, default!))
            .Argument("clientId", a => a.Type<NonNullType<IdType>>().ID(nameof(Client)))
            .Argument("subscriberId", a => a.Type<NonNullType<IdType>>().ID(nameof(Subscriber)))
            .UseMutationConvention();
    }
}
