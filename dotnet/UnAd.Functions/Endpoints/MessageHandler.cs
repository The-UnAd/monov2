using StackExchange.Redis;
using System.Globalization;
using System.Net;
using System.Text;
using UnAd.Redis;

namespace UnAd.Functions.Endpoints;

public class MessageHandler(MessageHelper messageHelper, IConnectionMultiplexer redis) {


    private static readonly string[] IgnoreList = [
        "stop",
        "start",
        "unstop",
        "help",
    ];
    private static Dictionary<string, string> ParseQueryString(string query) =>
        query.Split('&').ToDictionary(
            pair => WebUtility.UrlDecode(pair[..pair.IndexOf('=')]),
            pair => WebUtility.UrlDecode(pair[(pair.IndexOf('=') + 1)..]));

    /**
     * TODO: Create a new HTTP trigger that will handle the callback for status of messages.
     * https://www.twilio.com/docs/messaging/guides/track-outbound-message-status
     */
    public async Task<IResult> Endpoint(HttpRequest request) {
        using var streamReader = new StreamReader(request.Body);
        var body = await streamReader.ReadToEndAsync();
        var form = ParseQueryString(body);
        var smsBody = form["Body"];
        var smsFrom = form["From"];

        if (IgnoreList.Any(i => i.Equals(smsBody.Trim(), StringComparison.OrdinalIgnoreCase))) {
            return Results.Ok();
        }

        // NOTE: MessagingServiceSid and AccountSid are avialable in the form data

        var db = redis.GetDatabase();
        var subLocale = db.GetSubscriberHashValue(smsFrom, "locale");
        var clientLocale = db.GetClientHashValue(smsFrom, "locale");
        var location = subLocale.HasValue ? subLocale.ToString() : clientLocale.HasValue ? clientLocale.ToString() : "en-US";

        var culture = new CultureInfo(location);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;

        var message = messageHelper.ProcessMessage(smsBody, smsFrom);
        return Results.Text(message.ToString(), "text/xml", Encoding.UTF8);
    }
}