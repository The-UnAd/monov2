import { Head, Html, Main, NextScript } from 'next/document';

export default function MyDocument() {
  return (
    <Html>
      <Head>
        <meta name="theme-color" content="#111111" />
        <meta name="msapplication-navbutton-color" content="#111111" />
        <meta name="apple-mobile-web-app-capable" content="yes" />
        <meta name="apple-mobile-web-app-status-bar-style" content="#111111" />
        <link rel="apple-touch-icon" href="/img/favicon.webp" />
        <link rel="icon" type="image/x-icon" href="/img/favicon.webp" />
        <meta name="theme-color" content="#111111" />
        <meta name="msapplication-navbutton-color" content="#111111" />
        <meta name="apple-mobile-web-app-capable" content="yes" />
        <meta name="apple-mobile-web-app-status-bar-style" content="#111111" />
        <link rel="apple-touch-icon" href="/img/favicon.webp" />
        <link rel="icon" type="image/x-icon" href="/img/favicon.webp" />
        <link rel="preconnect" href="https://fonts.gstatic.com" />
      </Head>
      <body>
        <Main />
        <NextScript />
      </body>
    </Html>
  );
}
