import type { NextApiRequest, NextApiResponse } from 'next';

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
    using models = createModelFactory();
    if (!validatePhone(phone as string)) {
      throw new Error(t('errors.invalidPhoneNumber'));
    }

    models.connect();
    const existingClient = await models.getClientByPhone(phone);
    if (existingClient) {
      throw new Error(t('errors.phoneNumberRegistered'));
    }
    const namedClient = await models.getClientById(name);
    if (namedClient) {
      throw new Error(t('errors.clientNameTaken'));
    }

    const secret = generateSecret();
    await models.setOtpSecret(phone, secret);
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
