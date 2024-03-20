using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Stripe;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using UnAd.Redis;

namespace DbSeeder;

internal class DbSeedService(ILogger<DbSeedService> logger,
                           StripeClient stripe,
                           IConnectionMultiplexer redis,
                           UserDbContext userDbContext,
                           IHostApplicationLifetime appLifetime) : IHostedService {

    public async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogServiceStarting();

        await StoreProducts(cancellationToken);

        await StoreDefaultClient(cancellationToken);

        appLifetime.StopApplication();
    }

    private async Task StoreDefaultClient(CancellationToken cancellationToken) {
        var defaultClient = userDbContext.Clients.Where(c => c.PhoneNumber == "+15555555555").FirstOrDefault();
        if (defaultClient is null) {
            var newClient = userDbContext.Clients.Add(new Client {
                Locale = "en-US",
                Name = "UnAd",
                PhoneNumber = "+15555555555",
            });
            await userDbContext.SaveChangesAsync(cancellationToken);
            logger.LogStoredDefaultClient(newClient.Entity.Id);
        }
    }

    private async Task StoreProducts(CancellationToken cancellationToken) {
        var productService = new ProductService(stripe);

        var products = await productService.ListAsync(new ProductListOptions {
            Limit = 10, // TODO: how do I decide how many to get?
            Active = true,
            Type = "service"
        }, cancellationToken: cancellationToken);

        var db = redis.GetDatabase();
        logger.LogConnectedToRedis(redis.GetEndPoints().First());
        foreach (var product in products) {
            db.StoreProduct(product.Id, product.Name, product.Description);
            db.SetProductLimits(product.Id, product.Metadata);
            logger.LogStoredProduct(product.Id, product.Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogServiceStopping();
        return Task.CompletedTask;
    }
}

internal static class DbSeedServiceLogs {
    public static class EventIds {
        public const int ServiceStopping = 100;
        public const int ServiceStarting = 101;
        public const int ConnectedToRedis = 200;
        public const int StoredProduct = 201;
        public const int StoredDefaultClient = 300;
    }

    public static readonly Action<ILogger<DbSeedService>, Exception?> ServiceStopping =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStopping, nameof(ServiceStopping)), "DbSeedService is stopping.");
    public static readonly Action<ILogger<DbSeedService>, Exception?> ServiceStarting =
        LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStarting, nameof(ServiceStarting)), "DbSeedService is starting.");

    public static readonly Action<ILogger<DbSeedService>, System.Net.EndPoint, Exception?> ConnectedToRedis =
        LoggerMessage.Define<System.Net.EndPoint>(LogLevel.Information, new EventId(EventIds.ConnectedToRedis, nameof(ConnectedToRedis)), "Connected to Redis at {Endpoint}");
    public static readonly Action<ILogger<DbSeedService>, string, string, Exception?> StoredProduct =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(EventIds.StoredProduct, nameof(StoredProduct)), "Stored product {Id}: {Name}");

    public static readonly Action<ILogger<DbSeedService>, Guid, Exception?> StoredDefaultClient =
        LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(EventIds.StoredDefaultClient, nameof(StoredDefaultClient)), "Stored default client with Id {Id}");

    public static void LogServiceStopping(this ILogger<DbSeedService> logger, Exception? ex = null) => ServiceStopping(logger, ex);
    public static void LogServiceStarting(this ILogger<DbSeedService> logger, Exception? ex = null) => ServiceStarting(logger, ex);
    public static void LogConnectedToRedis(this ILogger<DbSeedService> logger, System.Net.EndPoint endpoint, Exception? ex = null) => ConnectedToRedis(logger, endpoint, ex);
    public static void LogStoredProduct(this ILogger<DbSeedService> logger, string id, string name, Exception? ex = null) => StoredProduct(logger, id, name, ex);
    public static void LogStoredDefaultClient(this ILogger<DbSeedService> logger, Guid id, Exception? ex = null) => StoredDefaultClient(logger, id, ex);
}
