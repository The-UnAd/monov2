using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Stripe;
using Twilio;
using UnAd.Data.Users;
using UserApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .ClearProviders()
    .AddConsole(b => b.FormatterName = ConsoleFormatterNames.Systemd)
    .AddDebug();

builder.Services.AddTransient<IConnectionMultiplexer>(c =>
    ConnectionMultiplexer.Connect(c.GetRequiredService<IConfiguration>().GetRedisUrl()));

builder.Services.AddPooledDbContextFactory<UserDbContext>((s, o) =>
    o.UseNpgsql(s.GetRequiredService<IConfiguration>().GetConnectionString("UserDb")));

builder.Services.AddSingleton<IStripeClient>(s =>
    new StripeClient(s.GetRequiredService<IConfiguration>().GetStripeApiKey()));

builder.Services.AddSingleton(() => {
    TwilioClient.Init(builder.Configuration.GetTwilioAccountSid(),
       builder.Configuration.GetTwilioAuthToken());
    return TwilioClient.GetRestClient();
});

builder.Services.AddSingleton<IMessageSender, MessageSender>();

if (!builder.Environment.IsDevelopment()) {
    builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.Authority = builder.Configuration["Cognito:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false
        };
    });
    builder.Services.AddAuthorization();
}

builder.Services
    .AddGraphQLServer()
    .AddAuthorization(o => {
        if (!builder.Environment.IsDevelopment()) {
            o.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        } else {
            o.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        }
    })
    .AddQueryType<QueryType>()
    .AddDiagnosticEventListener<LoggerExecutionEventListener>()
    .AddFiltering()
    .AddProjections()
    .AddSorting()
    .AddMutationType<MutationType>()
    .AddMutationConventions()
    .AddTypeExtension<ClientTypeExtensions>()
    .AddGlobalObjectIdentification()
    .RegisterDbContext<UserDbContext>(DbContextKind.Pooled)
    .RegisterService<IConnectionMultiplexer>()
    .RegisterService<IStripeClient>()
    .RegisterService<IMessageSender>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .InitializeOnStartup();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHealthChecks("/health");

if (!builder.Environment.IsDevelopment()) {
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGraphQL()
    .RequireAuthorization();

IdentityModelEventSource.ShowPII = app.Environment.IsDevelopment();

app.RunWithGraphQLCommands(args);

public partial class Program { }



