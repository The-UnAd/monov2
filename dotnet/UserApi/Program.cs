using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using StackExchange.Redis;
using Stripe;
using Twilio;
using UnAd.Data.Users;
using UserApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(b => b.FormatterName = ConsoleFormatterNames.Systemd)
    .AddDebug();

builder.Services.AddTransient<IConnectionMultiplexer>(c =>
    ConnectionMultiplexer.Connect(c.GetRequiredService<IConfiguration>()["REDIS_URL"]!));

builder.Services.AddPooledDbContextFactory<UserDbContext>((s, o) =>
    o.UseNpgsql(s.GetRequiredService<IConfiguration>().GetConnectionString("UserDb")));

builder.Services.AddSingleton<IStripeClient>(s =>
    new StripeClient(s.GetRequiredService<IConfiguration>().GetValue<string>("STRIPE_API_KEY")));

TwilioClient.Init(builder.Configuration.GetTwilioAccountSid(),
       builder.Configuration.GetTwilioAuthToken());

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<QueryType>()
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
    .ModifyRequestOptions(opt =>
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment())
    .InitializeOnStartup();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHealthChecks("/health");

app.MapGraphQL();

app.RunWithGraphQLCommands(args);

public partial class Program {
    private static readonly Action<ILogger, string, Exception?> LogProgramException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "UnhandledException"), "Unexpected Error: {Message}");
}



