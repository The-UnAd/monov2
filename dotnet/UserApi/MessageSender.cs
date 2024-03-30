using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace UserApi;

public interface IMessageSender {
    Task<MessageResource> Send(string phoneNumber, string message);
}

public class MessageSender(IConfiguration config, Func<ITwilioRestClient> twilioClientFunc) : IMessageSender {
    private readonly string _messageServiceSid = config.GetTwilioMessageServiceSid();

    public Task<MessageResource> Send(string phoneNumber, string message) =>
        MessageResource.CreateAsync(new CreateMessageOptions(phoneNumber) {
            MessagingServiceSid = _messageServiceSid,
            Body = message,
        }, twilioClientFunc());
}
