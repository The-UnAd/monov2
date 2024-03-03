namespace UserApi;

internal static class AppConfiguration {
    public static class Keys {
        public const string RedisUrl = "REDIS_URL";
        public const string TwilioAccountSid = "TWILIO_ACCOUNT_SID";
        public const string TwilioAuthToken = "TWILIO_AUTH_TOKEN";
        public const string TwilioMessageServiceSid = "TWILIO_MESSAGE_SERVICE_SID";
        public const string StripeApiKey = "STRIPE_API_KEY";
    }
    public static string GetTwilioAuthToken(this IConfiguration configuration) =>
        configuration[Keys.TwilioAuthToken] ?? throw new ArgumentNullException(nameof(Keys.TwilioAuthToken), $"Value ${Keys.TwilioAuthToken} has no value");

    public static string GetStripeApiKey(this IConfiguration configuration) =>
        configuration[Keys.StripeApiKey] ?? throw new ArgumentNullException(nameof(Keys.StripeApiKey), $"Value ${Keys.StripeApiKey} has no value");

    public static string GetRedisUrl(this IConfiguration configuration) =>
        configuration[Keys.RedisUrl] ?? throw new ArgumentNullException(nameof(Keys.RedisUrl), $"Value ${Keys.RedisUrl} has no value");

    public static string GetTwilioAccountSid(this IConfiguration configuration) =>
        configuration[Keys.TwilioAccountSid] ?? throw new ArgumentNullException(nameof(Keys.TwilioAccountSid), $"Value ${Keys.TwilioAccountSid} has no value");

    public static string GetTwilioMessageServiceSid(this IConfiguration configuration) =>
        configuration[Keys.TwilioMessageServiceSid] ?? throw new ArgumentNullException(nameof(Keys.TwilioMessageServiceSid), $"Value ${Keys.TwilioMessageServiceSid} has no value");
}