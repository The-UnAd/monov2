using StackExchange.Redis;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddExceptionHandler<Exception>((o, ex) =>
    o.ExceptionHandler = async context => {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(ex.Message);
    });

builder.Services.AddLogging(o => o.AddConsole());

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>()["REDIS_URL"]!));

builder.Services.AddHealthChecks();

var app = builder.Build();


app.Use(async (context, next) => {
    if (context.Request.Path.StartsWithSegments("/health")) {
        await next.Invoke(context);
        return;
    }
    if (context.Request.Headers.TryGetValue("X-Api-Key", out var value) &&
        value == context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("API_KEY")) {
        await next.Invoke(context);
        return;
    }
    context.Response.StatusCode = 401;
    await context.Response.WriteAsync("Unauthorized");
});

app.MapGet("/{name}", async (string name, IConnectionMultiplexer redis) => {
    var db = redis.GetDatabase();

    var url = await db.StringGetAsync($"graph:{name}");
    if (url.IsNullOrEmpty) {
        return Results.NotFound();
    }
    return Results.Text(url);
});

app.MapPost("/{name}", async (string name, HttpRequest request, IConnectionMultiplexer redis) => {
    var db = redis.GetDatabase();
    using var reader = new StreamReader(request.Body);
    var url = await reader.ReadToEndAsync();
    if (string.IsNullOrEmpty(url)) {
        return Results.BadRequest();
    }

    await db.StringSetAsync($"graph:{name}", url);

    var logger = request.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    GraphMonitorLogs.LogGraphStored(logger, name, null);
    return Results.Ok();
});

app.MapHealthChecks("/health");

await app.RunAsync();

internal class GraphMonitorLogs {
    public static readonly Action<ILogger<Program>, string, Exception?> LogGraphStored =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "GraphStored"), "Graph {Name} stored");
}

public partial class Program { }
