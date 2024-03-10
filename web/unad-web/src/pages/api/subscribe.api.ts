import type { NextApiRequest, NextApiResponse } from 'next';

import { prisma } from '@/lib/db';
import { createTranslator, DefaultLocale, getRequestLocale } from '@/lib/i18n';
import mixpanel from '@/lib/mixpanel';
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

  try {
    const client = await prisma.client.findUnique({ where: { id: clientId } });
    if (!client) {
      return res.status(404).json({
        message: t('errors.invalidClient'),
      });
    }
    const { phone_number: clientPhone } = client;

    const locale = getRequestLocale(req) ?? DefaultLocale;

    await prisma.$transaction([
      prisma.subscriber.upsert({
        create: {
          phone_number: phone,
          locale,
        },
        update: {
          locale,
        },
        where: {
          phone_number: phone,
        },
      }),
      prisma.client_subscriber.create({
        data: {
          client_id: clientId,
          subscriber_phone_number: phone,
        },
      }),
    ]);

    mixpanel.people.set(phone, {
      $phone: phone,
      locale,
      type: 'subscriber',
    });
    mixpanel.track('web.subscriberAdded', {
      distinct_id: client.phone_number,
      subscriber: phone,
    });
    mixpanel.track('web.subscribed', {
      distinct_id: phone,
      client: client.phone_number,
    });

    const subCount = await prisma.client_subscriber.count({
      where: { client_id: clientId },
    });

    await sendSms(phone, t('sms.welcome', { name: client.name }));
    await sendSms(clientPhone, t('sms.newSub', { subCount }));

    return res.status(200).end();
  } catch (error: any) {
    console.error('error in /api/subscribe', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
