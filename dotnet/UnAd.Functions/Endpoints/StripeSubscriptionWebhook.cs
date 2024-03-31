using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using Stripe;
using Twilio.Rest.Api.V2010.Account;
using UnAd.Data.Users;
using UnAd.Redis;

namespace UnAd.Functions;
public class StripeSubscriptionWebhook(IStripeClient stripeClient,
                                       IStripeVerifier stripeVerifier,
                                       IConnectionMultiplexer redis,
                                       IDbContextFactory<UserDbContext> dbFactory,
                                       ILogger<StripeSubscriptionWebhook> logger,
                                       IStringLocalizer<StripeSubscriptionWebhook> localizer,
                                       MixpanelClient mixpanelClient,
                                       IConfiguration config) {

    private readonly string _messageServiceSid = config.GetTwilioMessageServiceSid();
    private readonly string _stripeEndpointSecret = config.GetStripeSubscriptionEndpointSecret();
    private readonly string _stripePortalUrl = config.GetStripePortalUrl();
    private readonly string _clientLinkBaseUri = config.GetClientLinkBaseUri();

    private static void SetThreadCulture(string culture) => CultureInfo.CurrentCulture
            = CultureInfo.CurrentUICulture
            = new CultureInfo(culture);

    public async Task<IResult> Endpoint(HttpRequest request) {
        Event? stripeEvent = default;
        using var streamReader = new StreamReader(request.Body);
        var body = await streamReader.ReadToEndAsync();
        if (request.Headers.TryGetValue("stripe-signature", out var sig) &&
            !stripeVerifier.TryVerify(sig!, _stripeEndpointSecret, body, out stripeEvent)) {
            return Results.BadRequest(new {
                error = "Invalid Stripe signature"
            });
        }

        try {
            if (stripeEvent?.Type == Events.CustomerSubscriptionDeleted) {
                await HandleSubscriptionUpdated(stripeEvent);
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionUpdated) {
                await HandleSubscriptionUpdated(stripeEvent);
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionResumed) {
                await HandleSubscriptionUpdated(stripeEvent);
            } else if (stripeEvent?.Type == Events.CheckoutSessionCompleted) {
                await HandleCheckoutSessionCompleted(stripeEvent);
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionCreated) {
                await HandleSubscriptionCreated(stripeEvent);
            } else if (stripeEvent?.Type == Events.CustomerSubscriptionTrialWillEnd) {
                await UpdateClient(stripeEvent, "TrailEndsSoon");
            } else {
                logger.LogUnhandledEvent(stripeEvent?.Type ?? "null");
            }

            return Results.Ok();
        } catch (Exception e) {
            logger.LogException(e);
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
        await using var context = await dbFactory.CreateDbContextAsync();
        var client = context.Clients.FirstOrDefault(c => c.Id == Guid.Parse(id));
        if (client is null) {
            logger.LogWarning("Could not find client with id {id}", id);
            return;
        }
        SetThreadCulture(client.Locale);
        client.SubscriptionId = subscriptionId;
        await context.SaveChangesAsync();
        var customerService = new CustomerService(stripeClient);
        await customerService.UpdateAsync(session.CustomerId, new() {
            Description = client.Name,
            Phone = client.PhoneNumber,
            Metadata = new() {
                { "id", id },
                { "businessName",  client.Name }
            },
        });
        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "subscriptionId", subscriptionId},
            }, client.PhoneNumber);
    }

    private async Task HandleSubscriptionCreated(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Subscription subscription) {
            logger.LogWarning("Could not find subscription in event data");
            return;
        }
        await using var context = await dbFactory.CreateDbContextAsync();
        var client = context.Clients.FirstOrDefault(c => c.SubscriptionId == subscription.Id);
        if (client is null) {
            logger.LogWarning("Could not find subscription with id {subscriptionId}", subscription.Id);
            return;
        }
        SetThreadCulture(client.Locale);
        client.CustomerId = subscription.CustomerId;
        client.SubscriptionId = subscription.Id;
        await context.SaveChangesAsync();

        var productId = subscription.Items.Data[0].Plan.ProductId;
        var db = redis.GetDatabase();
        var maxMessages = db.GetProductLimitValue(productId, "maxMessages");
        db.SetClientProductLimit(client.PhoneNumber, "maxMessages", maxMessages);

        await MessageResource.CreateAsync(new CreateMessageOptions(client.PhoneNumber) {
            MessagingServiceSid = _messageServiceSid,
            Body = localizer.GetStringWithReplacements("SubscriptionStartedMessage",
            new {
                shareLink = $"{_clientLinkBaseUri}/{client.Id}",
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
            }, client.PhoneNumber);
    }

    private async Task HandleSubscriptionResumed(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Subscription subscription) {
            logger.LogWarning("Could not find subscription in event data");
            return;
        }
        await using var context = await dbFactory.CreateDbContextAsync();
        var client = context.Clients.FirstOrDefault(c => c.CustomerId == subscription.CustomerId);
        if (client is null) {
            logger.LogWarning("Could not find subscription with id {subscriptionId}", subscription.Id);
            return;
        }
        SetThreadCulture(client.Locale);
        client.CustomerId = subscription.CustomerId;
        client.SubscriptionId = subscription.Id;
        await context.SaveChangesAsync();

        var productId = subscription.Items.Data[0].Plan.ProductId;
        var db = redis.GetDatabase();
        var maxMessages = db.GetProductLimitValue(productId, "maxMessages");
        db.SetClientProductLimit(client.PhoneNumber, "maxMessages", maxMessages);

        await MessageResource.CreateAsync(new CreateMessageOptions(client.PhoneNumber) {
            MessagingServiceSid = _messageServiceSid,
            Body = localizer.GetStringWithReplacements("SubscriptionStartedMessage",
            new {
                shareLink = $"{_clientLinkBaseUri}/{client.Id}",
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
            }, client.PhoneNumber);
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Subscription subscription) {
            logger.LogWarning("Could not find subscription in event data");
            return;
        }

        await using var context = await dbFactory.CreateDbContextAsync();
        var client = context.Clients.FirstOrDefault(c => c.CustomerId == subscription.CustomerId);
        if (client is null) {
            logger.LogWarning("Could not find customer with id {CustomerId}", subscription.CustomerId);
            return;
        }
        SetThreadCulture(client.Locale);

        var db = redis.GetDatabase();
        if (subscription.CancellationDetails is not null) {
            client.SubscriptionId = null;
            await context.SaveChangesAsync();
            db.SetClientProductLimit(client.PhoneNumber, "maxMessages", 0);
            await MessageResource.CreateAsync(new CreateMessageOptions(client.PhoneNumber) {
                MessagingServiceSid = _messageServiceSid,
                Body = localizer.GetStringWithReplacements("SubscriptionCanceledMessage", new {
                    subLink = _stripePortalUrl,
                })
            });
            await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "subscriptionId", subscription.Id},
            }, client.PhoneNumber);
            return;
        }

        var productId = subscription.Items.Data[0].Plan.ProductId;
        // TODO: store the product id in the client's subscription data
        // so we don't have to look it up later from Stripe
        var maxMessages = db.GetProductLimitValue(productId, "maxMessages");
        db.SetClientProductLimit(client.PhoneNumber, "maxMessages", maxMessages);

        await MessageResource.CreateAsync(new CreateMessageOptions(client.PhoneNumber) {
            MessagingServiceSid = _messageServiceSid,
            Body = localizer.GetStringWithReplacements("SubscriptionStartedMessage",
            new {
                shareLink = $"{_clientLinkBaseUri}/{client.Id}",
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
            }, client.PhoneNumber);
    }

    private async Task UpdateClient(Event stripeEvent, string resourceName, object? replacements = default) {
        if (stripeEvent.Data.Object is not Subscription subscription) {
            logger.LogWarning("Could not find subscription in event data");
            return;
        }

        await using var context = await dbFactory.CreateDbContextAsync();
        var client = context.Clients.FirstOrDefault(c => c.SubscriptionId == subscription.Id);
        if (client is null) {
            logger.LogWarning("Could not find subscription {subscriptionId}", subscription.Id);
            return;
        }
        SetThreadCulture(client.Locale);
        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "subscriptionId", subscription.Id! },
            }, client.PhoneNumber);
        await MessageResource.CreateAsync(new CreateMessageOptions(client.PhoneNumber) {
            MessagingServiceSid = _messageServiceSid,
            Body = localizer.GetStringWithReplacements(resourceName, replacements ?? new { }),
        });
    }
}
