using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using StackExchange.Redis;
using Stripe;
using System.Net;
using System.Text;
using System.Text.Json;
using UnAd.Redis;

namespace UnAd.Functions;
public class StripeProductWebhook(IStripeClient stripeClient,
                                       IConnectionMultiplexer redis,
                                       ILogger<StripeProductWebhook> logger,
                                       IConfiguration config) {

    private readonly string _stripeEndpointSecret = config.GetStripeProductEndpointSecret();

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/StripeProductWebhook")]
    public async Task<APIGatewayProxyResponse> Run(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
        context.Logger.LogLine("Processing Stripe Product Webhook");
        using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Body));
        using var streamReader = new StreamReader(bodyStream);
        var json = await streamReader.ReadToEndAsync();

        try {
            var stripeEvent = EventUtility.ConstructEvent(json,
                request.Headers.FirstOrDefault(h => h.Key == "stripe-signature").Value, _stripeEndpointSecret);
            logger.LogInformation("Stripe Event Type: {type}", stripeEvent.Type);

            if (stripeEvent.Type == Events.ProductCreated) {
                await HandleProductCreatedEvent(stripeEvent);
            } else if (stripeEvent.Type == Events.ProductUpdated) {
                await HandleProductUpdatedEvent(stripeEvent);
            } else if (stripeEvent.Type == Events.ProductDeleted) {
                await HandleProductDeletedEvent(stripeEvent);
            } else {
                logger.LogWarning("Unhandled event type: {type}", stripeEvent.Type);
            }

            return new APIGatewayProxyResponse {
                StatusCode = (int)HttpStatusCode.OK
            };
        } catch (StripeException e) {
            logger.LogError("Stripe Exception: {e}", e);

            return new APIGatewayProxyResponse {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = JsonSerializer.Serialize(e),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }

    private Task HandleProductCreatedEvent(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return Task.CompletedTask;
        }

        var db = redis.GetDatabase();
        db.StoreProduct(product.Id, product.Name, product.Description);
        db.SetProductLimits(product.Id, product.Metadata);
        return Task.CompletedTask;
    }

    private Task HandleProductUpdatedEvent(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return Task.CompletedTask;
        }
        var db = redis.GetDatabase();
        db.StoreProduct(product.Id, product.Name, product.Description);
        db.SetProductLimits(product.Id, product.Metadata);
        return Task.CompletedTask;
    }

    private Task HandleProductDeletedEvent(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Product product) {
            logger.LogWarning("Could not find product in event data");
            return Task.CompletedTask;
        }
        // TODO: delete product from redis
        return Task.CompletedTask;
    }
}
