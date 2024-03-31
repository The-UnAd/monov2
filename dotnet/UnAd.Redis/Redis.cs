using StackExchange.Redis;

namespace UnAd.Redis;
public static class Redis {
    private static class Keys {
        public static string SubscriberStopModeHash(string phoneNumber) => $"subscriber:{phoneNumber}:clients:unsub";
        public static string PendingAnnouncement(string phoneNumber) => $"client:{phoneNumber}:announcements:confirm";
        public static string ProductHash(string productId) => $"product:{productId}";
        public static string ProductLimitsHash(string productId) => $"product:{productId}:limits";
        public static string ClientLimitsHash(string clientPhone) => $"client:{clientPhone}:limits";
        public static string ProductSet() => "products";
        public static string ProductFeatureFlagsSet(string productId) => $"product:{productId}:featureflags";
    }

    public static bool IsSubscriberInStopMode(this IDatabase db, string phoneNumber) =>
        db.KeyExists(Keys.SubscriberStopModeHash(phoneNumber));
    public static bool StopSubscriberStopMode(this IDatabase db, string phoneNumber) =>
        db.KeyDelete(Keys.SubscriberStopModeHash(phoneNumber));
    public static RedisValue GetStopModeClientIdByIndex(this IDatabase db, string phoneNumber, int index) =>
        db.HashGet(Keys.SubscriberStopModeHash(phoneNumber), index);
    public static void SetPendingAnnouncement(this IDatabase db, string clientPhone, string smsBody) =>
        db.StringSet(Keys.PendingAnnouncement(clientPhone), smsBody, TimeSpan.FromMinutes(5));
    public static RedisValue GetPendingAnnouncement(this IDatabase db, string clientPhone) =>
        db.StringGet(Keys.PendingAnnouncement(clientPhone));
    public static void DeletePendingAnnouncement(this IDatabase db, string clientPhone) =>
        db.KeyDelete(Keys.PendingAnnouncement(clientPhone));
    public static void StartSubscriberStopMode(this IDatabase db, string phoneNumber, IEnumerable<string> ids) {
        db.HashSet(Keys.SubscriberStopModeHash(phoneNumber), ids.Select((e, i) => new HashEntry(i + 1, e)).ToArray());
        db.KeyExpire(Keys.SubscriberStopModeHash(phoneNumber), TimeSpan.FromMinutes(5));
    }
    public static void StoreProduct(this IDatabase db, string productId, string name, string description) {
        db.HashSet(Keys.ProductHash(productId), [
            new HashEntry("name", name),
            new HashEntry("description", description),
        ]);
        db.SetAdd(Keys.ProductSet(), productId);
    }
    public static void SetProductLimits(this IDatabase db, string productId, Dictionary<string, string> pairs) =>
        db.HashSet(Keys.ProductLimitsHash(productId), pairs.Select(p => new HashEntry(p.Key, p.Value)).ToArray());
    public static void DeleteClientProductLimits(this IDatabase db, string clientPhone) =>
        db.KeyDelete(Keys.ClientLimitsHash(clientPhone));
    public static void SetClientProductLimit(this IDatabase db, string clientPhone, RedisValue name, RedisValue value) =>
        db.HashSet(Keys.ClientLimitsHash(clientPhone), [
            new HashEntry(name, value),
        ]);
    public static void DecrementClientProductLimitValue(this IDatabase db, string clientPhone, RedisValue name, double value) =>
        db.HashDecrement(Keys.ClientLimitsHash(clientPhone), name, value);
    public static RedisValue GetClientProductLimitValue(this IDatabase db, string clientPhone, RedisValue hashField) =>
        db.HashGet(Keys.ClientLimitsHash(clientPhone), hashField);
    public static HashEntry[] GetProductHash(this IDatabase db, string productId) =>
        db.HashGetAll(Keys.ProductHash(productId));
    public static RedisValue GetProductHashValue(this IDatabase db, string productId, RedisValue hashField) =>
        db.HashGet(Keys.ProductHash(productId), hashField);
    public static HashEntry[] GetProductLimits(this IDatabase db, string productId) =>
        db.HashGetAll(Keys.ProductLimitsHash(productId));
    public static RedisValue GetProductLimitValue(this IDatabase db, string productId, RedisValue hashField) =>
        db.HashGet(Keys.ProductLimitsHash(productId), hashField);
    public static RedisValue[] GetProductSet(this IDatabase db) =>
        db.SetMembers(Keys.ProductSet());
    public static void SetProductFeatureFlag(this IDatabase db, string productId, string featureFlag) =>
        db.SetAdd(Keys.ProductFeatureFlagsSet(productId), featureFlag);
    public static RedisValue[] GetProductFeatureFlags(this IDatabase db, string productId) =>
        db.SetMembers(Keys.ProductFeatureFlagsSet(productId));
    public static bool ProductHasFeatureFlag(this IDatabase db, string productId, string featureFlag) =>
        db.SetContains(Keys.ProductFeatureFlagsSet(productId), featureFlag);
}
