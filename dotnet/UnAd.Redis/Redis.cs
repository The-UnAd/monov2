using StackExchange.Redis;

namespace UnAd.Redis;
public static class Redis {
    private static class Keys {
        public static string SubscriptionToPhoneNumber(string subscriptionId) => $"subscription:{subscriptionId}";
        public static string SubscriberStopModeHash(string phoneNumber) => $"subscriber:{phoneNumber}:clients:unsub";
        public static string PendingAnnouncement(string phoneNumber) => $"client:{phoneNumber}:announcements:confirm";
        public static string PriceHash(string priceId) => $"price:{priceId}";
        public static string PriceLimitsHash(string priceId) => $"price:{priceId}:limits";
        public static string ClientLimitsHash(string clientPhone) => $"client:{clientPhone}:limits";
        public static string PriceSet() => "prices";
        public static string PriceFeatureFlagsSet(string priceId) => $"price:{priceId}:featureflags";
        public static string UserToken(string id) => $"token:{id}";
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
    public static void SetUnsubscribeListEntry(this IDatabase db, string phoneNumber, int index, string id) => 
        db.HashSet(Keys.SubscriberStopModeHash(phoneNumber), index, id);
    public static void ExpireUnsubscribeList(this IDatabase db, string phoneNumber) =>
        db.KeyExpire(Keys.SubscriberStopModeHash(phoneNumber), TimeSpan.FromMinutes(5));
    public static void StorePrice(this IDatabase db, string priceId, string name, string description) {
        db.HashSet(Keys.PriceHash(priceId), [
            new HashEntry("name", name),
            new HashEntry("description", description),
        ]);
        db.SetAdd(Keys.PriceSet(), priceId);
    }
    public static void SetPriceLimits(this IDatabase db, string priceId, Dictionary<string, string> pairs) =>
        db.HashSet(Keys.PriceLimitsHash(priceId), pairs.Select(p => new HashEntry(p.Key, p.Value)).ToArray());
    public static void DeleteClientPriceLimits(this IDatabase db, string clientPhone) =>
        db.KeyDelete(Keys.ClientLimitsHash(clientPhone));
    public static void SetClientPriceLimit(this IDatabase db, string clientPhone, RedisValue name, RedisValue value) =>
        db.HashSet(Keys.ClientLimitsHash(clientPhone), [
            new HashEntry(name, value),
        ]);
    public static void DecrementClientPriceLimitValue(this IDatabase db, string clientPhone, RedisValue name, double value) =>
        db.HashDecrement(Keys.ClientLimitsHash(clientPhone), name, value);
    public static RedisValue GetClientPriceLimitValue(this IDatabase db, string clientPhone, RedisValue hashField) =>
        db.HashGet(Keys.ClientLimitsHash(clientPhone), hashField);
    public static HashEntry[] GetPriceHash(this IDatabase db, string priceId) =>
        db.HashGetAll(Keys.PriceHash(priceId));
    public static RedisValue GetPriceHashValue(this IDatabase db, string priceId, RedisValue hashField) =>
        db.HashGet(Keys.PriceHash(priceId), hashField);
    public static HashEntry[] GetPriceLimits(this IDatabase db, string priceId) =>
        db.HashGetAll(Keys.PriceLimitsHash(priceId));
    public static RedisValue GetPriceLimitValue(this IDatabase db, string priceId, RedisValue hashField) =>
        db.HashGet(Keys.PriceLimitsHash(priceId), hashField);
    public static RedisValue[] GetPriceSet(this IDatabase db) =>
        db.SetMembers(Keys.PriceSet());
    public static void SetPriceFeatureFlag(this IDatabase db, string priceId, string featureFlag) =>
        db.SetAdd(Keys.PriceFeatureFlagsSet(priceId), featureFlag);
    public static RedisValue[] GetPriceFeatureFlags(this IDatabase db, string priceId) =>
        db.SetMembers(Keys.PriceFeatureFlagsSet(priceId));
    public static bool PriceHasFeatureFlag(this IDatabase db, string priceId, string featureFlag) =>
        db.SetContains(Keys.PriceFeatureFlagsSet(priceId), featureFlag);
    public static void StoreUserToken(this IDatabase db, string id, string jwt, int expiresIn) =>
        db.StringSet(Keys.UserToken(id), jwt, TimeSpan.FromSeconds(expiresIn));
    public static void DeleteUserToken(this IDatabase db, string id) =>
        db.KeyDelete(Keys.UserToken(id));
    public static RedisValue GetUserToken(this IDatabase db, string id) =>
        db.StringGet(Keys.UserToken(id));
}
