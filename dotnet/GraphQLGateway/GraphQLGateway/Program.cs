using GraphQLGateway;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution.Serialization;
using HotChocolate.Stitching;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging.Console;
using Polly;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(b => b.FormatterName = ConsoleFormatterNames.Systemd)
    .AddDebug();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(typeof(ILogger), c => {
    var logger = c.GetRequiredService<ILoggerFactory>()
        .CreateLogger("GraphQLGateway");
    return logger;
});

builder.Services.AddExceptionHandler(o => o.ExceptionHandler = context => {
    var exception = context.Features.Get<IExceptionHandlerFeature>()
        ?.Error;
    var logger = context.Features.Get<ILogger>();
    logger?.LogError(exception, "Unhandled exception {Message}", exception?.Message);
    return Task.CompletedTask;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHeaderPropagation(o => {
    o.Headers.Add("Authorization", "X-Forwarded-Token", c =>
        c.HeaderValue.ToString()
            .Split(' ')
            .LastOrDefault());
    //o.Headers.Add("Authorization", "Authorization", c => {
    //    var redis = c.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
    //    var token = c.HeaderValue.ToString()
    //        .Split(' ')
    //        .LastOrDefault();
    //    var jwt = redis.GetDatabase()
    //        .StringGet($"token:{token}");
    //    return $"Bearer {jwt}";
    //});
});
builder.Services.AddHttpClient("Fusion")
    .AddHeaderPropagation()
    .AddTransientHttpErrorPolicy(b
        => b.WaitAndRetryAsync(3, i
            => TimeSpan.FromSeconds(Math.Pow(2, i))));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>()["REDIS_URL"]!));

builder.Services
    .AddFusionGatewayServer()
    .ConfigureFromFile("./gateway.fgp")
    .CoreBuilder
    .AddDirectiveType(typeof(DelegateDirectiveType))
    .AddTypeExtension<QueryTypeExtension>()
    .ModifyRequestOptions(opt =>
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment())
    .InitializeOnStartup();


builder.Services.AddHealthChecks();

builder.Services.AddHttpResponseFormatter(new HttpResponseFormatterOptions {
    Json = new JsonResultFormatterOptions {
        NullIgnoreCondition = JsonNullIgnoreCondition.Fields
    }
});

var app = builder.Build();

app.UseHeaderPropagation();
app.UseExceptionHandler();

app.MapHealthChecks("/health");

app.MapGraphQL()
    .WithOptions(new GraphQLServerOptions {
        // Disable GraphQL IDE outside dev
        Tool = { Enable = app.Environment.IsDevelopment() }
    });

app.RunWithGraphQLCommands(args);



