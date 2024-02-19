using Microsoft.Extensions.Configuration;
using Twilio.Rest.Api.V2010.Account;

namespace UnAd.Functions;

public interface IMessageSender {
    Task Send(string phoneNumber, string message);
}

public class MessageSender(IConfiguration config) : IMessageSender {
    private readonly string _messageServiceSid = config.GetTwilioMessageServiceSid();

    public Task Send(string phoneNumber, string message) {
        return MessageResource.CreateAsync(new CreateMessageOptions(phoneNumber) {
            MessagingServiceSid = _messageServiceSid,
            Body = message,
        });
    }
}
