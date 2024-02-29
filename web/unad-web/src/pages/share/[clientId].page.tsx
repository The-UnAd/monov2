import Head from 'next/head';
import Image from 'next/image';
import type { GetServerSidePropsContext } from 'next/types';
import { useTranslations } from 'next-intl';
import { ParsedUrlQuery } from 'querystring';

import { prisma } from '@/lib/db';
import { importMessages } from '@/lib/i18n';

interface PageData {
  subscribeUrl: string;
}

interface ServerProps extends ParsedUrlQuery {
  clientId: string;
}

function SharePage({ subscribeUrl }: PageData) {
  const t = useTranslations('pages/share/[clientId]');
  return (
    <>
      <Head>
        <title>{t('page.title')}</title>
      </Head>
      <section className="app" data-testid="Subscribe__container">
        <div className="container-app h100 d-flex align-items-center">
          <div className="col-12 text-center">
            <div className="box box-1">
              <div>
                {/* TODO: this should be the business logo  */}
                <Image
                  src="/img/UnAd-Logo-Horizontal.svg"
                  className="img-fluid logo"
                  width={200}
                  height={150}
                  alt="UnAd - Marketing made simple"
                />
                <h1 className="primary mb-1">{t('title')}</h1>
                <div className="mb-4">
                  <a target="_blank" rel="noreferrer" href={subscribeUrl}>
                    <Image
                      src={`/api/qr?content=${encodeURIComponent(
                        subscribeUrl
                      )}`}
                      alt="QR Code"
                      width={250}
                      height={250}
                    />
                  </a>
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

export async function getServerSideProps(
  context: GetServerSidePropsContext<ServerProps>
) {
  const { clientId } = context.params as ServerProps;
  try {
    const client = await prisma.client.findUnique({ where: { id: clientId } });
    if (!client) {
      return {
        notFound: true,
      };
    }
    return {
      props: {
        messages: await importMessages(context.locale),
        subscribeUrl: `${process.env.SUBSCRIBE_HOST}/${clientId}`,
      },
    };
  } catch (err: any) {
    console.error(err);
    return {
      error: {
        message: err.message,
        stack: err.stack,
        statusCode: 500,
      },
    };
  }
}

export default SharePage;
