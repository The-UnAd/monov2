using System.CommandLine;
using Confluent.Kafka;
using KafkaConsumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using UnAd.Data.Products;

static IHostBuilder CreateBaseHost(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
            config
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables())
        .UseConsoleLifetime();

static IHostBuilder CreateSubscriptionHostBuilder(string[] args) =>
    CreateBaseHost(args)
        .ConfigureServices((hostContext, services) => {
            var config = hostContext.Configuration;
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IConnectionMultiplexer>((s) =>
                ConnectionMultiplexer.Connect(
                    ConfigurationOptions.Parse(config.GetRedisUrl())));
            services.AddDbContext<ProductDbContext>((c, o) =>
                o.UseNpgsql(config.GetConnectionString(AppConfiguration.ConnectionStrings.ProductDb), o => {
                    o.EnableRetryOnFailure(3);
                    o.CommandTimeout(30);
                }));
            services.AddSingleton(sp =>
                new ProducerBuilder<string, string>(
                    new ProducerConfig {
                        BootstrapServers = config.GetKafkaBrokerList(),
                        MessageTimeoutMs = 1000
                    }).Build());
            services.AddSingleton(sp =>
                new ConsumerBuilder<string, string>(
                    new ConsumerConfig {
                        GroupId = "subscriptions",
                        BootstrapServers = config.GetKafkaBrokerList(),
                        AutoOffsetReset = AutoOffsetReset.Earliest,
                        AllowAutoCreateTopics = true,
                    }).Build());
            services.AddHostedService<SubscriptionHandler>();
            services.AddUnAdClient()
                .ConfigureHttpClient((sp, client) => {
                    var config = sp.GetRequiredService<IConfiguration>();
                    client.BaseAddress = new Uri(config.GetGraphQLApiUrl());
                });
        });

static IHostBuilder CreatePaymentHostBuilder(string[] args) =>
    CreateBaseHost(args)
        .ConfigureServices((hostContext, services) => {
            var config = hostContext.Configuration;
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IConnectionMultiplexer>((s) =>
                ConnectionMultiplexer.Connect(
                    ConfigurationOptions.Parse(config.GetRedisUrl())));
            services.AddSingleton(sp =>
                new ProducerBuilder<string, string>(
                    new ProducerConfig {
                        BootstrapServers = config.GetKafkaBrokerList(),
                        MessageTimeoutMs = 1000
                    }).Build());
            services.AddSingleton(sp =>
                new ConsumerBuilder<string, string>(
                    new ConsumerConfig {
                        GroupId = "payments",
                        BootstrapServers = config.GetKafkaBrokerList(),
                        AutoOffsetReset = AutoOffsetReset.Earliest,
                        AllowAutoCreateTopics = true,
                    }).Build());

            services.AddHostedService<PaymentHandler>();
            services.AddUnAdClient()
                .ConfigureHttpClient((sp, client) => {
                    var config = sp.GetRequiredService<IConfiguration>();
                    client.BaseAddress = new Uri(config.GetGraphQLApiUrl());
                });
        });

var paymentsCmd = new Command("payments");
var subscriptionsCmd = new Command("subscriptions");

var rootCommand = new RootCommand("listen");
rootCommand.AddCommand(paymentsCmd);
rootCommand.AddCommand(subscriptionsCmd);

paymentsCmd.SetHandler(async () => await CreatePaymentHostBuilder(args).Build().RunAsync());
subscriptionsCmd.SetHandler(async () => await CreateSubscriptionHostBuilder(args).Build().RunAsync());

await rootCommand.InvokeAsync(args);
