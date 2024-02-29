namespace UnAd.Functions;

internal static class Logging {

    private static readonly Action<ILogger<Program>, string, Exception?> LogProgramException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "UnhandledException"), "Unexpected Error: {Message}");
    private static readonly Action<ILogger<MixpanelClient>, string, Exception?> LogMixpanelException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(2, "MixpanelError"), "Mixpanel Error: {Message}");
    private static readonly Action<ILogger<StripeVerifier>, string, Exception?> StripeVerificationFailure =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, "SignatureVerificationFailed"), "Stripe signature verification failed: {Message}");
    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> StripePaymentWebhookUnhandledType =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(4, "StripeUnhandledType"), "Unhandled event type: {Type}");
    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> StripePaymentWebhookException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(5, nameof(StripePaymentWebhookException)), "Unhandled exception: {Message}");

    internal static void LogStripeVerificationFailure(this ILogger<StripeVerifier> logger, Exception exception) =>
        StripeVerificationFailure(logger, exception.Message, exception);
    internal static void LogUnhandledType(this ILogger<StripePaymentWebhook> logger, string type) =>
        StripePaymentWebhookUnhandledType(logger, type, null);
    internal static void LogException(this ILogger<StripePaymentWebhook> logger, Exception ex) =>
        StripePaymentWebhookException(logger, ex.Message, ex);
    internal static void LogErrorResponse(this ILogger<MixpanelClient> logger, string response, Exception? exception = null) =>
        LogMixpanelException(logger, response, exception);
    internal static void LogException(this ILogger<Program> logger, Exception exception) =>
        LogProgramException(logger, exception.Message, exception);
}
