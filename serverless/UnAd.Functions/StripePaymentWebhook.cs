using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Annotations;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using Stripe;
using System.Globalization;
using System.Text.Json;
using UnAd.Redis;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Net;

namespace UnAd.Functions;

public class StripePaymentWebhook(IStripeClient stripeClient,
                                  IStripeVerifier stripeVerifier,
                                  IConnectionMultiplexer redis,
                                  IMessageSender messageSender,
                                  IStringLocalizer<StripePaymentWebhook> localizer,
                                  IConfiguration config) {

    private readonly string _stripePortalUrl = config.GetStripePortalUrl();
    private readonly string _stripeEndpointSecret = config.GetStripePaymentEndpointSecret();

    private void SetThreadCulture(string phone) {
        var db = redis.GetDatabase();
        var clientLocale = db.GetClientHashValue(phone, "locale");
        var location = clientLocale.HasValue ? clientLocale.ToString() : "en-US";

        CultureInfo.CurrentCulture
            = CultureInfo.CurrentUICulture
            = new CultureInfo(location);
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/StripePaymentWebhook")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> Run(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
        context.Logger.LogLine("Processing StripePaymentWebhook");
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

            if (stripeEvent?.Type == Events.InvoicePaid) {
                await HandleInvoicePaid(stripeEvent, context.Logger);
            } else if (stripeEvent?.Type == Events.InvoicePaymentFailed) {
                await HandleInvoicePaymentFailed(stripeEvent, context.Logger);
            } else {
                context.Logger.LogLine($"Unhandled event type: {stripeEvent?.Type}");
            }

            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.OK
            };
        } catch (Exception e) {
            context.Logger.LogError($"Stripe event {stripeEvent?.Type} unhandled");
            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonSerializer.Serialize(new {
                    error = e.Message
                }),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }

    private async Task HandleInvoicePaid(Event stripeEvent, ILambdaLogger logger) {
        if (stripeEvent.Data.Object is not Invoice invoice) {
            logger.LogWarning("Could not find product in event data");
            return;
        }

        if (string.IsNullOrEmpty(invoice.SubscriptionId)) {
            logger.LogWarning($"No subscription found on invoice {invoice.Id}");
            return;
        }

        if (invoice.BillingReason == "subscription_create") {
            logger.LogWarning($"Invoice {invoice.Id} is a new subscription; skipping");
            return;
        }

        var db = redis.GetDatabase();
        var clientPhone = db.GetSubscriptionPhone(invoice.SubscriptionId);

        if (!clientPhone.HasValue) {
            logger.LogWarning($"No subscription found for phon number {clientPhone}");
            return;
        }
        var validPhone = clientPhone.ToString();

        // TODO: check if we can get the product ID from the invoice
        var subscription = await new SubscriptionService(stripeClient).GetAsync(invoice.SubscriptionId);

        var productId = subscription.Items.Data[0].Plan.ProductId;
        var maxMessages = db.GetProductLimitValue(productId, "maxMessages");

        db.SetClientProductLimit(validPhone, "maxMessages", maxMessages);

        if (!clientPhone.HasValue) {
            logger.LogWarning($"No client found with subscription {invoice.SubscriptionId}");
            return;
        }

        SetThreadCulture(validPhone);
        await messageSender.Send(clientPhone.ToString(), localizer.GetStringWithReplacements("InvoicePaid", new {
            portalUrl = _stripePortalUrl
        }));
    }

    private async Task HandleInvoicePaymentFailed(Event stripeEvent, ILambdaLogger logger) {
        if (stripeEvent.Data.Object is not Invoice invoice) {
            logger.LogWarning("Could not find product in event data");
            return;
        }

        if (string.IsNullOrEmpty(invoice.SubscriptionId)) {
            logger.LogWarning($"No subscription found on invoice {invoice.Id}");
            return;
        }

        var db = redis.GetDatabase();
        var clientPhone = db.GetSubscriptionPhone(invoice.SubscriptionId);


        SetThreadCulture(clientPhone.ToString());
        await messageSender.Send(clientPhone.ToString(), localizer.GetStringWithReplacements("InvoicePaymentFailed", new {
            portalUrl = _stripePortalUrl
        }));
    }
}
