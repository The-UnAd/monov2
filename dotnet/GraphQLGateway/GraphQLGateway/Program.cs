using GraphQLGateway;
using HotChocolate.AspNetCore;
using HotChocolate.Stitching;
using Polly;
using StackExchange.Redis;
using UnAd.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets(typeof(Program).Assembly);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHeaderPropagation(o => {
    o.Headers.Add("Authorization", "X-Forwarded-Token", c =>
        c.HeaderValue.ToString()
            .Split(' ')
            .LastOrDefault());
    o.Headers.Add("Authorization", "Authorization", c => {
        var redis = c.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var tokenId = c.HeaderValue.ToString()
            .Split(' ')
            .LastOrDefault() ?? string.Empty;
        var jwt = redis.GetDatabase().GetUserToken(tokenId);
        /*
         * TODO: here's the problem:
         * When this token is missing or invalid, the delegated API will return a 401.
         * This apparently borks the whole process, and the gateway will return a 200, but with errors.
         */
        return $"Bearer {jwt}";
    });
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
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .InitializeOnStartup();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHeaderPropagation();

app.MapHealthChecks("/health");

app.MapGraphQL()
    .WithOptions(new GraphQLServerOptions {
        // Disable GraphQL IDE outside dev
        Tool = { Enable = app.Environment.IsDevelopment() }
    });

app.RunWithGraphQLCommands(args);



