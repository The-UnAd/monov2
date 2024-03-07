using GraphQLGateway;
using HotChocolate.AspNetCore;
using HotChocolate.Stitching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polly;
using StackExchange.Redis;

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
        var token = c.HeaderValue.ToString()
            .Split(' ')
            .LastOrDefault();
        var jwt = redis.GetDatabase()
            .StringGet($"token:{token}");
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
    .ModifyRequestOptions(opt =>
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment())
    .InitializeOnStartup();

builder.Services.AddHealthChecks();

builder.Services.AddCognitoIdentity().AddAuthentication(o => {
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o => {
    o.Authority = builder.Configuration["COGNITO_AUTHORITY"];
    o.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = false,
        ValidateAudience = false,
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHeaderPropagation();

app.UseAuthorization();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGraphQL()
    .WithOptions(new GraphQLServerOptions {
        // Disable GraphQL IDE outside dev
        Tool = { Enable = app.Environment.IsDevelopment() }
    });

app.RunWithGraphQLCommands(args);



