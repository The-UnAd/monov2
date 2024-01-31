import { GetServerSidePropsContext } from 'next';
import { useTranslations } from 'next-intl';

import { importMessages } from '@/lib/i18n';

function Canceled() {
  const t = useTranslations('pages/pay/canceled');
  return (
    <div>
      <div>
        <h3 data-testid="Register__heading">{t('h3')}</h3>
        <div>
          <p>{t('p')}</p>
        </div>
      </div>
    </div>
  );
}

export async function getServerSideProps(context: GetServerSidePropsContext) {
  return {
    props: {
      messages: await importMessages(context.locale),
    },
  };
}

export default Canceled;
