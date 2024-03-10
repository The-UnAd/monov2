import type { GetStaticPropsContext } from 'next';
import Head from 'next/head';
import Image from 'next/image';
import Link from 'next/link';
import { useRouter } from 'next/router';
import { useTranslations } from 'next-intl';
import { useState } from 'react';

import { importMessages } from '@/lib/i18n';
import { sanitizePhoneNumber } from '@/lib/util';

import OtpForm, { OtpFormData } from '../Components/OtpForm';
import type { RegisterData } from '../Components/RegisterForm';
import RegisterForm from '../Components/RegisterForm';

function Register() {
  const [registerData, setRegisterData] = useState<RegisterData>();
  const [error, setError] = useState<string | null>();
  const router = useRouter();
  const t = useTranslations('pages/register');

  const clickRegister = async ({ phone, name }: RegisterData) => {
    setError(null);
    const resp = await fetch(`/api/register/${encodeURIComponent(phone)}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        phone: sanitizePhoneNumber(phone),
        name: name.trim(),
      }),
    });
    const data = await resp.json();

    if (resp.ok) {
      setRegisterData(data);
    } else {
      setError(data.message);
    }
  };

  const clickValidate = async ({ otp }: OtpFormData) => {
    setError(null);
    const resp = await fetch('/api/register/validate', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        otp,
        ...registerData,
      }),
    });

    if (resp.ok) {
      const data = await resp.json();
      router.push(`/pay/${data.clientId}`);
    } else {
      const data = await resp.json();
      setError(data.message);
    }
  };

  return (
    <>
      <Head>
        <meta
          name="viewport"
          content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
        />
        <title>UnAd - Marketing Made Simple</title>
        <meta property="og:title" content={`Register for UnAd`} />
        <meta property="og:description" content="Marketing made simple" />
        <meta property="og:locale" content="en_US" />
        <meta property="og:type" content="website" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:image" content="https://theunad.com/logo-wide.svg" />
        <meta property="og:url" content="https://unad.dev" />
        <meta name="description" content="Marketing made simple" />
      </Head>
      <section className="app">
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
                <h1 className="primary mb-1">{t('header.h1')}</h1>
                <div className="mb-4">
                  <p className="mb-0">{t('header.mb0')}</p>
                  <p>{t('header.p')}</p>
                </div>
              </div>
              {!registerData && <RegisterForm onSubmit={clickRegister} />}
              {registerData && (
                <OtpForm
                  tFunc={(k) => t(`OtpForm.${k}`)}
                  onSubmit={clickValidate}
                />
              )}
              {error && (
                <div className="col-12">
                  <div className="form-message text-center" id="form-message">
                    <p data-testid="Register__error" className="quaternary">
                      {error}
                    </p>
                  </div>
                </div>
              )}
              <hr />
              <div className="col-12">
                <div className="text-center">
                  <p className="ternary">
                    {t.rich('links.account', {
                      link: (text) => (
                        <Link
                          data-testid="Register__accountLink"
                          className="links quaternary"
                          href={'/account'}
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

export async function getStaticProps({ locale }: GetStaticPropsContext) {
  return {
    props: {
      messages: await importMessages(locale),
    },
  };
}

export default Register;
