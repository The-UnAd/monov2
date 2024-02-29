import Head from 'next/head';
import Image from 'next/image';
import Link from 'next/link';
import type { GetServerSidePropsContext } from 'next/types';
import { useTranslations } from 'next-intl';
import type { ParsedUrlQuery } from 'querystring';

import { prisma } from '@/lib/db';
import { createTranslator, importMessages } from '@/lib/i18n';
import { verifyJwt } from '@/lib/jwt';
import { createModelFactory } from '@/lib/redis';

export interface AccountProps {
  name: string;
  subscribers: number;
  announcementCount: number;
  portalUrl: string;
  subscribeUrl: string;
}

interface ServerProps extends ParsedUrlQuery {
  clientId: string;
}

// TODO: show subscription link

function Account({
  name,
  subscribers,
  announcementCount,
  portalUrl,
  subscribeUrl,
}: AccountProps) {
  const t = useTranslations('pages/account/[clientId]');
  return (
    <>
      <Head>
        <title>{t('title', { name })}</title>
      </Head>
      <section className="app" data-testid="Account__container">
        <div className="container-app h100 d-flex align-items-center">
          <div className="col-12 text-center">
            <div className="box box-1">
              <div>
                <h1 className="primary mb-1" data-testid="Account__heading">
                  {t('header.h1')}
                </h1>
                <div className="mb-4">
                  <p className="mb-0">{t('subscribers', { subscribers })}</p>
                  <p>{t('announcementCount', { announcementCount })}</p>
                </div>
              </div>
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
                  {t('links.portal.text')}&emsp;
                  <Link target="_blank" rel="noreferrer" href={portalUrl}>
                    {portalUrl}
                  </Link>
                </p>
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
  const { clientId } = context.params as ServerProps;
  try {
    const client = await prisma.client.findUnique({
      where: { id: clientId },
    });
    if (!client) {
      return {
        redirect: {
          destination: '/account',
          permanent: false,
        },
      };
    }
    const token = context.req.cookies.token;
    if (!token) {
      return {
        redirect: {
          destination: '/account',
          permanent: false,
        },
      };
    }
    using models = createModelFactory();
    await models.connect();
    const jwt = await models.getSession(token);
    if (jwt !== null) {
      const { sub } = await verifyJwt(jwt);
      if (sub === clientId) {
        const subscribers = await prisma.client_subscriber.count({
          where: { client_id: clientId },
        });
        const announcementCount = await prisma.announcement.count({
          where: { client_id: clientId },
        });
        return {
          props: {
            messages: await importMessages(context.locale),
            name: client.name,
            subscribers,
            announcementCount,
            portalUrl: `${process.env.STRIPE_PORTAL_HOST}`,
            subscribeUrl: `${process.env.SHARE_HOST}/${clientId}`,
          },
        };
      }
    }
    const t = await createTranslator(context.req, 'pages/account/[clientId]');
    return {
      props: {
        error: {
          statusCode: 401,
          message: t('errors.noPermission'),
        },
      },
    };
  } catch (err: any) {
    return {
      props: {
        error: {
          statusCode: err.statusCode || 500,
          message: err.message,
        },
      },
    };
  }
}

export default Account;
