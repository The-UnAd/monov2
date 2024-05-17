import { Head, Html, Main, NextScript } from 'next/document';
import Script from 'next/script';

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
        <Script
          async
          strategy="lazyOnload"
          src="https://www.googletagmanager.com/gtag/js?id=G-L2SWP1F1TF"
        ></Script>
        <Script id="ga">
          {`
            window.dataLayer = window.dataLayer || [];
            function gtag(){dataLayer.push(arguments);}
            gtag('js', new Date());
            gtag('config', 'G-L2SWP1F1TF');
          `}
        </Script>
        <Script id="clarity">
          {`
            (function(c,l,a,r,i,t,y){
              c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};
              t=l.createElement(r);t.async=1;t.src="https://www.clarity.ms/tag/"+i;
              y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y);
            })(window, document, "clarity", "script", "mbjvc7zj0r");
          `}
        </Script>
      </Head>
      <body>
        <noscript>
          <iframe
            src="https://www.googletagmanager.com/ns.html?id=GTM-PKLDLT6H"
            height="0"
            width="0"
            style={{ display: 'none', visibility: 'hidden' }}
          ></iframe>
        </noscript>
        <Main />
        <NextScript />
      </body>
    </Html>
  );
}
