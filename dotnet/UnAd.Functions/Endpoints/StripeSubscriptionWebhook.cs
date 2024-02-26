using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using Stripe;
using System.Globalization;
using Twilio.Rest.Api.V2010.Account;
using UnAd.Redis;

namespace UnAd.Functions.Endpoints;
public class StripeSubscriptionWebhook(IStripeClient stripeClient,
                                       IStripeVerifier stripeVerifier,
                                       IConnectionMultiplexer redis,
                                       ILogger<StripeSubscriptionWebhook> logger,
                                       IStringLocalizer<StripeSubscriptionWebhook> localizer,
                                       MixpanelClient mixpanelClient,
                                       IConfiguration config) {

    private readonly string _messageServiceSid = config.GetTwilioMessageServiceSid();
    private readonly string _stripeEndpointSecret = config.GetStripeSubscriptionEndpointSecret();
    private readonly string _stripePortalUrl = config.GetStripePortalUrl();
    private readonly string _clientLinkBaseUri = config.GetClientLinkBaseUri();

    private void SetThreadCulture(string phone) {
        var db = redis.GetDatabase();
        var clientLocale = db.GetClientHashValue(phone, "locale");
        var location = clientLocale.HasValue ? clientLocale.ToString() : "en-US";

        var culture = new CultureInfo(location);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
    }

    public async Task<IResult> Endpoint(HttpRequest request) {
        logger.LogDebug("Processing StripeSubscriptionWebhook");
        Event stripeEvent = default!;
        using var streamReader = new StreamReader(request.Body);
        var body = await streamReader.ReadToEndAsync();
        if (request.Headers.TryGetValue("stripe-signature", out var sig) &&
            !stripeVerifier.TryVerify(sig!, _stripeEndpointSecret, body, out stripeEvent)) {
            return Results.BadRequest(new {
                error = "Invalid Stripe signature"
            });
        }
        logger.LogDebug("Stripe Event Type: {Type}", stripeEvent?.Type);
        try {

            if (stripeEvent?.Type == Events.CustomerSubscriptionDeleted) {
                await UpdateClient(stripeEvent, "SubscriptionCancelled");
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionUpdated) {
                await HandleSubscriptionUpdated(stripeEvent);
            } else if (stripeEvent?.Type == Events.CheckoutSessionCompleted) {
                await HandleCheckoutSessionCompleted(stripeEvent);
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionCreated) {
                await HandleSubscriptionCreated(stripeEvent);
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionResumed) {
                await UpdateClient(stripeEvent, "SubscriptionResumed");
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionTrialWillEnd) {
                await UpdateClient(stripeEvent, "TrailEndsSoon");
            } else {
                logger.LogWarning("Unhandled event type: {Type}", stripeEvent?.Type);
            }

            return Results.Ok();
        } catch (Exception e) {
            logger.LogError("Stripe event {Type} unhandled", stripeEvent?.Type);
            return Results.Problem(e.Message);
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Stripe.Checkout.Session session) {
            logger.LogWarning("Could not find session in event data");
            return;
        }
        var id = session.ClientReferenceId;
        var subscriptionId = session.SubscriptionId;
        var db = redis.GetDatabase();
        var clientPhone = db.GetClientPhone(id!);
        if (!clientPhone.HasValue) {
            logger.LogWarning("Could not find client with id {id}", id);
            return;
        }
        SetThreadCulture(clientPhone.ToString());
        db.SetSubscriptionPhone(subscriptionId, clientPhone.ToString());

        // TODO: this is timing out for some reason
        db.SetClientHashValue(clientPhone.ToString(), "subscriptionId", subscriptionId);
        var clientName = db.GetClientHashValue(clientPhone.ToString(), "name");
        var customerService = new CustomerService(stripeClient);
        await customerService.UpdateAsync(session.CustomerId, new() {
            Description = clientName.ToString(),
            Phone = clientPhone.ToString(),
            Metadata = new() {
                { "id", id },
                { "businessName",  clientName.ToString() }
            },
        });
        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "subscriptionId", subscriptionId},
            }, clientPhone);
    }

    private async Task HandleSubscriptionCreated(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Subscription subscription) {
            logger.LogWarning("Could not find subscription in event data");
            return;
        }
        var db = redis.GetDatabase();
        var clientPhone = db.GetSubscriptionPhone(subscription.Id);
        if (!clientPhone.HasValue) {
            logger.LogWarning("Could not find subscription with id {subscriptionId}", subscription.Id);
            return;
        }
        var validPhone = clientPhone.ToString();
        SetThreadCulture(validPhone);
        db.SetClientHashValue(validPhone, "customerId", subscription.CustomerId);

        var productId = subscription.Items.Data[0].Plan.ProductId;
        var maxMessages = db.GetProductLimitValue(productId, "maxMessages");
        db.SetClientProductLimit(validPhone, "maxMessages", maxMessages);

        var clientId = db.GetClientHashValue(validPhone, "clientId");
        await MessageResource.CreateAsync(new CreateMessageOptions(validPhone) {
            MessagingServiceSid = _messageServiceSid,
            Body = localizer.GetStringWithReplacements("SubscriptionStartedMessage",
            new {
                shareLink = $"{_clientLinkBaseUri}/{clientId}",
                subLink = _stripePortalUrl,
                trialInfo = subscription.TrialEnd.HasValue ?
                    "\n\n" + localizer.GetStringWithReplacements("TrialInfo",
                        new {
                            endDate = $"{subscription.TrialEnd.Value:D}",
                        })
                    : string.Empty,
            })
        });
        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "subscriptionId", subscription.Id},
            }, validPhone);
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Subscription subscription) {
            logger.LogWarning("Could not find subscription in event data");
            return;
        }
        var db = redis.GetDatabase();
        var clientPhone = db.GetSubscriptionPhone(subscription.Id);
        if (!clientPhone.HasValue) {
            logger.LogWarning("Could not find subscription with id {subscriptionId}", subscription.Id);
            return;
        }
        var validPhone = clientPhone.ToString();
        SetThreadCulture(validPhone);
        db.SetClientHashValue(validPhone, "customerId", subscription.CustomerId);

        var productId = subscription.Items.Data[0].Plan.ProductId;
        // TODO: store the product id in the client's subscription data
        // so we don't have to look it up later from Stripe
        var maxMessages = db.GetProductLimitValue(productId, "maxMessages");
        db.SetClientProductLimit(validPhone, "maxMessages", maxMessages);

        var clientId = db.GetClientHashValue(validPhone, "clientId");
        await MessageResource.CreateAsync(new CreateMessageOptions(validPhone) {
            MessagingServiceSid = _messageServiceSid,
            Body = localizer.GetStringWithReplacements("SubscriptionStartedMessage",
            new {
                shareLink = $"{_clientLinkBaseUri}/{clientId}",
                subLink = _stripePortalUrl,
                trialInfo = subscription.TrialEnd.HasValue ?
                    "\n\n" + localizer.GetStringWithReplacements("TrialInfo",
                        new {
                            endDate = $"{subscription.TrialEnd.Value:D}",
                        })
                    : string.Empty,
            })
        });
        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "subscriptionId", subscription.Id},
            }, validPhone);
    }

    private async Task UpdateClient(Event stripeEvent, string resourceName, object? replacements = default) {
        var subscription = stripeEvent.Data.Object as Subscription;
        var subscriptionId = subscription?.Id;
        var db = redis.GetDatabase();
        var clientPhone = db.GetSubscriptionPhone(subscriptionId!);
        if (!clientPhone.HasValue) {
            logger.LogWarning("Could not find subscription {subscriptionId}", subscriptionId);
            return;
        }
        SetThreadCulture(clientPhone.ToString());
        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "subscriptionId", subscriptionId! },
            }, clientPhone);
        await MessageResource.CreateAsync(new CreateMessageOptions(clientPhone.ToString()) {
            MessagingServiceSid = _messageServiceSid,
            Body = localizer.GetStringWithReplacements(resourceName, replacements ?? new { }),
        });
    }
}