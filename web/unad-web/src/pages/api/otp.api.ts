import type { NextApiRequest, NextApiResponse } from 'next';

import { createTranslator } from '@/lib/i18n';
import { generateSecret, generateToken } from '@/lib/otp';
import { createModelFactory } from '@/lib/redis';
import * as twilio from '@/lib/twilio';
import { validatePhone } from '@/lib/util';

interface JoinApiRequest extends NextApiRequest {
  body: {
    phone: string;
    clientId: string;
  };
}

export default async function handler(
  req: JoinApiRequest,
  res: NextApiResponse
) {
  const t = await createTranslator(req, 'pages/api/otp');
  const { phone } = req.body;
  try {
    using models = createModelFactory();
    validatePhone(phone);

    await models.connect();

    const secret = generateSecret();
    await models.setOtpSecret(phone, secret, 30);
    const otp = generateToken(secret);

    await twilio.sendSms(phone, t('sms.otp', { otp }));
    return res.status(200).json({});
  } catch (error: any) {
    console.error('error in /api/otp', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
