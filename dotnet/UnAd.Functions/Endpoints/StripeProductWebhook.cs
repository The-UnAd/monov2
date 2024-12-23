using System.Threading;
using StackExchange.Redis;
using Stripe;
using UnAd.Redis;

namespace UnAd.Functions;

public class StripeProductWebhook(IConnectionMultiplexer redis,
                                  IStripeVerifier stripeVerifier,
                                  IStripeClient stripeClient,
                                  IConfiguration config,
                                  ILogger<StripeProductWebhook> logger) {

    private readonly string _stripeEndpointSecret = config.GetStripePaymentEndpointSecret();

    public async Task<IResult> Endpoint(HttpRequest request) {
        Event stripeEvent = default!;
        using var streamReader = new StreamReader(request.Body);
        var body = await streamReader.ReadToEndAsync();
        if (request.Headers.TryGetValue("stripe-signature", out var sig) &&
            !stripeVerifier.TryVerify(sig!, _stripeEndpointSecret, body, out stripeEvent)) {
            return Results.BadRequest(new {
                error = "Invalid Stripe signature"
            });
        }
        try {

            if (stripeEvent?.Type == Events.ProductCreated) {
                HandleProductCreatedEvent(stripeEvent);
            } else if (stripeEvent?.Type == Events.ProductUpdated) {
                HandleProductUpdatedEvent(stripeEvent);
            } else if (stripeEvent?.Type == Events.ProductDeleted) {
                HandleProductDeletedEvent(stripeEvent);
            } else {
                logger.LogWarning($"Unhandled event type: {stripeEvent?.Type}");
            }

            return Results.Ok();
        } catch (StripeException e) {
            logger.LogError($"Stripe Exception: {e}");
            return Results.Problem(e.Message);
        }
    }

    private void HandleProductCreatedEvent(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return;
        }

        var db = redis.GetDatabase();
        var priceService = new PriceService(stripeClient);
        var prices = priceService.List(new PriceListOptions {
            Product = product.Id
        });
        foreach (var price in prices) {
            db.StorePrice(price.Id, product.Name, product.Description);
            db.SetPriceLimits(price.Id, price.Metadata);
        }
    }

    private void HandleProductUpdatedEvent(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return;
        }
        var db = redis.GetDatabase();
        var priceService = new PriceService(stripeClient);
        var prices = priceService.List(new PriceListOptions {
            Product = product.Id
        });
        foreach (var price in prices) {
            db.StorePrice(price.Id, product.Name, product.Description);
            db.SetPriceLimits(price.Id, price.Metadata);
        }
    }

    private void HandleProductDeletedEvent(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return;
        }
        // TODO: delete product from redis
        return;
    }
}
