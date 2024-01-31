import type { NextApiRequest, NextApiResponse } from 'next';
import Stripe from 'stripe';

import { createTranslator } from '@/lib/i18n';
import { createModelFactory } from '@/lib/redis';
import * as twilio from '@/lib/twilio';

interface ErrorResponse {
  message: string;
}

type ValidateResponse = {
  clientId: string;
};

interface ValidateApiRequest extends NextApiRequest {
  body: {
    clientId: string;
  };
}

export default async function handler(
  req: ValidateApiRequest,
  res: NextApiResponse<ValidateResponse | ErrorResponse>
) {
  const t = await createTranslator(req, 'pages/api/register/confirm');
  const { clientId } = req.body;
  try {
    using models = createModelFactory();
    await models.connect();
    const client = await models.getClientById(clientId);
    const subscriptionId = await client?.getStripeSubscriptionId();
    if (!client || !subscriptionId) {
      throw new Error(t('errors.noSubscription'));
    }

    const stripe = new Stripe(process.env.STRIPE_API_KEY as string, {
      apiVersion: '2023-10-16',
    });
    const subscription = await stripe.subscriptions.retrieve(
      subscriptionId as string
    );

    await twilio.sendSms(
      client.phone,
      t('sms.status', {
        status: subscription.status,
        subUrl: `${process.env.SUBSCRIBE_HOST}/${clientId}`,
        qrUrl: `${process.env.SHARE_HOST}/share/${clientId}`,
      })
    );

    return res.status(200).end();
  } catch (error: any) {
    console.error('error in /api/register/confirm', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
