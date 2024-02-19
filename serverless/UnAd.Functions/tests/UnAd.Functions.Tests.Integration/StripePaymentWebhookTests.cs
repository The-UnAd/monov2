using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StackExchange.Redis;
using Stripe;
using System.Net;
using System.Text;

namespace UnAd.Functions.Tests.Integration;

public class StripePaymentWebhookTests {
    [SetUp]
    public void Setup() {

    }

    [Test]
    public async Task ShouldCompleteSuccessfullyForUnsetEvent() {
        var stripe = Substitute.For<IStripeClient>();
        var verifier = Substitute.For<IStripeVerifier>();
        var messageSender = Substitute.For<IMessageSender>();
        var stripeEvent = Substitute.For<Event>();
        verifier.TryVerify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<Event>()).Returns(x => {
            x[3] = stripeEvent;
            return true;
        });
        var redis = Substitute.For<IConnectionMultiplexer>();
        var logger = Substitute.For<ILogger<StripePaymentWebhook>>();
        var localizer = Substitute.For<IStringLocalizer<StripePaymentWebhook>>();
        var config = Substitute.For<IConfiguration>();
        config.GetTwilioMessageServiceSid().Returns("test");
        config.GetStripePortalUrl().Returns("test");
        config.GetStripePaymentEndpointSecret().Returns("test");
        using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
        var request = TestHelpers.CreateRequest(bodyStream, responseStream, new HttpHeadersCollection([
           new KeyValuePair<string, string>("stripe-signature", "test")
        ]));

        var sut = new StripePaymentWebhook(stripe, verifier, redis, messageSender, logger, localizer, config);

        var result = await sut.Run(request);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Test]
    public async Task ShouldCompleteSuccessfullyInvoicePaidEvent() {
        var stripe = Substitute.For<IStripeClient>();
        var verifier = Substitute.For<IStripeVerifier>();
        var messageSender = Substitute.For<IMessageSender>();
        var stripeEvent = Substitute.For<Event>();
        var invoice = Substitute.For<Invoice>();
        stripeEvent.Type = Events.InvoicePaid;
        stripeEvent.Data = new EventData {
            Object = new Invoice {
                SubscriptionId = "test"
            }
        };
        verifier.TryVerify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<Event>()).Returns(x => {
            x[3] = stripeEvent;
            return true;
        });
        var redis = Substitute.For<IConnectionMultiplexer>();
        var db = Substitute.For<IDatabase>();
        redis.GetDatabase().Returns(db);
        db.StringGet(Arg.Any<RedisKey>()).Returns((RedisValue)"test");
        var logger = Substitute.For<ILogger<StripePaymentWebhook>>();
        var localizer = Substitute.For<IStringLocalizer<StripePaymentWebhook>>();
        var localizedString = new LocalizedString("test", "test", true, "Resources");
        localizer[Arg.Any<string>()].Returns(localizedString);
        var config = Substitute.For<IConfiguration>();
        config.GetTwilioMessageServiceSid().Returns("test");
        config.GetStripePortalUrl().Returns("test");
        config.GetStripePaymentEndpointSecret().Returns("test");
        using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
        var request = TestHelpers.CreateRequest(bodyStream, responseStream, new HttpHeadersCollection([
           new KeyValuePair<string, string>("stripe-signature", "test")
        ]));

        var sut = new StripePaymentWebhook(stripe, verifier, redis, messageSender, logger, localizer, config);

        var result = await sut.Run(request);

        logger.Received(1);
        messageSender.Received(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldCompleteSuccessfullyInvoicePaymentFailedEvent() {
        var stripe = Substitute.For<IStripeClient>();
        var verifier = Substitute.For<IStripeVerifier>();
        var messageSender = Substitute.For<IMessageSender>();
        var stripeEvent = Substitute.For<Event>();
        var invoice = Substitute.For<Invoice>();
        stripeEvent.Type = Events.InvoicePaymentFailed;
        stripeEvent.Data = new EventData {
            Object = new Invoice {
                SubscriptionId = "test"
            }
        };
        verifier.TryVerify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<Event>()).Returns(x => {
            x[3] = stripeEvent;
            return true;
        });
        var redis = Substitute.For<IConnectionMultiplexer>();
        var db = Substitute.For<IDatabase>();
        redis.GetDatabase().Returns(db);
        db.StringGet(Arg.Any<RedisKey>()).Returns((RedisValue)"test");
        var logger = Substitute.For<ILogger<StripePaymentWebhook>>();
        var localizer = Substitute.For<IStringLocalizer<StripePaymentWebhook>>();
        var localizedString = new LocalizedString("test", "test", true, "Resources");
        localizer[Arg.Any<string>()].Returns(localizedString);
        var config = Substitute.For<IConfiguration>();
        config.GetTwilioMessageServiceSid().Returns("test");
        config.GetStripePortalUrl().Returns("test");
        config.GetStripePaymentEndpointSecret().Returns("test");
        using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
        var request = TestHelpers.CreateRequest(bodyStream, responseStream, new HttpHeadersCollection([
           new KeyValuePair<string, string>("stripe-signature", "test")
        ]));

        var sut = new StripePaymentWebhook(stripe, verifier, redis, messageSender, logger, localizer, config);

        var result = await sut.Run(request);

        logger.Received(1);
        messageSender.Received(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
