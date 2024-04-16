using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaConsumer;

internal class PaymentHandler(ILogger<PaymentHandler> logger,
                           IHostApplicationLifetime appLifetime) : IHostedService {

    public async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogServiceStarting();

        await Task.Delay(1000); // TODO: remove this line

        appLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogServiceStopping();
        return Task.CompletedTask;
    }
}

internal static class PaymentHandlerLogs {
    public static class EventIds {
        public const int ServiceStopping = 1000;
        public const int ServiceStarting = 1010;
        public const int ConnectedToRedis = 2000;
        public const int StorePrice = 2010;
        public const int StoredDefaultClient = 3000;
        public const int Exception = 3010;
    }

    public static readonly Action<ILogger<PaymentHandler>, Exception?> ServiceStopping =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStopping, nameof(ServiceStopping)), "RedisSeedService is stopping.");

    public static readonly Action<ILogger<PaymentHandler>, Exception?> ServiceStarting =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStarting, nameof(ServiceStarting)), "RedisSeedService is starting.");

    public static readonly Action<ILogger<PaymentHandler>, Exception?> Exception =
        LoggerMessage.Define(LogLevel.Error, new EventId(EventIds.Exception, nameof(Exception)), "Error storing price");

    public static void LogServiceStopping(this ILogger<PaymentHandler> logger) =>
        ServiceStopping(logger, null);
    public static void LogServiceStarting(this ILogger<PaymentHandler> logger) =>
        ServiceStarting(logger, null);
    public static void LogException(this ILogger<PaymentHandler> logger, Exception ex) =>
        Exception(logger, ex);
}
