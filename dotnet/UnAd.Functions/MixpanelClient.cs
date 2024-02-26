using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace UnAd.Functions;
public sealed class MixpanelClient(IHttpClientFactory httpClientFactory, ILogger<MixpanelClient> logger, IOptions<MixpanelOptions> config) {
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<MixpanelClient> _logger = logger;
    private readonly MixpanelOptions _config = config.Value;

    public async Task Track(string eventName, Dictionary<string, string>? properties, string? distinctId = default) {
        properties ??= [];
        properties.Add("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        properties.Add("token", _config.Token);
        properties.Add("$insert_id", Guid.NewGuid().ToString());

        if (!string.IsNullOrEmpty(distinctId) && properties.ContainsKey("distinct_id") == false) {
            properties.Add("distinct_id", distinctId);
        }

        var json = JsonSerializer.Serialize(
            new MixpanelEvent(eventName, properties), MixpanelJsonSerializerContext.Default.MixpanelEvent);

        using var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        using var client = _httpClientFactory.CreateClient("Mixpanel");
        using var httpResponseMessage = await client.PostAsync("/track", content);
        if (!httpResponseMessage.IsSuccessStatusCode) {
            using var reader = new StreamReader(await httpResponseMessage.Content.ReadAsStreamAsync());
            var response = await reader.ReadToEndAsync();
            _logger.LogErrorResponse(response);
        }
    }

    public static class Events {
        public const string Unsubscribe = "funcs.unsubscribe";
        public const string UnsubscribeAll = "funcs.unsubscribeAll";
        public const string AnnouncementSent = "funcs.announcementSent";
        public const string AnnouncementClick = "funcs.click";
        public static string StripeEvent(string type) => $"funcs.stripe.{type}";
    }
}


[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(MixpanelEvent))]
internal partial class MixpanelJsonSerializerContext : JsonSerializerContext { }

public record MixpanelEvent(string Event, IDictionary<string, string> Properties);
