import { NextIntlClientProvider } from 'next-intl';

import { DefaultLocale } from '@/lib/i18n';

import messages from '../messages/en-US.json'; // TODO: maybe make this dynamic?

export function withIntl<T>(
  WrappedComponent: React.ComponentType<T>,
  locale = DefaultLocale
) {
  const ComponentWithIntl = (props: T) => {
    return (
      <NextIntlClientProvider locale={locale} messages={messages}>
        <WrappedComponent {...(props as JSX.IntrinsicAttributes & T)} />
      </NextIntlClientProvider>
    );
  };

  return ComponentWithIntl;
}
