using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Stripe;
using UnAd.Data.Users;
using UnAd.Functions;
using UnAd.Functions.Endpoints;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration
    .AddUserSecrets(typeof(Program).Assembly)
    .AddEnvironmentVariables();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.AddSingleton<IConnectionMultiplexer>((s) =>
    ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(
        builder.Configuration.GetRedisUrl())));
builder.Services.AddSingleton<IStripeClient>(s =>
    new StripeClient(builder.Configuration.GetStripeApiKey()));
builder.Services.AddLocalization(o => o.ResourcesPath = AppConfiguration.Keys.ResourcesPath);
builder.Services.AddExceptionHandler(o => o.ExceptionHandler = context => {
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = context.Features.Get<ILogger<Program>>();
    if (exception is null) {
        return Task.CompletedTask;
    }

    logger?.LogException(exception);
    return Task.CompletedTask;
});
builder.Services.AddTransient<MixpanelClient>();
builder.Services.AddHttpClient(AppConfiguration.Keys.MixpanelHttpClient, (s, c) => {
    c.BaseAddress = new Uri("https://api.mixpanel.com");
    c.DefaultRequestHeaders.Add("Accept", "text/plain");
    c.DefaultRequestHeaders.Add("User-Agent",
        typeof(Program)?.Assembly.GetName().Name);
    c.DefaultRequestHeaders.Add("Authorization",
        $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(
            builder.Configuration.GetMixPanelToken()))}");
});
builder.Services.Configure<MixpanelOptions>(
            builder.Configuration.GetSection(nameof(MixpanelOptions)));
builder.Services.AddSingleton<IStripeClient>(s =>
    new StripeClient(builder.Configuration.GetStripeApiKey()));
builder.Services.AddTransient<IStripeVerifier, StripeVerifier>();
builder.Services.AddTransient<MessageHelper>();
builder.Services.AddSingleton<IMessageSender, MessageSender>();
builder.Services.AddPooledDbContextFactory<UserDbContext>((c, o) =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("UserDb"))
        .UseModel(UnAd.Data.UserDbContextModel.Instance));

builder.Services.AddTransient<MessageHandler>();
builder.Services.AddTransient<StripePaymentWebhook>();
builder.Services.AddTransient<StripeProductWebhook>();
builder.Services.AddTransient<StripeSubscriptionWebhook>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Use((context, next) => {
    if (context.Request.Query.TryGetValue("code", out var value) &&
        value == context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("API_KEY")) {
        return next(context);
    }
    context.Response.StatusCode = 401;
    context.Response.CompleteAsync().ConfigureAwait(false);
    return next(context);
});

var api = app.MapGroup("/api");
api.MapPost("/MessageHandler", async (MessageHandler handler, HttpContext context) =>
    await handler.Endpoint(context.Request));
api.MapPost("/StripePaymentWebhook", async (StripePaymentWebhook handler, HttpContext context) =>
    await handler.Endpoint(context.Request));
api.MapPost("/StripeProductWebhook", async (StripeProductWebhook handler, HttpContext context) =>
    await handler.Endpoint(context.Request));
api.MapPost("/StripeSubscriptionWebhook", async (StripeSubscriptionWebhook handler, HttpContext context) =>
    await handler.Endpoint(context.Request));

app.Run();

internal partial class Program { }

[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }