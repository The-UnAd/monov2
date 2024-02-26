using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using Stripe;
using System.Globalization;
using UnAd.Redis;

namespace UnAd.Functions.Endpoints;
public class StripePaymentWebhook(IStripeClient stripeClient,
                                  IStripeVerifier stripeVerifier,
                                  IConnectionMultiplexer redis,
                                  IMessageSender messageSender,
                                  IStringLocalizer<StripePaymentWebhook> localizer,
                                  ILogger<StripePaymentWebhook> logger,
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

            if (stripeEvent!.Type == Events.InvoicePaid) {
                await HandleInvoicePaid(stripeEvent);
            } else if (stripeEvent!.Type == Events.InvoicePaymentFailed) {
                await HandleInvoicePaymentFailed(stripeEvent);
            } else {
                logger.LogUnhandledType(stripeEvent!.Type);
            }

            return Results.Ok();
        } catch (Exception e) {
            logger.LogException(e);
            return Results.Problem(e.Message);
        }
    }

    private async Task HandleInvoicePaid(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Invoice invoice) {
            CouldNotParse(logger, "Invoice", null);
            return;
        }

        if (string.IsNullOrEmpty(invoice.SubscriptionId)) {
            logger.LogWarning($"No subscription found on invoice {invoice.Id}");
            return;
        }

        if (invoice.BillingReason == "subscription_create") {
            logger.LogDebug($"Invoice {invoice.Id} is a new subscription; skipping");
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

    private async Task HandleInvoicePaymentFailed(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Invoice invoice) {
            CouldNotParse(logger, "Invoice", null);
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


    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> CouldNotParse =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(500, nameof(CouldNotParse)), "No {Type} data found in event.");
}