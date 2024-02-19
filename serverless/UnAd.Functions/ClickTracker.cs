using Microsoft.Extensions.Logging;
using System.Net;
using static UnAd.Functions.MixpanelClient;
using UnAd.Redis;

namespace UnAd.Functions;


public class ClickTracker(ILoggerFactory loggerFactory, MixpanelClient mixpanelClient) {
    public record ClickEvent {
        public string event_type { get; init; }
        public string sms_sid { get; init; }
        public string to { get; init; }
        public string from { get; init; }
        public string link { get; init; }
        public DateTime click_time { get; init; }
        public string messaging_service_sid { get; init; }
        public string account_sid { get; init; }
        public string user_agent { get; init; }
    }

    private readonly ILogger _logger = loggerFactory.CreateLogger<ClickTracker>();

    //public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req) {
    //    var json = await req.ReadFromJsonAsync<ClickEvent>();
    //    if (json is null) {
    //        _logger.LogError("Unable to parse JSON");
    //        return req.CreateResponse(HttpStatusCode.BadRequest);
    //    }

    //    var db = redis.GetDatabase();
    //    var from = db.GetAnnoucementSentFrom(json.sms_sid);

    //    db.StoreAnnoucementClick(from.ToString(), json.sms_sid);

    //    await mixpanelClient.Track(Events.AnnouncementClick, new() {
    //        { "sms_sid", json.sms_sid },
    //        { "click_time", json.click_time.ToString() },
    //        { "link", json.link },
    //    }, from);

    //    var response = req.CreateResponse(HttpStatusCode.OK);
    //    return response;
    //}
}
