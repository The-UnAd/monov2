using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using Stripe;
using UnAd.Data.Users;
using UnAd.Redis;

namespace UnAd.Functions;
public class StripePaymentWebhook(IStripeClient stripeClient,
                                  IStripeVerifier stripeVerifier,
                                  IConnectionMultiplexer redis,
                                  IDbContextFactory<UserDbContext> dbFactory,
                                  IMessageSender messageSender,
                                  IStringLocalizer<StripePaymentWebhook> localizer,
                                  ILogger<StripePaymentWebhook> logger,
                                  IMixpanelClient mixpanelClient,
                                  IConfiguration config) {

    private readonly string _stripePortalUrl = config.GetStripePortalUrl();
    private readonly string _stripeEndpointSecret = config.GetStripePaymentEndpointSecret();

    private void SetThreadCulture(string phone, string? culture) {
        if (culture is not null) {
            CultureInfo.CurrentCulture
                = CultureInfo.CurrentUICulture
                = new CultureInfo(culture);
            return;
        }
        using var context = dbFactory.CreateDbContext();
        var client = context.Clients.FirstOrDefault(c => c.PhoneNumber == phone);
        var location = client?.Locale ?? "en-US";

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

        await using var context = await dbFactory.CreateDbContextAsync();
        var client = context.Clients.FirstOrDefault(c => c.CustomerId == invoice.CustomerId);

        if (client is null) {
            logger.LogWarning($"No client found for customerId {invoice.CustomerId}");
            return;
        }

        // TODO: check if we can get the product ID from the invoice
        var subscription = await new SubscriptionService(stripeClient).GetAsync(invoice.SubscriptionId);

        var priceId = subscription?.Items?.FirstOrDefault()?.Price.Id;
        if (priceId is null) {
            logger.LogWarning($"No price found for subscription {subscription.Id}");
            return;
        }
        var db = redis.GetDatabase();
        var maxMessages = db.GetPriceLimitValue(priceId, "maxMessages");

        db.SetClientPriceLimit(client.PhoneNumber, "maxMessages", maxMessages);

        SetThreadCulture(client.PhoneNumber, client.Locale);
        await messageSender.Send(client.PhoneNumber, localizer.GetStringWithReplacements("InvoicePaid", new {
            portalUrl = _stripePortalUrl
        }));

        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "invoiceId", invoice.Id },
            }, client.PhoneNumber);
    }

    private async Task HandleInvoicePaymentFailed(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Invoice invoice) {
            CouldNotParse(logger, "Invoice", null);
            return;
        }

        await using var context = await dbFactory.CreateDbContextAsync();
        var client = context.Clients.FirstOrDefault(c => c.CustomerId == invoice.CustomerId);

        if (client is null) {
            logger.LogWarning($"No client found for customerId {invoice.CustomerId}");
            return;
        }

        SetThreadCulture(client.PhoneNumber, client.Locale);
        await messageSender.Send(client.PhoneNumber, localizer.GetStringWithReplacements("InvoicePaymentFailed", new {
            portalUrl = _stripePortalUrl
        }));

        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "invoiceId", invoice.Id },
            }, client.PhoneNumber);
    }


    private static readonly Action<ILogger<StripePaymentWebhook>, string, Exception?> CouldNotParse =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(500, nameof(CouldNotParse)), "No {Type} data found in event.");
}
