import { IncomingMessage } from 'http';
import { NextApiRequest } from 'next';
import { createTranslator as intlCreateTranslator } from 'next-intl';

export const DefaultLocale = 'en-US';

export async function createTranslator(
  req: NextApiRequest | IncomingMessage,
  namespace: string,
  defaultLocale: string = DefaultLocale
) {
  const locale = getRequestLocale(req) ?? defaultLocale;
  const messages = (await import(`../../messages/${locale}.json`)).default;
  return intlCreateTranslator({
    locale,
    messages,
    namespace,
  });
}

export function getRequestLocale(req: NextApiRequest | IncomingMessage) {
  return req.headers['accept-language']?.toString().split(',')[0];
}

export async function importMessages(defaultLocale: string = DefaultLocale) {
  const messages = (await import(`../../messages/${defaultLocale}.json`))
    .default;
  return messages;
}
