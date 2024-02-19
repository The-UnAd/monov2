using Stripe;

namespace UnAd.Functions;
internal static class StripeExtensions {
    private static readonly string[] _activeSubscriptionStatuses = ["active", "trialing"];

    public static bool IsActive(this Subscription subscription) =>
        _activeSubscriptionStatuses.Contains(subscription.Status);

    public static Dictionary<string, string>? GetSubscriptionProductMetaData(this Subscription subscription) =>
        subscription.Items.Data.FirstOrDefault()?.Price.Product.Metadata;
}
