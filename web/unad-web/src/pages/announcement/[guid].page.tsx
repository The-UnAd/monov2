import Head from 'next/head';
import Image from 'next/image';
import type { GetServerSidePropsContext } from 'next/types';
import { useTranslations } from 'next-intl';
import { ParsedUrlQuery } from 'querystring';

import { importMessages } from '@/lib/i18n';
import { createModelFactory } from '@/lib/redis';
import { getSmsBySid } from '@/lib/twilio';

interface PageData {
  message: string;
  clientName: string;
}

interface ServerProps extends ParsedUrlQuery {
  guid: string;
}

function Announcement({ message, clientName }: PageData) {
  const t = useTranslations('pages/announcement/[guid]');
  return (
    <>
      <Head>
        <title>{t('page.title')}</title>
      </Head>
      <section className="app" data-testid="Announcement__container">
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
                  <p>
                    {t.rich('template', {
                      name: clientName,
                      message,
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

export async function getServerSideProps(
  context: GetServerSidePropsContext<ServerProps>
) {
  const { guid } = context.params as ServerProps;
  try {
    using models = createModelFactory();
    await models.connect();
    const smsSid = await models.getAnnouncementSmsSidByGuid(guid);
    if (!smsSid) {
      return {
        notFound: true,
      };
    }
    const message = await getSmsBySid(smsSid);
    const client = await models.getClientByPhone(message.from);
    if (!client) {
      return {
        notFound: true,
      };
    }

    return {
      props: {
        messages: await importMessages(context.locale),
        message: message.body,
        clientName: client.name,
      },
    };
  } catch (error: any) {
    return {
      props: {
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

export default Announcement;
