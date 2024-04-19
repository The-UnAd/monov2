using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaConsumer.Handlers;

internal class SubscriptionHandler(ILogger<SubscriptionHandler> logger,
                                   IConsumer<string, string> consumer,
                                   IUnAdClient unAdClient,
                                   IHostApplicationLifetime appLifetime) : IHostedService {

    public async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogServiceStarting();

        consumer.Subscribe("subscriptions");

        try {
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    var cr = consumer.Consume(cancellationToken);
                    var input = JsonSerializer.Deserialize<SubscribeToPlanInput>(cr.Message.Value);
                    if (input is null) {
                        logger.LogAction("Could not parse message value.");
                        consumer.Commit(cr);
                        continue;
                    }
                    var result = await unAdClient.SubscribeToPlan.ExecuteAsync(input, cancellationToken);
                    if (result?.Data?.SubscribeToPlan.PlanSubscription is ISubscribeToPlan_SubscribeToPlan_PlanSubscription subscription) {
                        logger.LogAction($"Created Plan Subscription for client {subscription.ClientId}");
                        consumer.Commit(cr);
                        logger.LogAction($"Message {cr.Message.Key} handled successfully! 🎉");
                    } else {
                        logger.LogAction("No Plan Subscription created");
                    }
                } catch (ConsumeException e) {
                    logger.LogException(e);
                }
            }
        } catch (OperationCanceledException e) {
            logger.LogAction(e.Message);
            appLifetime.StopApplication();
        }

        appLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogServiceStopping();
        consumer.Close();
        return Task.CompletedTask;
    }
}

internal static class SubscriptionHandlerLogs {
    public static class EventIds {
        public const int ServiceStopping = 1100;
        public const int ServiceStarting = 1110;
        public const int ConnectedToRedis = 2100;
        public const int StorePrice = 2110;
        public const int StoredDefaultClient = 3100;
        public const int Exception = 3110;
        public const int Action = 4100;
    }

    public static readonly Action<ILogger<SubscriptionHandler>, Exception?> ServiceStopping =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStopping, nameof(ServiceStopping)), "SubscriptionHandler is stopping.");

    public static readonly Action<ILogger<SubscriptionHandler>, Exception?> ServiceStarting =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStarting, nameof(ServiceStarting)), "SubscriptionHandler is starting.");

    public static readonly Action<ILogger<SubscriptionHandler>, Exception?> Exception =
        LoggerMessage.Define(LogLevel.Error, new EventId(EventIds.Exception, nameof(Exception)), "An Exception occurred");

    public static readonly Action<ILogger<SubscriptionHandler>, string, Exception?> Action =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(EventIds.Action, nameof(Action)), "SubscriptionHandler: {Message}");

    public static void LogServiceStopping(this ILogger<SubscriptionHandler> logger) =>
        ServiceStopping(logger, null);
    public static void LogServiceStarting(this ILogger<SubscriptionHandler> logger) =>
        ServiceStarting(logger, null);
    public static void LogException(this ILogger<SubscriptionHandler> logger, Exception ex) =>
        Exception(logger, ex);
    public static void LogAction(this ILogger<SubscriptionHandler> logger, string message) =>
        Action(logger, message, null);
}
