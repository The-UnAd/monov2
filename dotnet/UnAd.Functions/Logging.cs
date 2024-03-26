namespace UnAd.Functions;

internal static class Logging {

    private static readonly Action<ILogger<Program>, string, Exception?> LogProgramException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(100, "UnhandledException"), "Unexpected Error: {Message}");

    private static readonly Action<ILogger<MixpanelClient>, string, Exception?> MixpanelResponse =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(200, nameof(MixpanelResponse)), "Mixpanel Response: {Response}");

    private static readonly Action<ILogger<MixpanelClient>, string, Exception?> MixpanelErrorResponse =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(201, nameof(MixpanelErrorResponse)), "Mixpanel Error Response: {Response}");

    private static readonly Action<ILogger<MixpanelClient>, Exception?> MixpanelSendException =
        LoggerMessage.Define(LogLevel.Error, new EventId(202, nameof(MixpanelSendException)), "Error sending Mixpanel request");

    private static readonly Action<ILogger<StripeVerifier>, string, Exception?> StripeVerificationFailure =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(300, "SignatureVerificationFailed"), "Stripe signature verification failed: {Message}");

    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> StripePaymentWebhookUnhandledType =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(301, "StripeUnhandledType"), "Unhandled event type: {Type}");

    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> StripePaymentWebhookException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(400, nameof(StripePaymentWebhookException)), "Unhandled exception: {Message}");

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
}
