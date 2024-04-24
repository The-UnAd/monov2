using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

namespace UnAd.Kafka;

public static class Topics {
    public const string Notifications = "notifications";
};

public record NotificationKey(string EventType, string EventKey) {
    public static class Types {
        public const string EndSubscription = nameof(EndSubscription);
        public const string StartSubscription = nameof(StartSubscription);
    }
    public static NotificationKey EndSubscription(string planSubscriptionNodeId) =>
        new(Types.EndSubscription, planSubscriptionNodeId);
    public static NotificationKey StartSubscription(string planSubscriptionNodeId) =>
        new(Types.StartSubscription, planSubscriptionNodeId);

    public override string ToString() => $"{EventType}:{EventKey}";
}

public record NotificationEvent {
    public static readonly NotificationEvent Empty = new();
}

public interface INotificationProducer : IDisposable {
    Task<DeliveryResult<NotificationKey, NotificationEvent>> ProduceEndSubscriptionNotification(string planSubscriptionNodeId, CancellationToken cancellationToken = default);
    Task<DeliveryResult<NotificationKey, NotificationEvent>> ProduceStartSubscriptionNotification(string planSubscriptionNodeId, CancellationToken cancellationToken = default);
}

public sealed class NotificationProducer(ProducerConfig producerConfig) : INotificationProducer {
    private const string Topic = Topics.Notifications;

    internal sealed class NotificationEventSerializer : ISerializer<NotificationEvent> {

        public byte[] Serialize(NotificationEvent data, SerializationContext context) =>
            JsonSerializer.SerializeToUtf8Bytes(data, NotificationEventSerializerContext.Default.NotificationEvent);
    }

    internal sealed class NotificationKeySerializer : ISerializer<NotificationKey> {

        public byte[] Serialize(NotificationKey data, SerializationContext context) =>
            Encoding.UTF8.GetBytes(data.ToString());
    }

    private readonly IProducer<NotificationKey, NotificationEvent> _producer =
        new ProducerBuilder<NotificationKey, NotificationEvent>(producerConfig)
        .SetValueSerializer(new NotificationEventSerializer())
        .SetKeySerializer(new NotificationKeySerializer())
        .Build();

    public Task<DeliveryResult<NotificationKey, NotificationEvent>> ProduceEndSubscriptionNotification(string planSubscriptionNodeId, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(planSubscriptionNodeId);

        return _producer.ProduceAsync(Topic, new Message<NotificationKey, NotificationEvent> {
            Key = NotificationKey.EndSubscription(planSubscriptionNodeId),
            Value = NotificationEvent.Empty
        }, cancellationToken);
    }

    public Task<DeliveryResult<NotificationKey, NotificationEvent>> ProduceStartSubscriptionNotification(string planSubscriptionNodeId, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(planSubscriptionNodeId);

        return _producer.ProduceAsync(Topic, new Message<NotificationKey, NotificationEvent> {
            Key = NotificationKey.StartSubscription(planSubscriptionNodeId),
            Value = NotificationEvent.Empty
        }, cancellationToken);
    }

    public void Dispose() =>
        _producer.Dispose();
}

public interface INotificationConsumer : IDisposable {
    void Subscribe();
    ConsumeResult<NotificationKey, NotificationEvent> Consume(CancellationToken cancellationToken = default);
    void Commit(ConsumeResult<NotificationKey, NotificationEvent> consumeResult);
    void Close();
}

public sealed class NotificationConsumer(ConsumerConfig consumerConfig) : INotificationConsumer {
    private const string Topic = Topics.Notifications;

    internal sealed class NotificationDeserializer : IDeserializer<NotificationEvent> {
        public NotificationEvent Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) =>
            isNull
            || Encoding.UTF8.GetString(data).Equals("null", StringComparison.OrdinalIgnoreCase)
                ? NotificationEvent.Empty
                : JsonSerializer.Deserialize(data, NotificationEventSerializerContext.Default.NotificationEvent)
                    ?? throw new InvalidOperationException("Failed to deserialize notification event");
    }

    internal sealed class NotificationKeyDeserializer : IDeserializer<NotificationKey> {
        public NotificationKey Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) {
            var key = Encoding.UTF8.GetString(data);
            var parts = key.Split(':');
            return new NotificationKey(parts[0], parts[1]);
        }
    }

    private readonly IConsumer<NotificationKey, NotificationEvent> _consumer =
        new ConsumerBuilder<NotificationKey, NotificationEvent>(consumerConfig)
        .SetValueDeserializer(new NotificationDeserializer())
        .SetKeyDeserializer(new NotificationKeyDeserializer())
        .Build();

    public void Subscribe() =>
        _consumer.Subscribe(Topic);

    public ConsumeResult<NotificationKey, NotificationEvent> Consume(CancellationToken cancellationToken = default) =>
        _consumer.Consume(cancellationToken);

    public void Commit(ConsumeResult<NotificationKey, NotificationEvent> consumeResult) =>
        _consumer.Commit(consumeResult);
    public void Close() =>
        _consumer.Close();

    public void Dispose() =>
        _consumer.Dispose();
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(NotificationEvent))]
internal sealed partial class NotificationEventSerializerContext : JsonSerializerContext { }
