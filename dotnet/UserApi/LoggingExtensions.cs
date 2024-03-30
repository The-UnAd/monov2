using Twilio.Rest.Api.V2010.Account;

namespace UserApi;

public static class LoggingExtensions {
    private static readonly Action<ILogger<Mutation>, string, Exception?> MutationException =
        LoggerMessage.Define<string>(LogLevel.Critical, new EventId(100, nameof(MutationException)), "Unexpected Error: {Message}");
    private static readonly Action<ILogger<Mutation>, MessageResource, Exception?> MessageSend =
        LoggerMessage.Define<MessageResource>(LogLevel.Debug, new EventId(101, nameof(MessageSend)), "Message Sent: {Result}");
    private static readonly Action<ILogger<Mutation>, string, Exception?> MessageSendError =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(101, nameof(MessageSendError)), "Message Send Error: {Message}");

    internal static void LogException(this ILogger<Mutation> logger, Exception ex) =>
        MutationException(logger, ex.Message, ex);
    internal static void LogMessageSend(this ILogger<Mutation> logger, MessageResource resource) =>
        MessageSend(logger, resource, null);
    internal static void LogMessageSendError(this ILogger<Mutation> logger, string message) =>
        MessageSendError(logger, message, null);
}
