import type { NextApiRequest, NextApiResponse } from 'next';

import { prisma } from '@/lib/db';
import { createTranslator } from '@/lib/i18n';
import { generateSecret, generateToken } from '@/lib/otp';
import { createModelFactory } from '@/lib/redis';
import * as twilio from '@/lib/twilio';

type ErrorResponse = {
  message: string;
};

type RegisterResponse = {};

interface JoinApiRequest extends NextApiRequest {
  body: {
    phone: string;
    name: string;
  };
}

export default async function handler(
  req: JoinApiRequest,
  res: NextApiResponse<RegisterResponse | ErrorResponse>
) {
  const t = await createTranslator(req, 'pages/api/register/[phone]');
  const { phone, name } = req.body;

  try {
    if (!validatePhone(phone)) {
      throw new Error(t('errors.invalidPhoneNumber'));
    }
    const client = await prisma.client.findUnique({
      where: { phone_number: phone },
    });
    if (client) {
      throw new Error(t('errors.phoneNumberRegistered'));
    }

    const secret = generateSecret();
    using models = createModelFactory();
    await models.connect();
    await models.setOtpSecret(phone, secret, 0);
    const otp = generateToken(secret);

    await twilio.sendSms(phone, t('otpMessage', { otp }));
    return res.status(200).json({ phone, name });
  } catch (error: any) {
    console.error('error in /api/register/[phone]', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}

function validatePhone(phone: string) {
  return phone.match(/^\+[1-9]\d{1,14}$/);
}
