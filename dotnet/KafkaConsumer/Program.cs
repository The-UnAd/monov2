using System.CommandLine;
using Confluent.Kafka;
using KafkaConsumer;
using KafkaConsumer.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using UnAd.Kafka;

var paymentsCmd = new Command("payments");
var subscriptionsCmd = new Command("subscriptions");
var notificationsCmd = new Command("notifications");
var allCmd = new Command("all");

var rootCommand = new RootCommand("listen");
rootCommand.AddCommand(paymentsCmd);
rootCommand.AddCommand(subscriptionsCmd);
rootCommand.AddCommand(notificationsCmd);
rootCommand.AddCommand(allCmd);

paymentsCmd.SetHandler(async () => await CreateBaseHost(args).ConfigurePaymentsHandler().Build().RunAsync());
subscriptionsCmd.SetHandler(async () => await CreateBaseHost(args).ConfigureSubscriptionHandler().Build().RunAsync());
notificationsCmd.SetHandler(async () => await CreateBaseHost(args).ConfigureNotificationHandler().Build().RunAsync());

allCmd.SetHandler(async () =>
    await CreateBaseHost(args)
        .ConfigureNotificationHandler()
        .ConfigurePaymentsHandler()
        .ConfigureSubscriptionHandler()
        .Build().RunAsync());

await rootCommand.InvokeAsync(args);


static IHostBuilder CreateBaseHost(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
            config
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables())
        .UseConsoleLifetime()
        .ConfigureServices((hostContext, services) => {
            var config = hostContext.Configuration;
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IConnectionMultiplexer>((s) =>
                ConnectionMultiplexer.Connect(
                    ConfigurationOptions.Parse(config.GetRedisUrl())));
            services.AddUnAdClient()
                .ConfigureHttpClient((sp, client) => {
                    var config = sp.GetRequiredService<IConfiguration>();
                    client.BaseAddress = new Uri(config.GetGraphQLApiUrl());
                });
        });

internal static class HandlerBuilders {

    public static IHostBuilder ConfigureSubscriptionHandler(this IHostBuilder builder) =>
        builder
            .ConfigureServices((hostContext, services) => {
                var config = hostContext.Configuration;

                services.AddSingleton(sp =>
                    new ConsumerBuilder<string, string>(
                        new ConsumerConfig {
                            GroupId = "subscriptions",
                            BootstrapServers = config.GetKafkaBrokerList(),
                            AutoOffsetReset = AutoOffsetReset.Earliest,
                            AllowAutoCreateTopics = true,
                        }).Build());
                services.AddHostedService<SubscriptionHandler>();
            });


    public static IHostBuilder ConfigureNotificationHandler(this IHostBuilder builder) =>
        builder
            .ConfigureServices((hostContext, services) => {
                services.AddSingleton<INotificationConsumer>(sp =>
                    new NotificationConsumer(new ConsumerConfig {
                        GroupId = "notifications-handlers",
                        BootstrapServers = sp.GetRequiredService<IConfiguration>().GetKafkaBrokerList(),
                        AutoOffsetReset = AutoOffsetReset.Earliest,
                        AllowAutoCreateTopics = true,
                    }));

                services.AddHostedService<NotificationHandler>();
            });

    public static IHostBuilder ConfigurePaymentsHandler(this IHostBuilder builder) =>
        builder
            .ConfigureServices((hostContext, services) => {
                var config = hostContext.Configuration;

                services.AddSingleton(sp =>
                    new ConsumerBuilder<string, string>(
                        new ConsumerConfig {
                            GroupId = "payments",
                            BootstrapServers = config.GetKafkaBrokerList(),
                            AutoOffsetReset = AutoOffsetReset.Earliest,
                            AllowAutoCreateTopics = true,
                        }).Build());

                services.AddHostedService<PaymentHandler>();
            });
}
