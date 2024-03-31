using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using Stripe;
using UnAd.Data.Users;
using UnAd.Redis;

namespace UnAd.Functions;

public class StripeCustomerWebhook(IConnectionMultiplexer redis,
                                  IStripeVerifier stripeVerifier,
                                  IConfiguration config,
                                  IMessageSender messageSender,
                                  IDbContextFactory<UserDbContext> dbFactory,
                                  IStringLocalizer<StripeCustomerWebhook> localizer,
                                  IMixpanelClient mixpanelClient,
                                  ILogger<StripeCustomerWebhook> logger) {

    private readonly string _stripeEndpointSecret = config.GetStripeCustomerEndpointSecret();

    public async Task<IResult> Endpoint(HttpRequest request) {
        Event stripeEvent = default!;
        using var streamReader = new StreamReader(request.Body);
        var body = await streamReader.ReadToEndAsync();
        if ((request.Headers.TryGetValue("stripe-signature", out var sig) &&
            !stripeVerifier.TryVerify(sig!, _stripeEndpointSecret, body, out stripeEvent))
            || stripeEvent is null) {
            return Results.BadRequest(new {
                error = "Invalid Stripe signature"
            });
        }
        try {

            if (stripeEvent.Type == Events.CustomerDeleted) {
                await HandleCustomerDeletedEvent(stripeEvent);
            } else {
                logger.LogUnhandledEvent(stripeEvent.Type);
            }

            return Results.Ok();
        } catch (StripeException e) {
            logger.LogException(e);
            return Results.Problem(e.Message);
        }
    }

    private async Task HandleCustomerDeletedEvent(Event stripeEvent) {
        if (stripeEvent.Data.Object is not Customer customer) {
            logger.LogWarning("Could not find customer in event data");
            return;
        }

        await using var context = await dbFactory.CreateDbContextAsync();
        var client = await context.Clients.FirstOrDefaultAsync(c => c.CustomerId == customer.Id);
        if (client is null) {
            logger.LogWarning($"Could not find client with customer id {customer.Id}");
            return;
        }
        var db = redis.GetDatabase();
        db.DeleteClientProductLimits(client.PhoneNumber);

        context.Remove(client);
        await context.SaveChangesAsync();

        await messageSender.Send(client.PhoneNumber, localizer.GetString("CustomerDeleted"));

        await mixpanelClient.Track(MixpanelClient.Events.StripeEvent(stripeEvent.Type), new() {
                { "customerId", customer.Id},
            }, client.PhoneNumber);

        return;
    }
}
