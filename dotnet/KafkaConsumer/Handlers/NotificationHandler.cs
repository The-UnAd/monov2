using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnAd.Kafka;

namespace KafkaConsumer;

internal class NotificationHandler(ILogger<NotificationHandler> logger,
                                   INotificationConsumer consumer,
                                   IUnAdClient unAdClient,
                                   IHostApplicationLifetime appLifetime) : IHostedService {

    public async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogServiceStarting();

        consumer.Subscribe();

        try {
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    var cr = consumer.Consume(cancellationToken);
                    // TODO: send notification
                    await (cr switch {
                        { Message.Key.EventKey: var planSubscriptionNodeId, Message.Key.EventType: NotificationKey.Types.EndSubscription } =>
                            HandleEndSubscription(planSubscriptionNodeId, cancellationToken),
                        { Message.Key.EventKey: var planSubscriptionNodeId, Message.Key.EventType: NotificationKey.Types.StartSubscription } =>
                            HandleStartSubscription(planSubscriptionNodeId, cancellationToken),
                        _ => Task.CompletedTask,
                    });

                    logger.LogAction($"Message {cr.Message.Key} handled successfully! 🎉");
                    consumer.Commit(cr);
                } catch (ConsumeException e) {
                    logger.LogException(e);
                } catch (Exception e) {
                    logger.LogException(e);
                }
            }
        } catch (OperationCanceledException e) {
            logger.LogAction(e.Message);
            appLifetime.StopApplication();
        }

        appLifetime.StopApplication();
    }

    private async Task HandleEndSubscription(string planSubscriptionId, CancellationToken cancellationToken = default) {
        logger.LogAction($"Handling EndSubscription for {planSubscriptionId}");
        var result = await unAdClient.GetPlanSubscription.ExecuteAsync(planSubscriptionId, cancellationToken);
        if (result.Data?.PlanSubcription is not IGetPlanSubscription_PlanSubcription subscription) {
            // TODO: log error
            return;
        }
        var clientPhone = subscription.Client.PhoneNumber;
        logger.LogAction($"Sending SMS to {clientPhone}");
        await Task.CompletedTask;
    }

    private async Task HandleStartSubscription(string planSubscriptionId, CancellationToken cancellationToken = default) {
        logger.LogAction($"Handling EndSubscription for {planSubscriptionId}");
        var result = await unAdClient.GetPlanSubscription.ExecuteAsync(planSubscriptionId, cancellationToken);
        if (result.Data?.PlanSubcription is not IGetPlanSubscription_PlanSubcription subscription) {
            // TODO: log error
            return;
        }
        var clientPhone = subscription.Client.PhoneNumber;
        logger.LogAction($"Sending SMS to {clientPhone}");
        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogServiceStopping();
        consumer.Close();
        return Task.CompletedTask;
    }
}

internal static class NotificationHandlerLogs {
    public static class EventIds {
        public const int ServiceStopping = 1100;
        public const int ServiceStarting = 1110;
        public const int Exception = 3110;
        public const int Action = 4100;
    }

    public static readonly Action<ILogger<NotificationHandler>, Exception?> ServiceStopping =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStopping, nameof(ServiceStopping)), "NotificationHandler is stopping.");

    public static readonly Action<ILogger<NotificationHandler>, Exception?> ServiceStarting =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStarting, nameof(ServiceStarting)), "NotificationHandler is starting.");

    public static readonly Action<ILogger<NotificationHandler>, Exception?> Exception =
        LoggerMessage.Define(LogLevel.Error, new EventId(EventIds.Exception, nameof(Exception)), "An Exception occurred");

    public static readonly Action<ILogger<NotificationHandler>, string, Exception?> Action =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(EventIds.Action, nameof(Action)), "NotificationHandler: {Message}");

    public static void LogServiceStopping(this ILogger<NotificationHandler> logger) =>
        ServiceStopping(logger, null);
    public static void LogServiceStarting(this ILogger<NotificationHandler> logger) =>
        ServiceStarting(logger, null);
    public static void LogException(this ILogger<NotificationHandler> logger, Exception ex) =>
        Exception(logger, ex);
    public static void LogAction(this ILogger<NotificationHandler> logger, string message) =>
        Action(logger, message, null);
}
