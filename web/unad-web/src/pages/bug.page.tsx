import Head from 'next/head';
import Image from 'next/image';
import Script from 'next/script';
import type { GetServerSidePropsContext } from 'next/types';
import { useTranslations } from 'next-intl';
import type { ParsedUrlQuery } from 'querystring';

import { importMessages } from '@/lib/i18n';

type PageData = Readonly<{
  pricingTableId: string;
}>;

type ServerProps = ParsedUrlQuery;

function PayBug({ pricingTableId }: PageData) {
  const t = useTranslations('pages/pay/[clientId]');
  return (
    <>
      <Head>
        <title>{t('title')}</title>
      </Head>
      <Script async src="https://js.stripe.com/v3/pricing-table.js" />
      <section className="app">
        <div className="container-app h95 d-flex align-items-center">
          <div className="col-12 text-center">
            <div className="box box-1">
              <div>
                <Image
                  src="/img/UnAd-Logo-Horizontal.svg"
                  height={150}
                  width={200}
                  className="img-fluid logo"
                  alt={t('title')}
                />
                <h1 className="primary mb-1">{t('h1')}</h1>
                <div className="mb-4">
                  <p>{t('p1')}</p>
                </div>
                <div className="mb-4">
                  <p>
                    {t.rich('p2', {
                      link: (text) => (
                        <a
                          key="0"
                          className="links link-4"
                          href={''}
                          rel="noopener noreferrer"
                        >
                          {text}
                        </a>
                      ),
                    })}
                  </p>
                </div>
              </div>
              <div className="my-2">
                <stripe-pricing-table
                  pricing-table-id={pricingTableId}
                  publishable-key={process.env.NEXT_PUBLIC_STRIPE_PUBLIC_KEY}
                ></stripe-pricing-table>
              </div>
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
  return {
    props: {
      messages: await importMessages(context.locale),
      pricingTableId: process.env.STRIPE_PRODUCT_BASIC_PRICING_TABLE,
    },
  };
}

export default PayBug;
