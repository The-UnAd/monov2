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

    private static readonly Action<ILogger<MessageHelper>, string, string, Exception?> SendMessageError =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(900, nameof(SendMessageError)), "Error sending message {Sid}: {ErrorMessage}");

    private static readonly Action<ILogger<MessageHelper>, Exception?> SendMessageException =
        LoggerMessage.Define(LogLevel.Error, new EventId(901, nameof(SendMessageException)), "Exception sending message");

    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> CouldNotParse =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1000, nameof(CouldNotParse)), "No {Type} data found in event.");

    internal static void LogStripeVerificationFailure(this ILogger<StripeVerifier> logger, Exception exception) =>
        StripeVerificationFailure(logger, exception.Message, exception);
    internal static void LogUnhandledType(this ILogger<StripePaymentWebhook> logger, string type) =>
        StripePaymentWebhookUnhandledType(logger, type, default);
    internal static void LogException(this ILogger<StripePaymentWebhook> logger, Exception ex) =>
        StripePaymentWebhookException(logger, ex.Message, ex);
    internal static void LogMixpanelErrorResponse(this ILogger<MixpanelClient> logger, string response, Exception? exception = default) =>
        MixpanelErrorResponse(logger, response, exception);
    internal static void LogMixpanelSendException(this ILogger<MixpanelClient> logger, Exception? exception = default) =>
        MixpanelSendException(logger, exception);
    internal static void LogMixpanelResponse(this ILogger<MixpanelClient> logger, string response, Exception? exception = default) =>
        MixpanelResponse(logger, response, exception);
    internal static void LogException(this ILogger<Program> logger, Exception exception) =>
        LogProgramException(logger, exception.Message, exception);
    internal static void LogUnhandledEvent(this ILogger<StripeCustomerWebhook> logger, string type) =>
        StripeCustomerWebhookUnhandledEvent(logger, type, default);
    internal static void LogUnhandledEvent(this ILogger<StripeSubscriptionWebhook> logger, string type) =>
        StripeSubscriptionWebhookException(logger, type, default);
    internal static void LogException(this ILogger<StripeCustomerWebhook> logger, Exception ex) =>
        StripeCustomerWebhookException(logger, ex.Message, ex);
    internal static void LogException(this ILogger<StripeSubscriptionWebhook> logger, Exception ex) =>
        StripeSubscriptionWebhookException(logger, ex.Message, ex);
    internal static void LogException(this ILogger<MessageHandler> logger, Exception ex) =>
        MessageHandlerException(logger, ex.Message, ex);
    internal static void LogSendError(this ILogger<MessageHelper> logger, string sid, string message) =>
        SendMessageError(logger, sid, message, default);
    internal static void LogSendException(this ILogger<MessageHelper> logger, Exception ex) =>
        SendMessageException(logger, ex);
    internal static void LogCouldNotParse(this ILogger<StripePaymentWebhook> logger, string type) =>
        CouldNotParse(logger, type, default);
}
