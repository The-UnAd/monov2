import Head from 'next/head';
import Image from 'next/image';
import Link from 'next/link';
import { useRouter } from 'next/router';
import type { GetServerSidePropsContext } from 'next/types';
import { useTranslations } from 'next-intl';
import type { ParsedUrlQuery } from 'querystring';
import { useCallback, useState } from 'react';

import OtpForm, { OtpFormData } from '@/Components/OtpForm';
import SubscribeForm, { SubscribeData } from '@/Components/SubscribeForm';
import { generateOtp, subscribeToClient, validateOtp } from '@/lib/api';
import { prisma } from '@/lib/db';
import { importMessages } from '@/lib/i18n';
import { sanitizePhoneNumber } from '@/lib/util';

interface SubscribeProps {
  name: string;
  clientId: string;
}

interface ServerProps extends ParsedUrlQuery {
  clientId: string;
}

function Subscribe({ name, clientId }: SubscribeProps) {
  const [error, setError] = useState('');
  const router = useRouter();
  const [phoneNumber, setPhoneNumber] = useState('');
  const t = useTranslations('pages/subscribe/[clientId]');

  const clickGetOtp = async ({ phone }: SubscribeData) => {
    const formattedPhone = sanitizePhoneNumber(phone);
    try {
      setPhoneNumber(formattedPhone);
      await generateOtp(formattedPhone);
    } catch (err: any) {
      setError(err.message);
    }
  };

  const clickValidate = useCallback(
    async ({ otp }: OtpFormData) => {
      try {
        const code = await validateOtp(otp, phoneNumber, clientId);
        await subscribeToClient(phoneNumber, clientId);
        router.push(`success/${code}`);
      } catch (err: any) {
        setError(err.message);
      }
    },
    [phoneNumber, clientId, router]
  );

  return (
    <>
      <Head>
        <title>{t('page.title', { name })}</title>
        <meta
          property="og:title"
          content={t('page.title', { name }) as string}
        />
        <meta
          property="og:description"
          content={t('page.title', { name }) as string}
        />
        <meta property="og:locale" content="en_US" />
        <meta property="og:type" content="website" />
        <meta property="og:url" content={`//unad.me/${clientId}`} />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:image" content="https://theunad.com/logo-wide.svg" />
        <meta
          name="description"
          content={t('page.title', { name }) as string}
        />
      </Head>
      <section className="app" data-testid="Subscribe__container">
        <div className="container-app h100 d-flex align-items-center">
          <div className="col-12 text-center">
            <div className="box box-1">
              <div>
                <Image // TODO: This should be a business logo
                  src="/img/UnAd-Logo-Horizontal.svg"
                  className="img-fluid logo"
                  width={200 * 2}
                  height={150 * 2}
                  alt="UnAd - Marketing made simple"
                />
                <h1 className="primary mb-1">{t('header.title', { name })}</h1>
                <div className="mb-4">
                  <p className="mb-0">{t('header.p1')}</p>
                  <p> {t('header.p2')}</p>
                </div>

                <p className="links">
                  {t.rich('links.learnMore', {
                    link: (text) => (
                      <Link
                        href={`/about?clientId=${clientId}`}
                        key="0"
                        className="link"
                      >
                        {text}
                      </Link>
                    ),
                  })}
                </p>
              </div>

              {!phoneNumber && <SubscribeForm onSubmit={clickGetOtp} />}

              {phoneNumber && <OtpForm onSubmit={clickValidate} />}

              {error && (
                <div className="col-12">
                  <div className="form-message text-center" id="form-message">
                    <p data-testid="Subscribe__error" className="quaternary">
                      {error}
                    </p>
                  </div>
                </div>
              )}
            </div>
            <div className="col-12 text-center mt-4">
              <p className="primary mb-0">{t('footer.p', { name })}</p>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

export async function getServerSideProps(
  context: GetServerSidePropsContext<ServerProps>
) {
  const { clientId } = context.params as ServerProps;
  try {
    const client = await prisma.client.findUnique({
      where: { id: clientId },
      select: { name: true },
    });

    if (!client) {
      return {
        notFound: true,
      };
    }
    return {
      props: {
        messages: await importMessages(context.locale),
        name: client.name,
        clientId,
      },
    };
  } catch (error: any) {
    return {
      props: {
        messages: await importMessages(context.locale),
        error: {
          message:
            process.env.NODE_ENV === 'development'
              ? error.stack ?? error.message
              : error.message,
          stack: error.stack,
          statusCode: 500,
        },
      },
    };
  }
}

export default Subscribe;
