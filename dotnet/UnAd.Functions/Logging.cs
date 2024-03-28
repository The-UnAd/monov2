namespace UnAd.Functions;

internal static class Logging {

    private static readonly Action<ILogger<Program>, string, Exception?> LogProgramException =
        LoggerMessage.Define<string>(LogLevel.Critical, new EventId(100, "UnhandledException"), "Unexpected Error: {Message}");

    private static readonly Action<ILogger<MixpanelClient>, string, Exception?> MixpanelResponse =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(200, nameof(MixpanelResponse)), "Mixpanel Response: {Response}");

    private static readonly Action<ILogger<MixpanelClient>, string, Exception?> MixpanelErrorResponse =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(201, nameof(MixpanelErrorResponse)), "Mixpanel Error Response: {Response}");

    private static readonly Action<ILogger<MixpanelClient>, Exception?> MixpanelSendException =
        LoggerMessage.Define(LogLevel.Warning, new EventId(202, nameof(MixpanelSendException)), "Error sending Mixpanel request");

    private static readonly Action<ILogger<StripeVerifier>, string, Exception?> StripeVerificationFailure =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(300, nameof(StripeVerificationFailure)), "Stripe signature verification failed: {Message}");

    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> StripePaymentWebhookUnhandledType =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(301, nameof(StripePaymentWebhookUnhandledType)), "Skipped event type: {Type}");

    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> StripePaymentWebhookException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(400, nameof(StripePaymentWebhookException)), "Unhandled exception: {Message}");

    private static readonly Action<ILogger<StripeCustomerWebhook>, string, Exception?> StripeCustomerWebhookException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(500, nameof(StripeCustomerWebhookException)), "Unhandled exception: {Message}");

    private static readonly Action<ILogger<StripeCustomerWebhook>, string, Exception?> StripeCustomerWebhookUnhandledEvent =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(501, nameof(StripeCustomerWebhookUnhandledEvent)), "Skipped event type: {Type}");

    private static readonly Action<ILogger<StripeSubscriptionWebhook>, string, Exception?> StripeSubscriptionWebhookUnhandledEvent =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(601, nameof(StripeSubscriptionWebhookUnhandledEvent)), "Skipped event type: {Type}");

    private static readonly Action<ILogger<StripeSubscriptionWebhook>, string, Exception?> StripeSubscriptionWebhookException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(700, nameof(StripeSubscriptionWebhookException)), "Unhandled exception: {Message}");

    private static readonly Action<ILogger<MessageHandler>, string, Exception?> MessageHandlerException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(800, nameof(MessageHandlerException)), "Unhandled exception: {Message}");

    internal static void LogStripeVerificationFailure(this ILogger<StripeVerifier> logger, Exception exception) =>
        StripeVerificationFailure(logger, exception.Message, exception);
    internal static void LogUnhandledType(this ILogger<StripePaymentWebhook> logger, string type) =>
        StripePaymentWebhookUnhandledType(logger, type, null);
    internal static void LogException(this ILogger<StripePaymentWebhook> logger, Exception ex) =>
        StripePaymentWebhookException(logger, ex.Message, ex);
    internal static void LogMixpanelErrorResponse(this ILogger<MixpanelClient> logger, string response, Exception? exception = null) =>
        MixpanelErrorResponse(logger, response, exception);
    internal static void LogMixpanelSendException(this ILogger<MixpanelClient> logger, Exception? exception = null) =>
        MixpanelSendException(logger, exception);
    internal static void LogMixpanelResponse(this ILogger<MixpanelClient> logger, string response, Exception? exception = null) =>
        MixpanelResponse(logger, response, exception);
    internal static void LogException(this ILogger<Program> logger, Exception exception) =>
        LogProgramException(logger, exception.Message, exception);
    internal static void LogUnhandledEvent(this ILogger<StripeCustomerWebhook> logger, string type) =>
        StripeCustomerWebhookUnhandledEvent(logger, type, null);
    internal static void LogUnhandledEvent(this ILogger<StripeSubscriptionWebhook> logger, string type) =>
        StripeSubscriptionWebhookException(logger, type, null);
    internal static void LogException(this ILogger<StripeCustomerWebhook> logger, Exception ex) =>
        StripeCustomerWebhookException(logger, ex.Message, ex);
    internal static void LogException(this ILogger<StripeSubscriptionWebhook> logger, Exception ex) =>
        StripeSubscriptionWebhookException(logger, ex.Message, ex);
    internal static void LogException(this ILogger<MessageHandler> logger, Exception ex) =>
        MessageHandlerException(logger, ex.Message, ex);
}
