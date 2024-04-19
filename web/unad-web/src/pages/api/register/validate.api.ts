import type { NextApiRequest, NextApiResponse } from 'next';

import { generateSlug } from '@/lib/crypto';
import { prisma } from '@/lib/db';
import { createTranslator, DefaultLocale, getRequestLocale } from '@/lib/i18n';
import mixpanel from '@/lib/mixpanel';
import { validateToken } from '@/lib/otp';
import { createModelFactory } from '@/lib/redis';

type ValidateResponse = {
  clientId: string;
};

interface ValidateApiRequest extends NextApiRequest {
  body: {
    phone: string;
    otp: string;
    name: string;
  };
}
interface ErrorResponse {
  message: string;
}

export default async function handler(
  req: ValidateApiRequest,
  res: NextApiResponse<ValidateResponse | ErrorResponse>
) {
  const t = await createTranslator(req, 'pages/api/register/validate');
  const { phone, otp, name } = req.body;

  using models = createModelFactory();
  try {
    await models.connect();
    const secret = await models.getOtpSecret(phone);
    if (!secret) {
      throw new Error(t('errors.otpExpired'));
    }
    const success = validateToken(otp, secret);

    if (!success) {
      throw new Error(t('errors.invalidOtp'));
    }
    await models.deleteOtpSecret(phone);

    const locale = getRequestLocale(req) ?? DefaultLocale;

    const client = await prisma.client.create({
      data: {
        phone_number: phone,
        name,
        locale,
        slug: generateSlug(phone),
      },
    });

    mixpanel.people.set(phone, {
      $phone: phone,
      $name: name,
      locale,
      type: 'client',
    });
    mixpanel.track('web.clientAdded', {
      distinct_id: phone,
      $phone: phone,
    });

    return res.status(200).json({ clientId: client.id });
  } catch (error: any) {
    console.error('error in /api/register/validate', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
