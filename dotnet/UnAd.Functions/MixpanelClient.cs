using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace UnAd.Functions;

public interface IMixpanelClient {
    Task Track(string eventName, Dictionary<string, string>? properties, string? distinctId = null);
}

public sealed class MixpanelClient(IHttpClientFactory httpClientFactory, ILogger<MixpanelClient> logger, IOptions<MixpanelOptions> config) : IMixpanelClient {
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<MixpanelClient> _logger = logger;
    private readonly MixpanelOptions _config = config.Value;

    public async Task Track(string eventName, Dictionary<string, string>? properties, string? distinctId = default) {
        properties ??= [];
        properties.Add("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
        properties.Add("token", _config.Token);
        properties.Add("$insert_id", Guid.NewGuid().ToString());

        if (!string.IsNullOrEmpty(distinctId) && properties.ContainsKey("distinct_id") == false) {
            properties.Add("distinct_id", distinctId);
        }

        try {
            var json = JsonSerializer.Serialize(new MixpanelEvent[] {
                new(eventName, properties)
            }, MixpanelJsonSerializerContext.Default.MixpanelEventArray);

            using var content = new StringContent(json, Encoding.UTF8,
                MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json));
            using var client = _httpClientFactory.CreateClient(AppConfiguration.Keys.MixpanelHttpClient);
            using var httpResponseMessage = await client.PostAsync("/track", content);
            if (!httpResponseMessage.IsSuccessStatusCode) {
                using var reader = new StreamReader(await httpResponseMessage.Content.ReadAsStreamAsync());
                var responseBody = await reader.ReadToEndAsync();
                _logger.LogMixpanelErrorResponse(responseBody);
            }
        } catch (Exception ex) {
            _logger.LogMixpanelSendException(ex);
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
[JsonSerializable(typeof(MixpanelEvent[]))]
internal partial class MixpanelJsonSerializerContext : JsonSerializerContext { }

public record MixpanelEvent(string Event, IDictionary<string, string> Properties);
