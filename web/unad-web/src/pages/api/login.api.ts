import { Users } from '@unad/models';
import type { NextApiRequest, NextApiResponse } from 'next';

import { prisma } from '@/lib/db';
import { createTranslator } from '@/lib/i18n';
import { signJwt } from '@/lib/jwt';
import { validateToken } from '@/lib/otp';
import { createModelFactory } from '@/lib/redis';

interface ErrorResponse {
  message: string;
}

export interface TokenResponse {
  token: string;
}

interface ValidateApiRequest extends NextApiRequest {
  body: {
    phone: string;
    otp: string;
  };
}

export default async function handler(
  req: ValidateApiRequest,
  res: NextApiResponse<TokenResponse | ErrorResponse>
) {
  const t = await createTranslator(req, 'pages/api/login');
  const { phone, otp } = req.body;
  try {
    using models = createModelFactory();
    await models.connect();

    const client = await prisma.client.findUnique({
      where: { phone_number: phone },
    });
    if (!client) {
      await models.deleteOtpSecret(phone);
      throw new Error(t('errors.invalidClient'));
    }
    const secret = await models.getOtpSecret(phone);
    if (!secret) {
      await models.deleteOtpSecret(phone);
      throw new Error(t('errors.invalidPhoneNumber'));
    }
    const success = validateToken(otp, secret);

    if (!success) {
      throw new Error(t('errors.invalidOtp'));
    }

    await models.deleteOtpSecret(phone);
    const jwt = await signJwt({
      phone,
      sub: client.id,
    });
    const token = await models.createSession(jwt);
    return res.status(200).send({
      token,
    });
  } catch (error: any) {
    console.error('error in /api/login', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
