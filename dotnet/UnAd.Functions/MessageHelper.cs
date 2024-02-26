using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.RegularExpressions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using UnAd.Data.Users;
using UnAd.Redis;
using static UnAd.Functions.MixpanelClient;

namespace UnAd.Functions;

public partial class MessageHelper {
    private readonly IConnectionMultiplexer _redis;
    private readonly IDbContextFactory<UserDbContext> _dbContextFactory;
    private readonly Stripe.IStripeClient _stripe;
    private readonly ILogger<MessageHelper> _logger;
    private readonly MixpanelClient _mixpanelClient;
    private readonly IConfiguration _config;
    private readonly Regex _stopRegex = StopRegex();
    private readonly IStringLocalizer<MessageHelper> _localizer;
    private readonly string _messageServiceSid;

    public MessageHelper(IConnectionMultiplexer redis,
                         IDbContextFactory<UserDbContext> dbContextFactory,
                         Stripe.IStripeClient stripe,
                         IStringLocalizer<MessageHelper> localizer,
                         ILogger<MessageHelper> logger,
                         MixpanelClient mixpanelClient,
                         IConfiguration config) {

        _redis = redis;
        _dbContextFactory = dbContextFactory;
        _stripe = stripe;
        _logger = logger;
        _mixpanelClient = mixpanelClient;
        _config = config;
        _localizer = localizer;
        _messageServiceSid = config.GetTwilioMessageServiceSid();
    }

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
        var db = _redis.GetDatabase();
        var isSubscriber = db.IsSubscriber(smsFrom);
        if (!isSubscriber) {
            return CreateSmsResponseContent(_localizer.GetString("NotSubscriber"));
        }
        _logger.LogInformation("Unsubscribing {smsFrom} from all clients", smsFrom);
        using var context = _dbContextFactory.CreateDbContext();
        var subscriber = context.Subscribers.Find(smsFrom);
        if (subscriber is null) {
            _logger.LogWarning("Subscriber {smsFrom} not found", smsFrom);
            return CreateSmsResponseContent(_localizer.GetString("NotSubscriber"));
        }

        foreach (var clientPhone in subscriber.Clients) {
            subscriber.Clients.Remove(clientPhone);
        }
        context.SaveChanges();
        _mixpanelClient.Track(Events.UnsubscribeAll, [], smsFrom)
            .ConfigureAwait(false).GetAwaiter().GetResult();
        return CreateSmsResponseContent(_localizer.GetString("UnsubscribeAllSuccess"));
    }

    private MessagingResponse ProcessStopSubscriptionMessage(int num, string smsFrom) {
        var db = _redis.GetDatabase();
        var isSubscriber = db.IsSubscriber(smsFrom);
        if (!isSubscriber) {
            return CreateSmsResponseContent(_localizer.GetString("NotSubscriber"));
        }
        var isInStopMode = db.IsSubscriberInStopMode(smsFrom);
        if (!isInStopMode) {
            return CreateSmsResponseContent(_localizer.GetString("NotInStopMode"));
        }
        var id = db.GetStopModeClientIdByIndex(smsFrom, num);
        if (id.IsNullOrEmpty) {
            return CreateSmsResponseContent(_localizer.GetString("UnsubscribeInvalidSelection"));
        }
        using var context = _dbContextFactory.CreateDbContext();
        var client = context.Clients.Find(id);
        if (client is null) {
            _logger.LogWarning("Client {id} not found", id);
            return CreateSmsResponseContent(_localizer.GetString("UnsubscribeInvalidSelection"));
        }
        db.StopSubscriberStopMode(smsFrom);
        client.Subscribers.Remove(client.Subscribers.First(x => x.PhoneNumber == smsFrom));

        _mixpanelClient.Track(Events.Unsubscribe, new() {
                { "from", client.PhoneNumber },
            }, smsFrom).ConfigureAwait(false).GetAwaiter().GetResult();
        return CreateSmsResponseContent(_localizer.GetStringWithReplacements("UnsubscribeSuccess", new {
            clientName = client.Name
        }));
    }

    private MessagingResponse ProcessCancelAnnouncementMessage(string smsFrom) {
        var db = _redis.GetDatabase();
        db.DeletePendingAnnouncement(smsFrom);
        return CreateSmsResponseContent(_localizer.GetString("AnnouncementCancelled"));
    }

    private MessagingResponse ProcessAnnouncementMessage(string smsBody, string smsFrom) {
        var db = _redis.GetDatabase();
        using var context = _dbContextFactory.CreateDbContext();
        var client = context.Clients.FirstOrDefault(x => x.PhoneNumber == smsFrom);
        if (client is null) {
            return CreateSmsResponseContent(_localizer.GetString("NotCustomer"));
        }

        var subscriptionService = new Stripe.SubscriptionService(_stripe);
        var subscription = subscriptionService.Get(client.SubscriptionId);
        if (!subscription.IsActive()) {
            return CreateSmsResponseContent(_localizer.GetString("SubscriptionNotActive"));
        }

        db.SetPendingAnnouncement(smsFrom, smsBody);
        var message = _localizer.GetStringWithReplacements("AnnouncementConfirm", new {
            smsBody
        });
        return CreateSmsResponseContent(message);
    }

    public MessagingResponse ProcessHelpMessage(string smsFrom) {
        var db = _redis.GetDatabase();
        using var context = _dbContextFactory.CreateDbContext();
        var subscriber = context.Subscribers.Find(smsFrom);
        if (subscriber is not null) {
            return CreateSmsResponseContent(_localizer.GetString("SubscriberHelpMessage"));
        }
        var client = context.Clients.FirstOrDefault(c => c.PhoneNumber == smsFrom);
        if (client is not null) {
            var message = _localizer.GetStringWithReplacements("ClientHelpMessage", new {
                accountUrl = _config.GetAccountUrl(),
                subUrl = _config.GetStripePortalUrl(),
                shareUrl = $"{_config.GetClientLinkBaseUri()}/{client.Id}", // TODO: should be short ID
            });
            return CreateSmsResponseContent(message);
        }
        return CreateSmsResponseContent(_localizer.GetString("NotCustomer"));
    }

    public MessagingResponse ProcessConfirmAnnouncementMessage(string smsFrom) {
        var db = _redis.GetDatabase();
        using var context = _dbContextFactory.CreateDbContext();
        var client = context.Clients.FirstOrDefault(x => x.PhoneNumber == smsFrom);
        if (client is null) {
            return CreateSmsResponseContent(_localizer.GetString("NotCustomer"));
        }

        var subscriptionService = new Stripe.SubscriptionService(_stripe);
        var subscription = subscriptionService.Get(client.SubscriptionId);
        if (!subscription.IsActive()) {
            return CreateSmsResponseContent(_localizer.GetString("SubscriptionNotActive"));
        }

        var subscribers = client.Subscribers.Select(x => x.PhoneNumber).ToArray();
        var smsBody = db.GetPendingAnnouncement(smsFrom);
        if (smsBody.IsNullOrEmpty) {
            return CreateSmsResponseContent(_localizer.GetString("NoPendingAnnouncement"));
        }

        var messageMax = db.GetClientProductLimitValue(smsFrom, "maxMessages");
        if (int.TryParse(messageMax, out var messagesLeft) && messagesLeft == 0) {
            // TODO: flesh out this message
            return CreateSmsResponseContent(_localizer.GetString("NoMessagesRemaining"));
        }

        var count = 0;
        foreach (var number in subscribers) {
            try {
                //var linkId = Guid.NewGuid().ToString("o");
                // TODO: at some point I need to wrap this in a rate-limiting mechanism
                var resource = MessageResource.Create(new CreateMessageOptions(number) {
                    MessagingServiceSid = _messageServiceSid,
                    ShortenUrls = true,
                    Body = _localizer.GetStringWithReplacements("AnnouncementTemplate", new {
                        smsBody,
                        clientName = client.Name,
                        link = "" //$"{_config.GetValue<string>("SmsLinkBaseUri")}/{linkId}",
                    }),
                });
                if (resource.ErrorCode.HasValue) {
                    _logger.LogError("Error sending message to {number}: {ErrorMessage}", number, resource.ErrorMessage);
                    continue;
                }

                db.StoreAnnouncement(smsFrom, resource.Sid);
                count++;

            } catch (Exception ex) {
                _logger.LogError("Error sending message to {number}: {ex}", number, ex);
            }
        }
        db.DeletePendingAnnouncement(smsFrom);
        // TOOD: still need to store these in Redis
        db.DecrementClientProductLimitValue(smsFrom, "maxMessages", 1);
        _mixpanelClient.Track(Events.AnnouncementSent, new() {
            { "count", count.ToString()}
        }, smsFrom).ConfigureAwait(false).GetAwaiter().GetResult();
        return CreateSmsResponseContent(_localizer.GetStringWithReplacements("AnnouncementSent", new { count }));
    }

    public MessagingResponse ProcessUnsubscribeMessage(string smsBody, string smsFrom) {
        var db = _redis.GetDatabase();
        using var context = _dbContextFactory.CreateDbContext();
        if (context.Clients.Any(c => c.PhoneNumber == smsFrom)) {
            return CreateSmsResponseContent(_localizer.GetString("SupportMessage"));
        }

        var clients = context.Subscribers.Find(smsFrom)?.Clients
            .Select((client, index) => (
                client,
                index
            ))
            .ToArray();
        if (clients is null) {
            return CreateSmsResponseContent(_localizer.GetString("NotSubscriber"));
        }

        if (clients?.Length > 1) {
            var message = clients.Aggregate(new StringBuilder(), (sb, i) => {
                var (client, index) = i;
                sb.Append(_localizer.GetStringWithReplacements("UnsubscribeListEntry", new {
                    clientName = client.Name,
                    index
                }));

                return sb;
            });
            foreach (var (client, index) in clients) {
                db.SetUnsubscribeListEntry(smsFrom, index + 1, client.Id.ToString());
            }
            return CreateSmsResponseContent(_localizer.GetStringWithReplacements("UnsubscribeListTemplate", new {
                list = message
            }));

        } else {
            db.ExpireUnsubscribeList(smsFrom);
            _mixpanelClient.Track(Events.UnsubscribeAll, new() {
                { "phone", smsFrom },
            }, smsFrom).ConfigureAwait(false).GetAwaiter().GetResult();
            return CreateSmsResponseContent(_localizer.GetString("UnsubscribeAllSuccess"));
        }
    }

    [GeneratedRegex(@"STOP (\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex StopRegex();
}
