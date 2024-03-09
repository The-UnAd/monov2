import { ParsedUrlQuery } from 'node:querystring';

import { GetServerSidePropsContext } from 'next';
import Image from 'next/image';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import Stripe from 'stripe';

import { prisma } from '@/lib/db';
import { createTranslator, importMessages } from '@/lib/i18n';
import { createModelFactory } from '@/lib/redis';

interface PageData {
  accountUrl: string;
  portalUrl: string;
  subscribeUrl: string;
}
interface ServerProps extends ParsedUrlQuery {
  session_id: string;
}

// TODO: show subscription link

function Pay({ accountUrl, portalUrl, subscribeUrl }: PageData) {
  const t = useTranslations('pages/pay/success');
  return (
    <section className="app">
      <div className="container-app h100 d-flex align-items-center">
        <div className="col-12 text-center">
          <div className="box box-1">
            <div>
              <h1 className="primary mb-1">{t('h1')}</h1>
              <h3 className="white">{t('h3')}</h3>
              <div className="mb-2">
                <p>{t('p')}</p>
              </div>
              <hr />
              <div className="mb-2">
                <h3 className="white">{t('mb2.h3')}</h3>
                {[...Array(4).keys()].map((i) => (
                  <p key={i} className="primary mb-0">
                    - {t(`mb2.list.${i}`)}
                  </p>
                ))}
              </div>
              <hr />
              <div className="mb-2">
                <div className="business-link">
                  <p className="white">
                    {t('links.subscribe.text')}&emsp;
                    <Link target="_blank" rel="noreferrer" href={subscribeUrl}>
                      {subscribeUrl}
                    </Link>
                  </p>
                  <p className="white">{t('links.subscribe.description')}</p>
                </div>
              </div>
              <div className="mb-2">
                <div className="business-link">
                  <p className="white">
                    <Image
                      src={`/api/qr?content=${subscribeUrl}`}
                      width={200}
                      height={200}
                      alt="QR Code"
                    />
                  </p>
                  <p className="white">{t('links.qr.description')}</p>
                </div>
              </div>
              <div className="mb-2">
                <div className="business-link">
                  <p className="white">
                    {t('links.account.text')}&emsp;
                    <Link target="_blank" rel="noreferrer" href={accountUrl}>
                      {accountUrl}
                    </Link>
                  </p>
                </div>
              </div>
              <div className="mb-2">
                <div className="business-link">
                  <p className="white">
                    {t('links.portal.text')}&emsp;
                    <Link target="_blank" rel="noreferrer" href={portalUrl}>
                      {portalUrl}
                    </Link>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

export async function getServerSideProps(
  context: GetServerSidePropsContext<ServerProps>
) {
  const t = await createTranslator(context.req, 'pages/pay/[clientId]');
  const { session_id } = context.query as ServerProps;

  const stripe = new Stripe(process.env.STRIPE_API_KEY as string, {
    apiVersion: '2023-10-16',
  });
  using models = createModelFactory();
  await models.connect();

  const session = await stripe.checkout.sessions.retrieve(session_id);
  if (!session) {
    return {
      props: {
        error: {
          statusCode: 404,
          message: t('errors.sessionNotFound'),
        },
      },
    };
  }

  try {
    const client = await prisma.client.findUnique({
      where: { id: session.client_reference_id! },
    });
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

    return {
      props: {
        messages: await importMessages(context.locale),
        accountUrl: `${process.env.SITE_HOST}/account`,
        portalUrl: `${process.env.STRIPE_PORTAL_HOST}`,
        subscribeUrl: `${process.env.SUBSCRIBE_HOST}/${session.client_reference_id}`,
      },
    };
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
