namespace UnAd.Functions;

internal static partial class Logging {

    [LoggerMessage(Level = LogLevel.Warning, EventId = 1000, EventName = "Application Error")]
    internal static partial void LogException(this ILogger<Program> logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Stripe signature verification failed")]
    internal static partial void LogStripeVerificationFailure(this ILogger<StripeVerifier> logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped event type: {Type}")]
    internal static partial void LogUnhandledType(this ILogger<StripePaymentWebhook> logger, string type);

    [LoggerMessage(Level = LogLevel.Warning)]
    internal static partial void LogException(this ILogger<StripePaymentWebhook> logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error sending Mixpanel request")]
    internal static partial void LogMixpanelSendException(this ILogger<MixpanelClient> logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Mixpanel Error Response: {Response}")]
    internal static partial void LogMixpanelErrorResponse(this ILogger<MixpanelClient> logger, string response);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled event: {Type}")]
    internal static partial void LogUnhandledEvent(this ILogger<StripeCustomerWebhook> logger, string type);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled event: {Type}")]
    internal static partial void LogUnhandledEvent(this ILogger<StripeSubscriptionWebhook> logger, string type);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling event: {Type}")]
    internal static partial void LogHandlingEvent(this ILogger<StripeSubscriptionWebhook> logger, string type);

    [LoggerMessage(Level = LogLevel.Warning)]
    internal static partial void LogException(this ILogger<StripeCustomerWebhook> logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning)]
    internal static partial void LogException(this ILogger<StripeSubscriptionWebhook> logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning)]
    internal static partial void LogException(this ILogger<MessageHandler> logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Subscriber {SmsFrom} not found")]
    internal static partial void LogSubscriberNotFound(this ILogger<MessageHelper> logger, string smsFrom);
}
