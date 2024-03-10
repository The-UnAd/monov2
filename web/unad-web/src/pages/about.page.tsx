import type { GetServerSidePropsContext } from 'next';
import Head from 'next/head';
import Image from 'next/image';
import Link from 'next/link';
import { useTranslations } from 'next-intl';

import { importMessages } from '@/lib/i18n';

function About({ locale, subLink }: { locale: string; subLink: string }) {
  const t = useTranslations('pages/about');

  return (
    <>
      <Head>
        <meta
          name="viewport"
          content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
        />
        <title>{t('title')}</title>
        <meta property="og:title" content={`UnAd`} />
        <meta property="og:description" content={t('tagline')} />
        <meta property="og:locale" content={locale} />
        <meta property="og:type" content="website" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:image" content="https://theunad.com/logo-wide.svg" />
        <meta property="og:url" content="https://unad.dev/about" />
        <meta name="description" content={t('tagline')} />
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
                  alt={t('title')}
                />
                <h1
                  className="primary mb-1"
                  style={{
                    textTransform: 'initial',
                  }}
                >
                  {t('header.h1')}
                </h1>
                <div className="mb-4">
                  <p className="mb-0 about-p">{t('header.mb0.p1')}</p>
                  <p className="mb-0 about-p">{t('header.mb0.p2')}</p>
                  <p className="mb-0 about-p">{t('header.mb0.p3')}</p>
                </div>
                {subLink && (
                  <Link href={subLink} className="btn btn-lg btn-1">
                    {t('sign-up.btn')}
                  </Link>
                )}
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

export async function getServerSideProps({
  locale,
  query: { clientId },
}: GetServerSidePropsContext) {
  return {
    props: {
      locale,
      subLink: clientId ? `${process.env.SUBSCRIBE_HOST}/${clientId}` : null,
      messages: await importMessages(locale),
    },
  };
}

export default About;
