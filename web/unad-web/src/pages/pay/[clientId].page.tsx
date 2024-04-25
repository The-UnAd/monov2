import type { price_tier as PriceTier, Prisma } from '@unad/product-models';
import Head from 'next/head';
import Image from 'next/image';
import type { GetServerSidePropsContext } from 'next/types';
import { useTranslations } from 'next-intl';
import type { ParsedUrlQuery } from 'querystring';

import ProductTable from '@/Components/ProductTable';
import { ProductDb, UserDb } from '@/lib/db';
import { createTranslator, importMessages } from '@/lib/i18n';

type PlanWithTier = Prisma.planGetPayload<{
  include: { price_tier: true };
}>;

type PageData = Readonly<{
  plans: PlanWithTier[];
}>;

type ServerProps = ParsedUrlQuery & {
  clientId: string;
};

function Pay({ plans }: PageData) {
  const t = useTranslations('pages/pay/[clientId]');

  const onSelect = (tier: PriceTier) => {
    console.log('Selected tier:', tier);
  };

  return (
    <>
      <Head>
        <title>{t('title')}</title>
      </Head>
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
                <ProductTable plans={plans} onSelect={onSelect} />
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
  const t = await createTranslator(context.req, 'pages/pay/[clientId]');
  const { clientId } = context.params as ServerProps;

  try {
    const client = await UserDb.client.findUnique({ where: { id: clientId } });
    if (!client) {
      return {
        props: {
          error: {
            statusCode: 404,
            message: t('errors.clientNotFound'),
          },
        },
      };
    }

    if (!client.subscription_id) {
      // this is the happy path: client created, but no subscription

      const plans = await ProductDb.plan.findMany({
        where: { status: 'active' },
        include: {
          price_tier: true,
        },
      });

      return {
        props: {
          messages: await importMessages(context.locale),
          clientId,
          plans,
        },
      };
    }
    const subscription = await ProductDb.plan_subscription.findMany({
      where: { client_id: client.id, status: 'active' },
    });
    if (subscription.length > 0) {
      return {
        props: {
          error: {
            statusCode: 400,
            message: t('errors.subscriptionActive'),
          },
        },
      };
    }
  } catch (error: any) {
    console.error(error);
    return {
      props: {
        error: {
          statusCode: 500,
          message:
            process.env.NODE_ENV === 'development'
              ? error.stack ?? error.message
              : error.message,
        },
      },
    };
  }
}

export default Pay;
