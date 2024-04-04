using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;

namespace GraphQLGateway;

public class LoggerExecutionEventListener(ILogger<LoggerExecutionEventListener> logger) : ExecutionDiagnosticEventListener {

    private static readonly Action<ILogger<LoggerExecutionEventListener>, Exception?> GraphqlError =
        LoggerMessage.Define(LogLevel.Error, new EventId(100, nameof(GraphqlError)), "GraphQL Request Error");

    public override void RequestError(IRequestContext context,
        Exception exception) {
        GraphqlError(logger, exception);
        base.RequestError(context, exception);
    }
}


