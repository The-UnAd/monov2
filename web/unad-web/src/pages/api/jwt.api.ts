import type { NextApiRequest, NextApiResponse } from 'next';

import { createTranslator } from '@/lib/i18n';
import { verifyJwt } from '@/lib/jwt';
import { createModelFactory } from '@/lib/redis';

interface ErrorResponse {
  message: string;
}

interface JwtTokenResponse {
  clientId: string;
}

interface ValidateApiRequest extends NextApiRequest {
  body: {
    token: string;
  };
}

export default async function handler(
  req: ValidateApiRequest,
  res: NextApiResponse<ErrorResponse | JwtTokenResponse>
) {
  const t = await createTranslator(req, 'pages/api/jwt');
  const { token } = req.body;

  try {
    using models = createModelFactory();
    await models.connect();
    const jwt = await models.getSession(token);
    if (!jwt) {
      throw new Error(t('errors.invalidToken'));
    }
    const payload = await verifyJwt(jwt);
    return res.status(200).json({
      clientId: payload.sub as string,
    });
  } catch (error: any) {
    console.error('error in /api/jwt', error.stack);
    return res.status(500).json({
      message:
        process.env.NODE_ENV === 'development'
          ? error.stack ?? error.message
          : error.message,
    });
  }
}
