namespace UserApi;

internal static class AppConfiguration {
    public static class Keys {
        public const string RedisUrl = "REDIS_URL";
        public const string TwilioAccountSid = "TWILIO_ACCOUNT_SID";
        public const string TwilioAuthToken = "TWILIO_AUTH_TOKEN";
        public const string TwilioMessageServiceSid = "TWILIO_MESSAGE_SERVICE_SID";
        public const string StripeApiKey = "STRIPE_API_KEY";
        public const string SubscribeHost = "SUBSCRIBE_HOST";
        public const string CognitoAuthority = "Cognito:Authority";
    }
    public static string GetTwilioAuthToken(this IConfiguration configuration) =>
         configuration[Keys.TwilioAuthToken] ?? throw new MissingConfigException(Keys.TwilioAuthToken);

    public static string GetStripeApiKey(this IConfiguration configuration) =>
         configuration[Keys.StripeApiKey] ?? throw new MissingConfigException(Keys.StripeApiKey);

    public static string GetRedisUrl(this IConfiguration configuration) =>
         configuration[Keys.RedisUrl] ?? throw new MissingConfigException(Keys.RedisUrl);

    public static string GetTwilioAccountSid(this IConfiguration configuration) =>
         configuration[Keys.TwilioAccountSid] ?? throw new MissingConfigException(Keys.TwilioAccountSid);

    public static string GetTwilioMessageServiceSid(this IConfiguration configuration) =>
         configuration[Keys.TwilioMessageServiceSid] ?? throw new MissingConfigException(Keys.TwilioMessageServiceSid);

    public static string GetSubscribeHost(this IConfiguration configuration) =>
         configuration[Keys.SubscribeHost] ?? throw new MissingConfigException(Keys.SubscribeHost);

    public static string GetCognitoAuthority(this IConfiguration configuration) =>
         configuration[Keys.CognitoAuthority] ?? throw new MissingConfigException(Keys.CognitoAuthority);
}


[Serializable]
public class MissingConfigException : Exception {
    public MissingConfigException(string key) : base($"Config key ${key} has no value") { }
}
