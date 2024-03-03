import 'bootstrap/dist/css/bootstrap.css';
import './sass/main.scss';

import type { AppProps } from 'next/app';
import Error from 'next/error';
import { useRouter } from 'next/router';
import { NextIntlClientProvider } from 'next-intl';

function MyApp({ Component, pageProps }: AppProps) {
  const router = useRouter();
  if (pageProps.error) {
    if (pageProps.error) {
      return (
        <Error
          statusCode={pageProps.error.statusCode}
          title={
            process.env.NODE_ENV === 'development'
              ? pageProps.error.stack ?? pageProps.error.message
              : pageProps.error.message
          }
        />
      );
    }
  }
  return (
    <NextIntlClientProvider
      // To achieve consistent date, time and number formatting
      // across the app, you can define a set of global formats.
      formats={{
        dateTime: {
          short: {
            day: 'numeric',
            month: 'short',
            year: 'numeric',
          },
        },
      }}
      locale={router.locale}
      // Messages can be received from individual pages or configured
      // globally in this module (`App.getInitialProps`). Note that in
      // the latter case the messages are available as a top-level prop
      // and not nested within `pageProps`.
      messages={pageProps.messages}
      // Providing an explicit value for `now` ensures consistent formatting of
      // relative values regardless of the server or client environment.
      now={new Date(pageProps.now)}
      timeZone={Intl.DateTimeFormat().resolvedOptions().timeZone}
    >
      <Component {...pageProps} />
    </NextIntlClientProvider>
  );
}

export default MyApp;
