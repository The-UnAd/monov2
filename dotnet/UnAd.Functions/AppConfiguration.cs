using Microsoft.Extensions.Configuration;

namespace UnAd.Functions;
internal static class AppConfiguration {
    public static class Keys {
        public const string MixpanelHttpClient = "Mixpanel";

        public const string ResourcesPath = "Resources";

        public const string RedisUrl = "REDIS_URL";
        public const string MixPanelToken = "MIXPANEL_TOKEN";
        public const string TwilioAccountSid = "TWILIO_ACCOUNT_SID";
        public const string TwilioAuthToken = "TWILIO_AUTH_TOKEN";
        public const string TwilioMessageServiceSid = "TWILIO_MESSAGE_SERVICE_SID";
        public const string StripeApiKey = "STRIPE_API_KEY";
        public const string StripeSubscriptionEndpointSecret = "STRIPE_SUBSCRIPTION_ENDPOINT_SECRET";
        public const string StripeProductEndpointSecret = "STRIPE_PRODUCT_ENDPOINT_SECRET";
        public const string StripePaymentEndpointSecret = "STRIPE_PAYMENT_ENDPOINT_SECRET";
        public const string AccountUrl = "AccountUrl";
        public const string StripePortalUrl = "StripePortalUrl";
        public const string ClientLinkBaseUri = "ClientLinkBaseUri";
    }
    public static string GetTwilioAuthToken(this IConfiguration configuration) =>
        configuration[Keys.TwilioAuthToken] ?? throw new ArgumentNullException(nameof(Keys.TwilioAuthToken), $"Value ${Keys.TwilioAuthToken} has no value");

    public static string GetStripeApiKey(this IConfiguration configuration) =>
        configuration[Keys.StripeApiKey] ?? throw new ArgumentNullException(nameof(Keys.StripeApiKey), $"Value ${Keys.StripeApiKey} has no value");

    public static string GetStripeSubscriptionEndpointSecret(this IConfiguration configuration) =>
        configuration[Keys.StripeSubscriptionEndpointSecret]
        ?? throw new ArgumentNullException(nameof(Keys.StripeSubscriptionEndpointSecret), $"Value ${Keys.StripeSubscriptionEndpointSecret} has no value");

    public static string GetStripeProductEndpointSecret(this IConfiguration configuration) =>
        configuration[Keys.StripeProductEndpointSecret]
        ?? throw new ArgumentNullException(nameof(Keys.StripeProductEndpointSecret), $"Value ${Keys.StripeProductEndpointSecret} has no value");
    public static string GetStripePaymentEndpointSecret(this IConfiguration configuration) =>
        configuration[Keys.StripePaymentEndpointSecret]
        ?? throw new ArgumentNullException(nameof(Keys.StripePaymentEndpointSecret), $"Value ${Keys.StripePaymentEndpointSecret} has no value");

    public static string GetRedisUrl(this IConfiguration configuration) =>
        configuration[Keys.RedisUrl] ?? throw new ArgumentNullException(nameof(Keys.RedisUrl), $"Value ${Keys.RedisUrl} has no value");

    public static string GetMixPanelToken(this IConfiguration configuration) =>
        configuration[Keys.MixPanelToken] ?? throw new ArgumentNullException(nameof(Keys.MixPanelToken), $"Value ${Keys.MixPanelToken} has no value");

    public static string GetTwilioAccountSid(this IConfiguration configuration) =>
        configuration[Keys.TwilioAccountSid] ?? throw new ArgumentNullException(nameof(Keys.TwilioAccountSid), $"Value ${Keys.TwilioAccountSid} has no value");

    public static string GetTwilioMessageServiceSid(this IConfiguration configuration) =>
        configuration[Keys.TwilioMessageServiceSid] ?? throw new ArgumentNullException(nameof(Keys.TwilioMessageServiceSid), $"Value ${Keys.TwilioMessageServiceSid} has no value");

    public static string GetAccountUrl(this IConfiguration configuration) =>
        configuration[Keys.AccountUrl] ?? throw new ArgumentNullException(nameof(Keys.AccountUrl), $"Value ${Keys.AccountUrl} has no value");

    public static string GetStripePortalUrl(this IConfiguration configuration) =>
        configuration[Keys.StripePortalUrl] ?? throw new ArgumentNullException(nameof(Keys.StripePortalUrl), $"Value ${Keys.StripePortalUrl} has no value");

    public static string GetClientLinkBaseUri(this IConfiguration configuration) =>
        configuration[Keys.ClientLinkBaseUri] ?? throw new ArgumentNullException(nameof(Keys.ClientLinkBaseUri), $"Value ${Keys.ClientLinkBaseUri} has no value");

    public static IConfiguration BuildConfiguration(this IConfigurationBuilder builder) => builder
        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
}
