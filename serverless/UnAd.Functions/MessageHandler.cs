using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using StackExchange.Redis;
using System.Globalization;
using System.Net;
using UnAd.Redis;

namespace UnAd.Functions;

public class MessageHandler(MessageHelper messageHelper, IConnectionMultiplexer redis) {


    private static readonly string[] _ignoreList = [
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

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/MessageHandler")]
    public APIGatewayHttpApiV2ProxyResponse Run(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
        var form = ParseQueryString(request.Body);
        var smsBody = form["Body"];
        var smsFrom = form["From"];

        if (_ignoreList.Any(i => i.Equals(smsBody.Trim(), StringComparison.OrdinalIgnoreCase))) {
            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        // NOTE: MessagingServiceSid and AccountSid are avialable in the form data

        var db = redis.GetDatabase();
        var subLocale = db.GetSubscriberHashValue(smsFrom, "locale");
        var clientLocale = db.GetClientHashValue(smsFrom, "locale");
        var location = subLocale.HasValue ? subLocale.ToString() : clientLocale.HasValue ? clientLocale.ToString() : "en-US";

        var culture = new CultureInfo(location);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;

        var message = messageHelper.ProcessMessage(smsBody, smsFrom);
        return new APIGatewayHttpApiV2ProxyResponse {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Body = message.ToString(),
            Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" }
                }
        };
    }
}
