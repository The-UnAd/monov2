namespace UserApi.Models;

public class StripeSubscription {
    public string? Id { get; set; }
    public string? Status { get; set; }
}

public class StripeSubscriptionType : ObjectType<StripeSubscription> {
    protected override void Configure(IObjectTypeDescriptor<StripeSubscription> descriptor) {
        descriptor.Field(f => f.Id);
        descriptor.Field(f => f.Status);
    }
}

public static partial class StripeSubscriptionExtensions {
    public static StripeSubscription ToSubscriptionType(this Stripe.Subscription stripeSubscription) =>
        new() {
            Id = stripeSubscription.Id,
            Status = stripeSubscription.Status
        };
}
