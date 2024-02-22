using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using StackExchange.Redis;
using Stripe;
using System.Net;
using System.Text.Json;
using UnAd.Redis;

namespace UnAd.Functions;

public class StripeProductWebhook(IConnectionMultiplexer redis, StripeVerifier stripeVerifier, IConfiguration config) {
    private readonly string _stripeEndpointSecret = config.GetStripePaymentEndpointSecret();

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/StripeProductWebhook")]
    public APIGatewayHttpApiV2ProxyResponse Run(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
        context.Logger.LogLine("Processing StripeProductWebhook");
        Event stripeEvent = default!;
        if (request.Headers.TryGetValue("stripe-signature", out var sig) &&
            !stripeVerifier.TryVerify(sig, _stripeEndpointSecret, request.Body, out stripeEvent)) {
            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonSerializer.Serialize(new {
                    error = "Invalid Stripe signature"
                }),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" }
                }
            };
        }
        context.Logger.LogLine($"Stripe Event Type: {stripeEvent?.Type}");
        try {

            if (stripeEvent?.Type == Events.ProductCreated) {
                HandleProductCreatedEvent(stripeEvent, context.Logger);
            } else if (stripeEvent?.Type == Events.ProductUpdated) {
                HandleProductUpdatedEvent(stripeEvent, context.Logger);
            } else if (stripeEvent?.Type == Events.ProductDeleted) {
                HandleProductDeletedEvent(stripeEvent, context.Logger);
            } else {
                context.Logger.LogLine($"Unhandled event type: { stripeEvent?.Type}");
            }

            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.OK
            };
        } catch (StripeException e) {
            context.Logger.LogError($"Stripe Exception: {e}");

            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = JsonSerializer.Serialize(e),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }

    private void HandleProductCreatedEvent(Event stripeEvent, ILambdaLogger logger) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return;
        }

        var db = redis.GetDatabase();
        db.StoreProduct(product.Id, product.Name, product.Description);
        db.SetProductLimits(product.Id, product.Metadata);
        return;
    }

    private void HandleProductUpdatedEvent(Event stripeEvent, ILambdaLogger logger) { 
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return;
        }
        var db = redis.GetDatabase();
        db.StoreProduct(product.Id, product.Name, product.Description);
        db.SetProductLimits(product.Id, product.Metadata);
        return;
    }

    private void HandleProductDeletedEvent(Event stripeEvent, ILambdaLogger logger) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return;
        }
        // TODO: delete product from redis
        return;
    }
}
