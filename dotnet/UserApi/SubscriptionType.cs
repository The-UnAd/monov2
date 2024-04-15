using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using UnAd.Data.Users.Models;
using UserApi.Models;

namespace UserApi;

public class Subscriptions {
    public static class Events {
        public const string ClientAdded = nameof(ClientAdded);
        public const string SubscriberSubscribed = nameof(SubscriberSubscribed);
        public const string SubscriberUnsubscribed = nameof(SubscriberUnsubscribed);
    }
    public ValueTask<ISourceStream<Subscriber>> SubscribeToSubscribers(
        [Service] ITopicEventReceiver receiver)
        => receiver.SubscribeAsync<Subscriber>("ExampleTopic");

    [Subscribe(With = nameof(SubscribeToSubscribers))]
    public Subscriber SubscriberAdded([EventMessage] Subscriber subscriber)
        => subscriber;
}

public class SubscriptionType : ObjectType<Subscriptions> {

    protected override void Configure(IObjectTypeDescriptor<Subscriptions> descriptor) {
        descriptor
            .Field(Subscriptions.Events.ClientAdded)
            .Type<ClientType>()
            .Resolve(context => context.GetEventMessage<Client>())
            .Subscribe(async context => {
                var receiver = context.Service<ITopicEventReceiver>();

                var stream =
                    await receiver.SubscribeAsync<Client>(Subscriptions.Events.ClientAdded);

                return stream;
            });
        descriptor
            .Field(Subscriptions.Events.SubscriberUnsubscribed)
            .Type<SubscriberType>()
            .Resolve(context => context.GetEventMessage<Subscriber>())
            .Subscribe(async context => {
                var receiver = context.Service<ITopicEventReceiver>();

                var stream =
                    await receiver.SubscribeAsync<Subscriber>(Subscriptions.Events.SubscriberUnsubscribed);

                return stream;
            });
    }
}

