namespace ProductApi;

public static class LoggingExtensions {
    private static readonly Action<ILogger<Mutation>, Exception> MutationException =
        LoggerMessage.Define(LogLevel.Critical, new EventId(100, nameof(MutationException)), "GraphQL Mutation Error");
    private static readonly Action<ILogger<Mutation>, string, int, Exception?> PlanCreated =
        LoggerMessage.Define<string, int>(LogLevel.Debug, new EventId(101, nameof(PlanCreated)), "Plan {Name} ({Id}) Created");
    private static readonly Action<ILogger<Mutation>, string, Exception?> KafkaMessageSent =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(101, nameof(KafkaMessageSent)), "Kafka message sent to topic {Topic}");
    private static readonly Action<ILogger<LoggerExecutionEventListener>, Exception?> GraphqlError =
        LoggerMessage.Define(LogLevel.Error, new EventId(201, nameof(GraphqlError)), "GraphQL Request Error");
    private static readonly Action<ILogger, Exception?> AuthFailure =
        LoggerMessage.Define(LogLevel.Error, new EventId(300, nameof(AuthFailure)), "Auth Failure");

    internal static void LogException(this ILogger<Mutation> logger, Exception ex) =>
        MutationException(logger, ex);
    internal static void LogPlanCreated(this ILogger<Mutation> logger, int id, string name) =>
        PlanCreated(logger, name, id, null);
    internal static void LogKafkaMessageSent(this ILogger<Mutation> logger, string topic) =>
        KafkaMessageSent(logger, topic, null);
    internal static void LogGraphqlError(this ILogger<LoggerExecutionEventListener> logger, Exception ex) =>
        GraphqlError(logger, ex);
    internal static void LogAuthFailure(this ILogger logger, Exception ex) =>
        AuthFailure(logger, ex);
}
