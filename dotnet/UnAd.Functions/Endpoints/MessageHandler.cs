using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Globalization;
using System.Net;
using System.Text;
using UnAd.Data.Users;

namespace UnAd.Functions;

public class MessageHandler(MessageHelper messageHelper, IDbContextFactory<UserDbContext> dbFactory) {


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

        await using var context = await dbFactory.CreateDbContextAsync();
        var client = await context.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == smsFrom);
        var sub = await context.Subscribers.FirstOrDefaultAsync(s => s.PhoneNumber == smsFrom);
        var location = string.IsNullOrEmpty(sub?.Locale) ? string.IsNullOrEmpty(client?.Locale) ? "en-US" : client.Locale : sub.Locale;

        var culture = new CultureInfo(location);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;

        var message = messageHelper.ProcessMessage(smsBody, smsFrom);
        return Results.Text(message.ToString(), "text/xml", Encoding.UTF8);
    }
}
