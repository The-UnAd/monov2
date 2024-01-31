import twilio from 'twilio';
const client = twilio(
  process.env.TWILIO_ACCOUNT_SID,
  process.env.TWILIO_AUTH_TOKEN
);

export function sendSms(phone: string, message: string) {
  return client.messages.create({
    body: message,
    to: phone,
    messagingServiceSid: process.env.TWILIO_MESSAGE_SERVICE_SID,
  });
};

export function getSmsBySid(sid: string) {
  return client.messages.get(sid).fetch();
}
