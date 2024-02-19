//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
//using StackExchange.Redis;
//using Stripe;
//using System.Globalization;
//using System.Text.Json;
//using UnAd.Redis;

//namespace UnAd.Functions;

//public class StripePaymentWebhook(IStripeClient stripeClient,
//                                  IStripeVerifier stripeVerifier,
//                                  IConnectionMultiplexer redis,
//                                  IMessageSender messageSender,
//                                  ILogger<StripePaymentWebhook> logger,
//                                  IStringLocalizer<StripePaymentWebhook> localizer,
//                                  IConfiguration config) {

//    private readonly string _stripePortalUrl = config.GetStripePortalUrl();
//    private readonly string _stripeEndpointSecret = config.GetStripePaymentEndpointSecret();

//    private void SetThreadCulture(string phone) {
//        var db = redis.GetDatabase();
//        var clientLocale = db.GetClientHashValue(phone, "locale");
//        var location = clientLocale.HasValue ? clientLocale.ToString() : "en-US";

//        CultureInfo.CurrentCulture 
//            = CultureInfo.CurrentUICulture 
//            = new CultureInfo(location);
//    }

//    [Function("StripePaymentWebhook")]
//    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req) {
        
//        using var streamReader = new StreamReader(req.Body);
//        var json = await streamReader.ReadToEndAsync();
//        if (!stripeVerifier.TryVerify(
//            req.Headers.GetValues("stripe-signature").FirstOrDefault()!, _stripeEndpointSecret, json, out var stripeEvent)) {
//            var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
//            await response.WriteAsJsonAsync(new {
//                error = "Invalid Stripe signature"
//            });
//            return response;
//        }
//        logger.LogInformation("Stripe Event Type: {type}", stripeEvent.Type);

//        try {

//            if (stripeEvent.Type == Events.InvoicePaid) {
//                await HandleInvoicePaid(stripeEvent);
//            } else if (stripeEvent.Type == Events.InvoicePaymentFailed) {
//                await HandleInvoicePaymentFailed(stripeEvent);
//            } else {
//                logger.LogWarning("Unhandled event type: {type}", stripeEvent.Type);
//            }

//            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
//            return response;
//        } catch (Exception e) {
//            logger.LogError(e, "Stripe event {Type} unhandled", stripeEvent.Type);
//            var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
//            response.Headers.Add("Content-Type", "application/json");
//            response.Body.Write(JsonSerializer.SerializeToUtf8Bytes(e));
//            return response;
//        }
//    }

//    private async Task HandleInvoicePaid(Event stripeEvent) {
//        if (stripeEvent.Data.Object is not Invoice invoice) {
//            logger.LogWarning("Could not find product in event data");
//            return;
//        }

//        if (string.IsNullOrEmpty(invoice.SubscriptionId)) {
//            logger.LogWarning("No subscription found on invoice {Id}", invoice.Id);
//            return;
//        }

//        if (invoice.BillingReason == "subscription_create") {
//            logger.LogInformation("Invoice {Id} is a new subscription; skipping", invoice.Id);
//            return;
//        }

//        var db = redis.GetDatabase();
//        var clientPhone = db.GetSubscriptionPhone(invoice.SubscriptionId);

//        if (!clientPhone.HasValue) { 
//            logger.LogWarning("No subscription found for phon number {Phone}", clientPhone);
//            return;
//        }
//        var validPhone = clientPhone.ToString();

//        // TODO: check if we can get the product ID from the invoice
//        var subscription = await new SubscriptionService(stripeClient).GetAsync(invoice.SubscriptionId);

//        var productId = subscription.Items.Data[0].Plan.ProductId;
//        var maxMessages = db.GetProductLimitValue(productId, "maxMessages");

//        db.SetClientProductLimit(validPhone, "maxMessages", maxMessages);

//        if (!clientPhone.HasValue) {
//            logger.LogWarning("No client found with subscription {SubscriptionId}", invoice.SubscriptionId);
//            return;
//        }

//        SetThreadCulture(validPhone);
//        await messageSender.Send(clientPhone.ToString(), localizer.GetStringWithReplacements("InvoicePaid", new {
//            portalUrl = _stripePortalUrl
//        }));
//    }

//    private async Task HandleInvoicePaymentFailed(Event stripeEvent) {
//        if (stripeEvent.Data.Object is not Invoice invoice) {
//            logger.LogWarning("Could not find product in event data");
//            return;
//        }

//        if (string.IsNullOrEmpty(invoice.SubscriptionId)) {
//            logger.LogWarning("No subscription found on invoice {Id}", invoice.Id);
//            return;
//        }

//        var db = redis.GetDatabase();
//        var clientPhone = db.GetSubscriptionPhone(invoice.SubscriptionId);


//        SetThreadCulture(clientPhone.ToString());
//        await messageSender.Send(clientPhone.ToString(), localizer.GetStringWithReplacements("InvoicePaymentFailed", new {
//            portalUrl = _stripePortalUrl
//        }));
//    }
//}
