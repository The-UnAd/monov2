using Microsoft.Extensions.Logging;
using Stripe;

namespace UnAd.Functions;

public interface IStripeVerifier {
    bool TryVerify(string stripeSignature, string stripeEndpointSecret, string json, out Event @event);
}

public class StripeVerifier(ILogger<StripeVerifier> logger) : IStripeVerifier {
    public bool TryVerify(string stripeSignature, string stripeEndpointSecret, string json, out Event @event) {
        if (string.IsNullOrEmpty(stripeEndpointSecret)) {
            throw new ArgumentException($"'{nameof(stripeEndpointSecret)}' cannot be null or empty.", nameof(stripeEndpointSecret));
        }

        if (string.IsNullOrEmpty(json)) {
            throw new ArgumentException($"'{nameof(json)}' cannot be null or empty.", nameof(json));
        }

        try {
            @event = EventUtility.ConstructEvent(json, stripeSignature, stripeEndpointSecret);
            return @event is not null;
        } catch (StripeException e) {
            logger.LogError(e, "Stripe signature verification failed");
            @event = default!;
            return false;
        }
    }
}
