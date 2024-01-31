using StackExchange.Redis;

namespace UnAd.Redis;
public static class Redis {
    private static class Keys {
        public static string SubscriberHash(string phoneNumber) => $"subscriber:{phoneNumber}";
        public static string ClientHash(string phoneNumber) => $"client:{phoneNumber}";
        public static string ClientPhone(string id) => $"client:{id}";
        public static string ClientSet() => "clients";
        public static string SubscriberSet() => "subscribers";
        public static string SubscriberClientSet(string phoneNumber) => $"subscriber:{phoneNumber}:clients";
        public static string ClientSubscriberSet(string id) => $"client:{id}:subscribers";
        public static string SubscriptionToPhoneNumber(string subscriptionId) => $"subscription:{subscriptionId}";
        public static string ClientAnnouncementsSet(string phoneNumber) => $"client:{phoneNumber}:announcements";
        public static string AnnouncementHash(string sid) => $"announcement:{sid}";
        public static string SubscriberStopModeHash(string phoneNumber) => $"subscriber:{phoneNumber}:clients:unsub";
        public static string PendingAnnouncement(string phoneNumber) => $"client:{phoneNumber}:announcements:confirm";
        public static string SubscriptionPhone(string subscriptionId) => $"subscription:{subscriptionId}";
        public static string ProductHash(string productId) => $"product:{productId}";
        public static string ProductLimitsHash(string productId) => $"product:{productId}:limits";
        public static string ClientLimitsHash(string clientPhone) => $"client:{clientPhone}:limits";
        public static string ProductSet() => "products";
        public static string ProductFeatureFlagsSet(string productId) => $"product:{productId}:featureflags";
    }

    public static HashEntry[] GetSubscriberHash(this IDatabase db, string subscriptionId) => 
        db.HashGetAll(Keys.SubscriberHash(subscriptionId));
    public static RedisValue GetSubscriberHashValue(this IDatabase db, string subscriptionId, RedisValue hashField) => 
        db.HashGet(Keys.SubscriberHash(subscriptionId), hashField);
    public static bool IsClient(this IDatabase db, string clientPhone) =>
        db.KeyExists(Keys.ClientHash(clientPhone));
    public static RedisValue GetClientPhone(this IDatabase db, string phoneNumber) => 
        db.StringGet(Keys.ClientPhone(phoneNumber));
    public static HashEntry[] GetClientHash(this IDatabase db, string id) => 
        db.HashGetAll(Keys.ClientHash(id));
    public static RedisValue GetClientHashValue(this IDatabase db, string phoneNumber, RedisValue hashField) =>
        db.HashGet(Keys.ClientHash(phoneNumber), hashField);
    public static RedisValue SetClientHashValue(this IDatabase db, string phoneNumber, RedisValue hashField, RedisValue hashValue) =>
        db.HashSet(Keys.ClientHash(phoneNumber), hashField, hashValue);
    public static void StoreAnnouncement(this IDatabase db, string clientPhone, string sid) {
        db.HashSet(Keys.AnnouncementHash(sid), "from", clientPhone);
        db.SortedSetAdd(Keys.ClientAnnouncementsSet(clientPhone), sid, 0);
    }
    public static RedisValue GetAnnoucementSentFrom(this IDatabase db, string sid) => 
        db.HashGet(Keys.AnnouncementHash(sid), "from");
    public static double StoreAnnoucementClick(this IDatabase db, string phoneNumber, string sid) =>
        db.SortedSetIncrement(Keys.ClientAnnouncementsSet(phoneNumber), sid, 1);
    public static bool IsSubscriber(this IDatabase db, string phoneNumber) =>
        (bool)db.Execute("SISMEMBER", new[] { "subscribers", phoneNumber });
    public static RedisValue[] GetSubscriberClientSet(this IDatabase db, string phoneNumber) =>
        db.SetMembers(Keys.SubscriberClientSet(phoneNumber));
    public static void RemoveSubscriberFromClient(this IDatabase db, string subscriberPhone, string id) =>
        db.SetRemove(Keys.ClientSubscriberSet(id), subscriberPhone);
    public static RedisValue[] GetClientSubscriberList(this IDatabase db, string id) =>
        db.SetMembers(Keys.ClientSubscriberSet(id));
    public static void RemoveClientFromSubscriber(this IDatabase db, string subscriberPhone, string clientPhone) =>
        db.SetRemove(Keys.SubscriberClientSet(subscriberPhone), clientPhone);
    public static void RemoveSubscriber(this IDatabase db, string phoneNumber) => 
        db.SetRemove(Keys.SubscriberSet(), phoneNumber);
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
    public static int GetSubscriberClientCount(this IDatabase db, string subscriberPhone) =>
        (int)db.Execute("SCARD", new[] { Keys.SubscriberClientSet(subscriberPhone) });
    public static void SetUnsubscribeListEntry(this IDatabase db, string phoneNumber, int index, string id) => 
        db.HashSet(Keys.SubscriberStopModeHash(phoneNumber), index, id);
    public static void ExpireUnsubscribeList(this IDatabase db, string phoneNumber) =>
        db.KeyExpire(Keys.SubscriberStopModeHash(phoneNumber), TimeSpan.FromMinutes(5));
    public static RedisValue GetSubscriptionPhone(this IDatabase db, string subscriptionId) =>
        db.StringGet(Keys.SubscriptionPhone(subscriptionId));
    public static void SetSubscriptionPhone(this IDatabase db, string subscriptionId, string phoneNumber) =>
        db.StringSet(Keys.SubscriptionPhone(subscriptionId), phoneNumber);
    public static void StoreProduct(this IDatabase db, string productId, string name, string description) {
        db.HashSet(Keys.ProductHash(productId), [
            new HashEntry("name", name),
            new HashEntry("description", description),
        ]);
        db.SetAdd(Keys.ProductSet(), productId);
    }
    public static void SetProductLimits(this IDatabase db, string productId, Dictionary<string, string> pairs) {
        db.HashSet(Keys.ProductLimitsHash(productId), pairs.Select(p => new HashEntry(p.Key, p.Value)).ToArray());
    }
    public static void SetClientProductLimit(this IDatabase db, string clientPhone, RedisValue name, RedisValue value) {
        db.HashSet(Keys.ClientLimitsHash(clientPhone), [
            new HashEntry(name, value),
        ]);
    }
    public static void DecrementClientProductLimitValue(this IDatabase db, string clientPhone, RedisValue name, double value) {
        db.HashDecrement(Keys.ClientLimitsHash(clientPhone), name, value);
    }
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
