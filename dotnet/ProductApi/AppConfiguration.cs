namespace ProductApi;

internal static class AppConfiguration {
    public static class Keys {
        public const string RedisUrl = "REDIS_URL";
        public const string CognitoAuthority = "Cognito:Authority";
        public const string KafkaBrokerList = "KAFKA_BROKER_LIST";
    }

    public static string GetRedisUrl(this IConfiguration configuration) =>
         configuration[Keys.RedisUrl] ?? throw new MissingConfigException(Keys.RedisUrl);

    public static string GetCognitoAuthority(this IConfiguration configuration) =>
         configuration[Keys.CognitoAuthority] ?? throw new MissingConfigException(Keys.CognitoAuthority);

    public static string GetKafkaBrokerList(this IConfiguration configuration) =>
        configuration[Keys.KafkaBrokerList] ?? throw new MissingConfigException(Keys.KafkaBrokerList);
}


[Serializable]
public class MissingConfigException(string key) : Exception($"Config key ${key} has no value") { }
