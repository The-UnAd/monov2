using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Stripe;
using UnAd.Redis;

internal class DbSeedService(ILogger<DbSeedService> logger,
                           StripeClient stripe,
                           IConnectionMultiplexer redis,
                           IHostApplicationLifetime appLifetime) : IHostedService {

    public async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("DbSeedService is starting.");

        var productService = new ProductService(stripe);

        var products = await productService.ListAsync(new ProductListOptions {
            Limit = 10, // TODO: how do I decide how many to get?
            Active = true,
            Type = "service"
        }, cancellationToken: cancellationToken);

        var db = redis.GetDatabase();
        logger.LogInformation("Connected to Redis at {Endpoint}", redis.GetEndPoints().First());
        foreach (var product in products) {
            db.StoreProduct(product.Id, product.Name, product.Description);
            db.SetProductLimits(product.Id, product.Metadata);
            logger.LogInformation("Stored product {Id}: {Name}", product.Id, product.Name);
        }

        // TODO: seed the database with default users, etc

        appLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("DbSeedService is stopping.");

        return Task.CompletedTask;
    }

    
}
