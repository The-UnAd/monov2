import { ParsedUrlQuery } from 'node:querystring';

import { GetServerSidePropsContext } from 'next';
import Head from 'next/head';
import { useTranslations } from 'next-intl';

import { createTranslator, importMessages } from '@/lib/i18n';
import { createModelFactory } from '@/lib/redis';

interface PageData {
  name: string;
  locale: string;
}
interface ServerProps extends ParsedUrlQuery {
  code: string;
}

function Success({ name, locale }: PageData) {
  const t = useTranslations('pages/subscribe/success/[code]');
  return (
    <>
      <Head>
        <meta
          name="viewport"
          content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
        />
        <title>{t('page.title', { name })}</title>
        <meta property="og:title" content={`UnAd`} />
        <meta
          property="og:description"
          content={t('page.description', { name })}
        />
        <meta property="og:locale" content={locale} />
        <meta property="og:type" content="website" />
        <meta property="og:site_name" content="UnAd" />
        <meta property="og:image" content="https://theunad.com/logo-wide.svg" />
        <meta property="og:url" content="https://theunad.com/" />
        <meta name="description" content={t('page.description', { name })} />
      </Head>
      <section className="app">
        <div className="container-app h100 d-flex align-items-center">
          <div className="col-12 text-center">
            <div className="box box-1">
              <div>
                <h1 className="primary mb-1">{t('h1', { name })}</h1>
                <div className="mb-2">
                  <p>{t('p', { name })}</p>
                </div>
                <hr />
                <div className="mb-2">
                  <h3 className="white">{t('mb2.h3')}</h3>
                  {[...Array(2).keys()].map((i) => (
                    <p key={i} className="primary mb-0">
                      - {t(`mb2.list.${i}`)}
                    </p>
                  ))}
                </div>
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
  const t = await createTranslator(
    context.req,
    'pages/subscribe/success/[code]'
  );
  const { code } = context.params as ServerProps;

  using models = createModelFactory();
  await models.connect();
  try {
    const phone = await models.getClientPhoneFromSubscriberConfirmation(
      code as string
    );
    const client = await models.getClientByPhone(phone);
    if (!client) {
      return {
        notFound: true,
      };
    }
    return {
      props: {
        messages: await importMessages(context.locale),
        name: client.name,
        locale: context.locale,
      },
    };
  } catch (error: any) {
    console.error('error in pages/subscribe/success/[code]', error);
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

export default Success;
