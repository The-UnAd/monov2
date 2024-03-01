using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using System.Text;
using System.Text.RegularExpressions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;
using UnAd.Redis;
using static UnAd.Functions.MixpanelClient;

namespace UnAd.Functions;

public partial class MessageHelper(IConnectionMultiplexer redis,
                     IDbContextFactory<UserDbContext> dbContextFactory,
                     Stripe.IStripeClient stripe,
                     IStringLocalizer<MessageHelper> localizer,
                     ILogger<MessageHelper> logger,
                     MixpanelClient mixpanelClient,
                     IConfiguration config) {

    private readonly Regex _stopRegex = StopRegex();
    private readonly string _messageServiceSid = config.GetTwilioMessageServiceSid();

    private static MessagingResponse CreateSmsResponseContent(string message) => new MessagingResponse()
                .Message(message);

    public MessagingResponse ProcessMessage(string smsBody, string smsFrom) {

        if ("commands".Equals(smsBody, StringComparison.OrdinalIgnoreCase)) {
            return ProcessHelpMessage(smsFrom);
        }
        if (int.TryParse(_stopRegex.Match(smsBody).Groups[1].Value, out var num)) {
            return ProcessStopSubscriptionMessage(num, smsFrom);
        }
        if ("stop all".Equals(smsBody, StringComparison.OrdinalIgnoreCase)) {
            return ProcessStopAllMessage(smsBody, smsFrom);
        }
        if ("unsubscribe".Equals(smsBody, StringComparison.OrdinalIgnoreCase)) {
            return ProcessUnsubscribeMessage(smsBody, smsFrom);
        }
        if ("send".Equals(smsBody, StringComparison.OrdinalIgnoreCase)) {
            return ProcessConfirmAnnouncementMessage(smsFrom);
        }
        if ("cancel".Equals(smsBody, StringComparison.OrdinalIgnoreCase)) {
            return ProcessCancelAnnouncementMessage(smsFrom);
        }
        // TODO: Add a "support" command to get support number
        // TODO: Add a "link" command to get subscribe link

        // TODO: what to do when they send us stuff that the opt-in service will catch?
        return ProcessAnnouncementMessage(smsBody, smsFrom);
    }

    private MessagingResponse ProcessStopAllMessage(string smsBody, string smsFrom) {
        using var context = dbContextFactory.CreateDbContext();
        var subscriber = context.Subscribers.Find(smsFrom);
        if (subscriber is null) {
            logger.LogWarning("Subscriber {smsFrom} not found", smsFrom);
            return CreateSmsResponseContent(localizer.GetString("NotSubscriber"));
        }

        logger.LogInformation("Unsubscribing {smsFrom} from all clients", smsFrom);
        foreach (var clientPhone in subscriber.Clients) {
            subscriber.Clients.Remove(clientPhone);
        }
        context.SaveChanges();
        mixpanelClient.Track(Events.UnsubscribeAll, [], smsFrom)
            .ConfigureAwait(false).GetAwaiter().GetResult();
        return CreateSmsResponseContent(localizer.GetString("UnsubscribeAllSuccess"));
    }

    private MessagingResponse ProcessStopSubscriptionMessage(int num, string smsFrom) {
        var db = redis.GetDatabase();
        using var context = dbContextFactory.CreateDbContext();
        var subscriber = context.Subscribers.Find(smsFrom);
        if (subscriber is null) {
            return CreateSmsResponseContent(localizer.GetString("NotSubscriber"));
        }
        var isInStopMode = db.IsSubscriberInStopMode(smsFrom);
        if (!isInStopMode) {
            return CreateSmsResponseContent(localizer.GetString("NotInStopMode"));
        }
        var id = db.GetStopModeClientIdByIndex(smsFrom, num);
        if (id.IsNullOrEmpty) {
            return CreateSmsResponseContent(localizer.GetString("UnsubscribeInvalidSelection"));
        }
        var client = context.Clients.Find(id);
        if (client is null) {
            logger.LogWarning("Client {id} not found", id);
            return CreateSmsResponseContent(localizer.GetString("UnsubscribeInvalidSelection"));
        }
        db.StopSubscriberStopMode(smsFrom);
        client.SubscriberPhoneNumbers.Remove(client.SubscriberPhoneNumbers.First(x => x.PhoneNumber == smsFrom));
        context.SaveChanges();
        mixpanelClient.Track(Events.Unsubscribe, new() {
                { "from", client.PhoneNumber },
            }, smsFrom).ConfigureAwait(false).GetAwaiter().GetResult();
        return CreateSmsResponseContent(localizer.GetStringWithReplacements("UnsubscribeSuccess", new {
            clientName = client.Name
        }));
    }

    private MessagingResponse ProcessCancelAnnouncementMessage(string smsFrom) {
        var db = redis.GetDatabase();
        db.DeletePendingAnnouncement(smsFrom);
        return CreateSmsResponseContent(localizer.GetString("AnnouncementCancelled"));
    }

    private MessagingResponse ProcessAnnouncementMessage(string smsBody, string smsFrom) {
        var db = redis.GetDatabase();
        using var context = dbContextFactory.CreateDbContext();
        var client = context.Clients.FirstOrDefault(x => x.PhoneNumber == smsFrom);
        if (client is null) {
            return CreateSmsResponseContent(localizer.GetString("NotCustomer"));
        }

        var subscriptionService = new Stripe.SubscriptionService(stripe);
        var subscription = subscriptionService.Get(client.SubscriptionId);
        if (!subscription.IsActive()) {
            return CreateSmsResponseContent(localizer.GetString("SubscriptionNotActive"));
        }

        db.SetPendingAnnouncement(smsFrom, smsBody);
        var message = localizer.GetStringWithReplacements("AnnouncementConfirm", new {
            smsBody
        });
        return CreateSmsResponseContent(message);
    }

    public MessagingResponse ProcessHelpMessage(string smsFrom) {
        var db = redis.GetDatabase();
        using var context = dbContextFactory.CreateDbContext();
        var subscriber = context.Subscribers.Find(smsFrom);
        if (subscriber is not null) {
            return CreateSmsResponseContent(localizer.GetString("SubscriberHelpMessage"));
        }
        var client = context.Clients.FirstOrDefault(c => c.PhoneNumber == smsFrom);
        if (client is not null) {
            var message = localizer.GetStringWithReplacements("ClientHelpMessage", new {
                accountUrl = config.GetAccountUrl(),
                subUrl = config.GetStripePortalUrl(),
                shareUrl = $"{config.GetClientLinkBaseUri()}/{client.Id}", // TODO: should be short ID
            });
            return CreateSmsResponseContent(message);
        }
        return CreateSmsResponseContent(localizer.GetString("NotCustomer"));
    }

    public MessagingResponse ProcessConfirmAnnouncementMessage(string smsFrom) {
        var db = redis.GetDatabase();
        using var context = dbContextFactory.CreateDbContext();
        var client = context.Clients
            .Include(c => c.SubscriberPhoneNumbers)
            .FirstOrDefault(x => x.PhoneNumber == smsFrom);
        if (client is null) {
            return CreateSmsResponseContent(localizer.GetString("NotCustomer"));
        }

        var subscriptionService = new Stripe.SubscriptionService(stripe);
        var subscription = subscriptionService.Get(client.SubscriptionId);
        if (!subscription.IsActive()) {
            return CreateSmsResponseContent(localizer.GetString("SubscriptionNotActive"));
        }

        var subscribers = client.SubscriberPhoneNumbers.Select(x => x.PhoneNumber).ToArray();
        var smsBody = db.GetPendingAnnouncement(smsFrom);
        if (smsBody.IsNullOrEmpty) {
            return CreateSmsResponseContent(localizer.GetString("NoPendingAnnouncement"));
        }

        var messageMax = db.GetClientProductLimitValue(smsFrom, "maxMessages");
        if (int.TryParse(messageMax, out var messagesLeft) && messagesLeft == 0) {
            // TODO: flesh out this message
            return CreateSmsResponseContent(localizer.GetString("NoMessagesRemaining"));
        }

        var count = 0;
        foreach (var number in subscribers) {
            try {
                //var linkId = Guid.NewGuid().ToString("o");
                // TODO: at some point I need to wrap this in a rate-limiting mechanism
                var resource = MessageResource.Create(new CreateMessageOptions(number) {
                    MessagingServiceSid = _messageServiceSid,
                    ShortenUrls = true,
                    Body = localizer.GetStringWithReplacements("AnnouncementTemplate", new {
                        smsBody,
                        clientName = client.Name,
                        link = "" //$"{config.GetValue<string>("SmsLinkBaseUri")}/{linkId}",
                    }),
                });
                if (resource.ErrorCode.HasValue) {
                    logger.LogError("Error sending message to {number}: {ErrorMessage}", number, resource.ErrorMessage);
                    continue;
                }

                context.Announcements.Add(new Announcement {
                    ClientId = client.Id,
                    MessageSid = resource.Sid
                });
                count++;

            } catch (Exception ex) {
                logger.LogError("Error sending message to {number}: {ex}", number, ex);
            }
        }
        context.SaveChanges();
        db.DeletePendingAnnouncement(smsFrom);
        // TOOD: still need to store these in Redis
        db.DecrementClientProductLimitValue(smsFrom, "maxMessages", 1);
        mixpanelClient.Track(Events.AnnouncementSent, new() {
            { "count", count.ToString()}
        }, smsFrom).ConfigureAwait(false).GetAwaiter().GetResult();
        return CreateSmsResponseContent(localizer.GetStringWithReplacements("AnnouncementSent", new { count }));
    }

    public MessagingResponse ProcessUnsubscribeMessage(string smsBody, string smsFrom) {
        var db = redis.GetDatabase();
        using var context = dbContextFactory.CreateDbContext();
        if (context.Clients.Any(c => c.PhoneNumber == smsFrom)) {
            return CreateSmsResponseContent(localizer.GetString("SupportMessage"));
        }

        var clients = context.Subscribers.Find(smsFrom)?.Clients
            .Select((client, index) => (
                client,
                index
            ))
            .ToArray();
        if (clients is null) {
            return CreateSmsResponseContent(localizer.GetString("NotSubscriber"));
        }

        if (clients?.Length > 1) {
            var message = clients.Aggregate(new StringBuilder(), (sb, i) => {
                var (client, index) = i;
                sb.Append(localizer.GetStringWithReplacements("UnsubscribeListEntry", new {
                    clientName = client.Name,
                    index
                }));

                return sb;
            });
            foreach (var (client, index) in clients) {
                db.SetUnsubscribeListEntry(smsFrom, index + 1, client.Id.ToString());
            }
            return CreateSmsResponseContent(localizer.GetStringWithReplacements("UnsubscribeListTemplate", new {
                list = message
            }));

        } else {
            db.ExpireUnsubscribeList(smsFrom);
            mixpanelClient.Track(Events.UnsubscribeAll, new() {
                { "phone", smsFrom },
            }, smsFrom).ConfigureAwait(false).GetAwaiter().GetResult();
            return CreateSmsResponseContent(localizer.GetString("UnsubscribeAllSuccess"));
        }
    }

    [GeneratedRegex(@"STOP (\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex StopRegex();
}
