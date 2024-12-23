import Head from 'next/head';
import Image from 'next/image';
import Link from 'next/link';
import { useRouter } from 'next/router';
import type { GetServerSidePropsContext } from 'next/types';
import { useTranslations } from 'next-intl';
import type { ParsedUrlQuery } from 'querystring';
import React, { useCallback, useEffect, useState } from 'react';

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
  slug: string;
}

export const CountdownSeconds = 30;

function Countdown({
  seconds,
  children,
  message,
  done,
}: React.PropsWithChildren<{
  seconds: number;
  message: (c: number) => string;
  done?: () => void;
}>) {
  const [count, setCount] = useState(seconds);
  useEffect(() => {
    const interval = setInterval(() => {
      setCount((c) => c - 1);
    }, 1000);
    return () => {
      setCount(seconds);
      done?.();
      clearInterval(interval);
    };
  }, [done, seconds]);
  return (
    <div data-testid="Subscribe__countdown">
      {count > 0 ? message(count) : children}
    </div>
  );
}

function Subscribe({ name, clientId }: Readonly<SubscribeProps>) {
  const [error, setError] = useState('');
  const router = useRouter();
  const [phoneNumber, setPhoneNumber] = useState('');
  const t = useTranslations('pages/subscribe/[slug]');
  const subscribeTFunc = useTranslations('Components/SubscribeForm');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [countdown, setCountdown] = useState(CountdownSeconds);

  const clickGetOtp = async ({ phone }: SubscribeData) => {
    setIsSubmitting(true);
    const formattedPhone = sanitizePhoneNumber(phone);
    try {
      setPhoneNumber(formattedPhone);
      await generateOtp(formattedPhone);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setIsSubmitting(false);
      setCountdown(CountdownSeconds);
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
        <meta property="og:title" content={t('page.title', { name })} />
        <meta property="og:description" content={t('page.title', { name })} />
        <meta property="og:locale" content="en_US" />
        <meta property="og:type" content="website" />
        <meta property="og:url" content={`//unad.me/${clientId}`} />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:image" content="https://theunad.com/logo-wide.svg" />
        <meta name="description" content={t('page.title', { name })} />
      </Head>
      <section className="app" data-testid="Subscribe__container">
        <div className="container-app h95 d-flex align-items-center">
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

              {!phoneNumber && (
                <SubscribeForm
                  data-testid="Subscribe__SubscribeForm"
                  tFunc={subscribeTFunc}
                  onSubmit={clickGetOtp}
                />
              )}

              {phoneNumber && (
                <>
                  <OtpForm
                    tFunc={(k) => t(`OtpForm.${k}`)}
                    onSubmit={clickValidate}
                  />

                  <p>
                    {t('missingCode')}&nbsp;
                    <Countdown
                      data-testid="Subscribe__countdown"
                      seconds={countdown}
                      message={(time) => t('countdown', { time })}
                      done={() => setCountdown(0)}
                    >
                      <button
                        onClick={() =>
                          void clickGetOtp({
                            phone: phoneNumber.slice(-10),
                            terms: true,
                          })
                        }
                        data-testid="Subscribe__resend"
                        className="links"
                      >
                        {isSubmitting
                          ? t('buttons.resend.loading')
                          : t('buttons.resend.unpressed')}
                      </button>
                    </Countdown>
                  </p>
                </>
              )}

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
  const { slug } = context.params as ServerProps;
  try {
    const client = await prisma.client.findUnique({
      where: { slug },
      select: { name: true, id: true },
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
        clientId: client.id,
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
