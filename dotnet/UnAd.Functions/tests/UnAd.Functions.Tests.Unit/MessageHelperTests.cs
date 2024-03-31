using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using StackExchange.Redis;
using Twilio.TwiML;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UnAd.Functions.Tests.Unit;

public class MessageHelperTests {

    private static MessageHelper CreateHelper(Client[]? clients = default, Subscriber[]? subscribers = default) {
        var mockRedis = new Mock<IConnectionMultiplexer>();
        var mockDatabase = new Mock<IDatabase>();
        var mockDbContext = new Mock<UserDbContext>(
            new DbContextOptionsBuilder<UserDbContext>().Options);
        var mockDbContextFactory = new Mock<IDbContextFactory<UserDbContext>>();
        var mockStripe = new Mock<Stripe.IStripeClient>();
        var mockLocalizer = new Mock<IStringLocalizer<MessageHelper>>();
        var mockLogger = new Mock<ILogger<MessageHelper>>();
        var mockMixpanelClient = new Mock<IMixpanelClient>();
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c[It.IsAny<string>()]).Returns((string e) => e);

        var messageHelper = new MessageHelper(
            mockRedis.Object,
            mockDbContextFactory.Object,
            mockStripe.Object,
            mockLocalizer.Object,
            mockLogger.Object,
            mockMixpanelClient.Object,
            mockConfig.Object);

        mockDbContext.Setup(d => d.Clients).ReturnsDbSet(clients ?? []);
        mockDbContext.Setup(d => d.Subscribers).ReturnsDbSet(subscribers ?? []);
        if (subscribers is not null && subscribers.Length > 0) {
            mockDbContext.Setup(d => d.Subscribers.Find(subscribers[0].PhoneNumber)).Returns(subscribers[0]);
        }
        mockDbContextFactory.Setup(d => d.CreateDbContext()).Returns(mockDbContext.Object);
        mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string k) => new LocalizedString(k, k));
        mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);

        return messageHelper;
    }

    [Fact(DisplayName =
        "ProcessUnsubscribeMessage returns \"NotSubscriber\" when the phone number provided is not a present as a Subscriber in the database")]
    public void ProcessUnsubscribeMessageReturnsNotSubscriber() {
        // Arrange
        var sut = CreateHelper([], [
            new Subscriber() {
                PhoneNumber = "1",
            }
        ]);

        // Act
        var result = sut.ProcessUnsubscribeMessage("1234567890");

        // Assert
        Assert.Equal(
            new MessagingResponse().Message("NotSubscriber").ToString(),
            result.ToString());
    }

    [Fact(DisplayName =
        "ProcessUnsubscribeMessage returns \"UnsubscribeFromClient\" when the phone number provided is not a present as a Client in the database")]
    public void ProcessUnsubscribeMessageReturnsUnsubscribeFromClient() {
        // Arrange
        var sut = CreateHelper([
            new Client() {
                Id = Guid.NewGuid(),
                PhoneNumber = "1234567890"
            }
        ]);

        // Act
        var result = sut.ProcessUnsubscribeMessage("1234567890");

        // Assert
        Assert.Equal(
            new MessagingResponse()
                .Message("UnsubscribeFromClient").ToString(),
            result.ToString());
    }

    [Fact(DisplayName =
        "ProcessUnsubscribeMessage returns \"NoSubscriptions\" when the subscriber has no clients assigned")]
    public void ProcessUnsubscribeMessageReturnsNoSubscriptions() {
        // Arrange
        var sut = CreateHelper([], [
            new Subscriber() {
                PhoneNumber = "1234567890"
            }
        ]);

        // Act
        var result = sut.ProcessUnsubscribeMessage("1234567890");

        // Assert
        Assert.Equal(
            new MessagingResponse()
                .Message("NoSubscriptions").ToString(),
            result.ToString());
    }

    [Fact(DisplayName =
        "ProcessUnsubscribeMessage returns \"UnsubscribeAllSuccess\" when the subscriber has one client assigned")]
    public void ProcessUnsubscribeMessageReturnsUnsubscribeAllSuccess() {
        // Arrange
        var sut = CreateHelper([], [
            new Subscriber() {
                PhoneNumber = "1234567890",
                Clients = [
                    new Client() {
                        Id = Guid.NewGuid()
                    }
                ]
            }
        ]);

        // Act
        var result = sut.ProcessUnsubscribeMessage("1234567890");

        // Assert
        Assert.Equal(
            new MessagingResponse()
                .Message("UnsubscribeAllSuccess").ToString(),
            result.ToString());
    }

    [Fact(DisplayName =
        "ProcessUnsubscribeMessage returns \"UnsubscribeListTemplate\" when the subscriber has multiple clients assigned")]
    public void ProcessUnsubscribeMessageReturnsUnsubscribeListTemplate() {
        // Arrange
        var sut = CreateHelper([], [
            new Subscriber() {
                PhoneNumber = "1234567890",
                Clients = [
                    new Client() {
                        Id = Guid.NewGuid()
                    },
                    new Client() {
                        Id = Guid.NewGuid()
                    }
                ]
            }
        ]);

        // Act
        var result = sut.ProcessUnsubscribeMessage("1234567890");

        // Assert
        Assert.Equal(
            new MessagingResponse()
                .Message("UnsubscribeListTemplate").ToString(),
            result.ToString());
    }
}
