using Microsoft.Extensions.Configuration;

internal static class AppConfiguration {
    public static class Keys {
        public const string RedisUrl = "REDIS_URL";
        public const string StripeApiKey = "STRIPE_API_KEY";
    }
    public static string GetStripeApiKey(this IConfiguration configuration) =>
        configuration[Keys.StripeApiKey]
        ?? throw new ArgumentNullException(nameof(Keys.StripeApiKey), $"Value ${Keys.StripeApiKey} has no value");

    public static string GetRedisUrl(this IConfiguration configuration) =>
        configuration[Keys.RedisUrl]
        ?? throw new ArgumentNullException(Keys.RedisUrl, $"Value ${Keys.RedisUrl} has no value");
}
