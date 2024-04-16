using Microsoft.Extensions.Configuration;

namespace KafkaConsumer;

internal static class AppConfiguration {

    public static class ConnectionStrings {
        public const string UserDb = nameof(UserDb);
        public const string ProductDb = nameof(ProductDb);
    }

    public static class Keys {
        public const string RedisUrl = "REDIS_URL";
        public const string StripeApiKey = "STRIPE_API_KEY";
        public const string KafkaBrokerList = "KAFKA_BROKER_LIST";
        public const string GraphQLApiUrl = "GRAPHQL_API_URL";
    }

    public static string GetKafkaBrokerList(this IConfiguration configuration) =>
        configuration[Keys.KafkaBrokerList] ?? throw new MissingConfigException(Keys.KafkaBrokerList);

    public static string GetStripeApiKey(this IConfiguration configuration) =>
        configuration[Keys.StripeApiKey] ?? throw new MissingConfigException(Keys.StripeApiKey);

    public static string GetRedisUrl(this IConfiguration configuration) =>
        configuration[Keys.RedisUrl] ?? throw new MissingConfigException(Keys.RedisUrl);

    public static string GetGraphQLApiUrl(this IConfiguration configuration) =>
        configuration[Keys.GraphQLApiUrl] ?? throw new MissingConfigException(Keys.GraphQLApiUrl);
}

[Serializable]
public class MissingConfigException(string key) : Exception($"Config key ${key} has no value") { }
