using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Stripe;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) => {
            config.AddUserSecrets<Program>();
        })
        .UseConsoleLifetime()
        .ConfigureServices((hostContext, services) => {
            var config = hostContext.Configuration;
            services.AddLogging(configure => configure.AddConsole())
                    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
            services.AddSingleton<IConnectionMultiplexer>((s) =>
                ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(
                    config.GetRedisUrl())));
            services.AddSingleton(s => new StripeClient(config.GetStripeApiKey()));

            services.AddHostedService<DbSeedService>();
        });

await CreateHostBuilder(args).Build().RunAsync();
