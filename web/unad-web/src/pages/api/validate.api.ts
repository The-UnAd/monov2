import type { NextApiRequest, NextApiResponse } from 'next';

import { prisma } from '@/lib/db';
import { createTranslator } from '@/lib/i18n';
import { validateToken } from '@/lib/otp';
import { createModelFactory } from '@/lib/redis';

type ValidateResponse = {
  code: string;
};

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
  res: NextApiResponse<ValidateResponse | ErrorResponse>
) {
  const t = await createTranslator(req, 'pages/api/validate');
  const { phone, otp, clientId } = req.body;

  try {
    using models = createModelFactory();
    await models.connect();

    const secret = await models.getOtpSecret(phone);
    if (!secret) {
      throw new Error(t('errors.invalidOtp'));
    }
    const success = validateToken(otp, secret);

    if (!success) {
      throw new Error(t('errors.invalidOtp'));
    }

    await models.deleteOtpSecret(phone);
    const client = await prisma.client.findUnique({ where: { id: clientId } });
    if (!client) {
      throw new Error(t('errors.invalidClient'));
    }
    const code = await models.createSubscriberConfirmation(
      phone,
      client.phone_number!
    );
    return res.status(200).json({ code });
  } catch (error: any) {
    console.error('error in /api/validate', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
