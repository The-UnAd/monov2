import type { GetServerSidePropsContext } from 'next';
import Head from 'next/head';
import Image from 'next/image';
import Link from 'next/link';
import { useTranslations } from 'next-intl';

import { importMessages } from '@/lib/i18n';

function Index() {
  const t = useTranslations('pages/index');

  return (
    <>
      <Head>
        <meta
          name="viewport"
          content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
        />
        <title>{t('title')}</title>
        <meta property="og:title" content={t('title')} />
        <meta property="og:description" content={t('title')} />
        <meta property="og:locale" content="en_US" />
        <meta property="og:type" content="website" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:image" content="https://theunad.com/logo-wide.svg" />
        <meta property="og:url" content="https://unad.dev" />
        <meta name="description" content={t('title')} />
      </Head>
      <section className="app">
        <div className="container-app h100 d-flex align-items-center">
          <div className="col-12 text-center">
            <div className="box box-1">
              <div>
                <Image
                  src="/img/UnAd-Logo-Horizontal.svg"
                  className="img-fluid logo"
                  width={5600}
                  height={375}
                  alt={t('tagline')}
                />
                <div className="mb-4">
                  <p className="mb-0">{t('header.mb0')}</p>
                </div>
                <p className="links">
                  {t.rich('links.learnMore', {
                    link: (text) => (
                      <Link href="https://theunad.com" key="0" className="link">
                        {text}
                      </Link>
                    ),
                  })}
                </p>
              </div>
              <div className="my-2">
                <Link href="/register" className="btn btn-lg btn-1">
                  {t('register.btn')}
                </Link>
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
}: GetServerSidePropsContext) {
  try {
    return {
      props: {
        messages: await importMessages(locale),
      },
    };
  } catch (error: any) {
    console.error(error);
    return {
      props: {
        error: {
          message:
            process.env.NODE_ENV === 'development'
              ? error.stack ?? error.message
              : error.message,
          statusCode: 500,
          stack: error.stack,
        },
      },
    };
  }
}

export default Index;
