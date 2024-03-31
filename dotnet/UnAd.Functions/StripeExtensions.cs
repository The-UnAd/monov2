using Stripe;

namespace UnAd.Functions;
internal static class StripeExtensions {
    private static readonly string[] ActiveSubscriptionStatuses = ["active", "trialing"];

    public static bool IsActive(this Subscription subscription) =>
        ActiveSubscriptionStatuses.Contains(subscription.Status);
}
