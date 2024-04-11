using System.CommandLine;
using DbSeeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Stripe;
using UnAd.Data.Users;

static IHostBuilder CreateDbHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
            config.AddUserSecrets<Program>()
            .AddEnvironmentVariables())
        .UseConsoleLifetime()
        .ConfigureServices((hostContext, services) => {
            var config = hostContext.Configuration;
            services.AddLogging(configure => configure.AddConsole());
            services.AddDbContext<UserDbContext>((c, o) =>
                o.UseNpgsql(config.GetConnectionString(AppConfiguration.ConnectionStrings.UserDb), o => {
                    o.EnableRetryOnFailure(3);
                    o.CommandTimeout(30);
                }));

            services.AddHostedService<DbSeedService>();
        });

static IHostBuilder CreateRedisHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
            config.AddUserSecrets<Program>()
            .AddEnvironmentVariables())
        .UseConsoleLifetime()
        .ConfigureServices((hostContext, services) => {
            var config = hostContext.Configuration;
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IConnectionMultiplexer>((s) =>
                ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(
                    config.GetRedisUrl())));
            services.AddSingleton<IStripeClient>(s =>
                new StripeClient(config.GetStripeApiKey()));

            services.AddHostedService<RedisSeedService>();
        });

var redisCommand = new Command("redis");
var dbCommand = new Command("db");

var rootCommand = new RootCommand("seed");
rootCommand.AddCommand(redisCommand);
rootCommand.AddCommand(dbCommand);

redisCommand.SetHandler(async () => await CreateRedisHostBuilder(args).Build().RunAsync());
dbCommand.SetHandler(async () => await CreateDbHostBuilder(args).Build().RunAsync());

await rootCommand.InvokeAsync(args);
