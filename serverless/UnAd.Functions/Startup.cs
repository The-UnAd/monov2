using Amazon.Lambda.Annotations;
using Microsoft.AspNetCore.Diagnostics;
using StackExchange.Redis;
using Stripe;
using System.Reflection;
using System.Text;
using UnAd.Functions;

[LambdaStartup]
public class Startup {

    public Startup() {
        Configuration = new ConfigurationBuilder().AddEnvironmentVariables()
            .Build();
    }
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton(Configuration);
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
        services.AddSingleton<IConnectionMultiplexer>((s) =>
            ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(
                Configuration.GetRedisUrl())));
        services.AddSingleton<IStripeClient>(s =>
            new StripeClient(Configuration.GetStripeApiKey()));
        services.AddLocalization(options => {
            options.ResourcesPath = AppConfiguration.Keys.ResourcesPath;
        });
        services.AddExceptionHandler(o => o.ExceptionHandler = context => {
            var exception = context.Features.Get<IExceptionHandlerFeature>()
                ?.Error;
            var logger = context.Features.Get<ILogger>();
            logger?.LogError(exception, "Unhandled exception {Message}", exception?.Message);
            return Task.CompletedTask;
        });
        services.AddTransient<MixpanelClient>();
        services.AddHttpClient(AppConfiguration.Keys.MixpanelHttpClient, (s, c) => {
            c.BaseAddress = new Uri("https://api.mixpanel.com");
            c.DefaultRequestHeaders.Add("Accept", "text/plain");
            c.DefaultRequestHeaders.Add("User-Agent",
                Assembly.GetEntryAssembly()?.GetName().Name ?? "UnAd.Functions");
            c.DefaultRequestHeaders.Add("Authorization",
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    Configuration.GetMixPanelToken()))}");
        });
        services.Configure<MixpanelOptions>(
                    Configuration.GetSection(nameof(MixpanelOptions)));
        services.AddSingleton<IStripeClient>(s =>
            new StripeClient(Configuration.GetStripeApiKey()));
        services.AddTransient<IStripeVerifier, StripeVerifier>();
        services.AddTransient<MessageHelper>();
        services.AddSingleton<IMessageSender, MessageSender>();
    }
}