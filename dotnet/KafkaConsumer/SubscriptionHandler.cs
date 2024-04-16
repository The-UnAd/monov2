using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaConsumer;

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
                    var planSubscription = await unAdClient.GetPlanSubscription.ExecuteAsync(cr.Message.Key, cancellationToken);
                    if (planSubscription?.Data?.PlanSubcription is not null) {
                        logger.LogAction($"Found Plan Subscription with End Date {planSubscription.Data.PlanSubcription.EndDate}");
                    } else {
                        logger.LogAction("No Plan Subscription found");
                    }
                    logger.LogAction($"Message {cr.Message.Key} Handled");
                    consumer.Commit(cr);
                } catch (ConsumeException e) {
                    logger.LogException(e);
                }
            }
        } catch (OperationCanceledException e) {
            logger.LogException(e);
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
