import { GetServerSidePropsContext } from 'next';
import Head from 'next/head';
import Image from 'next/image';
import Link from 'next/link';
import { useRouter } from 'next/router';
import { useTranslations } from 'next-intl';
import { useCallback, useState } from 'react';
import { CookiesProvider, useCookies } from 'react-cookie';

import OtpForm, { type OtpFormData } from '@/Components/OtpForm';
import PhoneNumberForm, {
  type PhoneNumberData,
} from '@/Components/PhoneNumberForm';
import * as api from '@/lib/api';
import { importMessages } from '@/lib/i18n';
import { verifyJwt } from '@/lib/jwt';
import { createModelFactory } from '@/lib/redis';
import { sanitizePhoneNumber } from '@/lib/util';

interface LoginProps {}

function Login() {
  const t = useTranslations('pages/account/index');
  const [error, setError] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const router = useRouter();
  const [_c, setCookie, _r] = useCookies(['token']);

  const clickGetOtp = async ({ phone }: PhoneNumberData) => {
    const formattedPhone = sanitizePhoneNumber(phone);
    try {
      await api.generateOtp(formattedPhone);
      setPhoneNumber(formattedPhone);
    } catch (err: any) {
      setError(err.message);
    }
  };

  const clickLogin = useCallback(
    async ({ otp }: OtpFormData) => {
      try {
        const { token } = await api.login(phoneNumber, otp);
        const { sub: clientId } = await api.validateJwt(token);
        setCookie('token', token, {
          path: '/',
          sameSite: true,
          secure: true,
          maxAge: Number(process.env.NEXT_PUBLIC_SESSION_LENGTH),
        });
        router.push(`/account/${clientId}`);
      } catch (err: any) {
        setError(err.message);
      }
    },
    [phoneNumber, router, setCookie]
  );

  return (
    <>
      <Head>
        <title>{t('title')}</title>
        <meta property="og:title" content="Log in to your UnAd Account" />
        <meta property="og:locale" content="en_US" />
        <meta property="og:type" content="website" />
        <meta property="og:url" content={`https://unad.tech/account`} />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:image" content="https://theunad.com/logo-wide.svg" />
        <meta name="description" content="Log in to your UnAd Account" />
      </Head>
      <section className="app" data-testid="Login__container">
        <div className="container-app h100 d-flex align-items-center">
          <div className="col-12 text-center">
            <div className="box box-1">
              <div>
                <Image
                  src="/img/UnAd-Logo-Horizontal.svg"
                  className="img-fluid logo"
                  width={200}
                  height={150}
                  alt="UnAd - Marketing made simple"
                />
                <h1 className="primary mb-1" data-testid="Login__heading">
                  {t('header.h1')}
                </h1>
              </div>

              {!phoneNumber && <PhoneNumberForm onSubmit={clickGetOtp} />}
              {phoneNumber && <OtpForm onSubmit={clickLogin} />}

              {error && (
                <div className="col-12">
                  <div className="form-message text-center" id="form-message">
                    <p data-testid="Login__error" className="quaternary">
                      {error}
                    </p>
                  </div>
                </div>
              )}
              <hr />
              <div className="col-12">
                <div className="text-center">
                  <p className="ternary">
                    {t.rich('links.register', {
                      link: (text) => (
                        <Link
                          className="links quaternary"
                          href={'/register'}
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          {text}
                        </Link>
                      ),
                    })}
                  </p>
                </div>
              </div>
            </div>
            <div className="col-12 text-center mt-4">
              <p className="primary mb-0">{t('footer.p')}</p>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

function LoginWrapper(props: LoginProps) {
  return (
    <CookiesProvider>
      <Login {...props} />
    </CookiesProvider>
  );
}

export default LoginWrapper;

export async function getServerSideProps(context: GetServerSidePropsContext) {
  const token = context.req.cookies.token;
  if (!token) {
    return {
      props: {
        messages: await importMessages(context.locale),
      },
    };
  }

  try {
    using models = createModelFactory();
    await models.connect();
    const jwt = await models.getSession(token);
    if (!jwt) {
      return {
        props: {
          messages: await importMessages(context.locale),
        },
      };
    }
    const { sub } = await verifyJwt(jwt);
    return {
      redirect: {
        destination: `/account/${sub}`,
        permanent: false,
      },
    };
  } catch (err: any) {
    return {
      props: {
        error: err.message,
        statusCode: 500,
      },
    };
  }
}
