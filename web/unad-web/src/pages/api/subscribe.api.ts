import type { NextApiRequest, NextApiResponse } from 'next';

import { createTranslator, DefaultLocale, getRequestLocale } from '@/lib/i18n';
import mixpanel from '@/lib/mixpanel';
import { createModelFactory } from '@/lib/redis';
import { sendSms } from '@/lib/twilio';

interface ErrorResponse {
  message: string;
}

interface ValidateApiRequest extends NextApiRequest {
  body: {
    phone: string;
    otp: string;
    clientId: string;
  };
}

export default async function handler(
  req: ValidateApiRequest,
  res: NextApiResponse<ErrorResponse>
) {
  const t = await createTranslator(req, 'pages/api/subscribe');
  const { phone, clientId } = req.body;

  using models = createModelFactory();
  try {
    await models.connect();
    models.beginTransaction();
    const client = await models.getClientById(clientId);
    if (!client) {
      return res.status(404).json({
        message: t('errors.invalidClient'),
      });
    }
    const { phone: clientPhone } = client;

    const locale = getRequestLocale(req) ?? DefaultLocale;

    const subscriber = models.createSubscriber(phone);

    subscriber.save();
    subscriber.subscribeToClient(client.id!);
    subscriber.setLocale(locale);
    client.addSubscriber(subscriber.phone);
    await models.commitTransaction();

    mixpanel.people.set(phone, {
      $phone: phone,
      locale,
      type: 'subscriber',
    });
    mixpanel.track('web.subscriberAdded', {
      distinct_id: client.phone,
      subscriber: phone,
    });
    mixpanel.track('web.subscribed', {
      distinct_id: phone,
      client: client.phone,
    });

    const subCount = await client.getSubscriberCount();

    await sendSms(phone, t('sms.welcome', { name: client.name }));
    await sendSms(clientPhone, t('sms.newSub', { subCount }));

    return res.status(200).end();
  } catch (error: any) {
    models.rollbackTransaction();
    console.error('error in /api/subscribe', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
