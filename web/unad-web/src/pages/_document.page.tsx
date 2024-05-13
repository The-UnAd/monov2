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
        <script type="text/javascript">
          (function(c,l,a,r,i,t,y){
            c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};
            t=l.createElement(r);t.async=1;t.src="https://www.clarity.ms/tag/"+i;
            y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y);
          })(window, document, "clarity", "script", "mbjvc7zj0r");
        </script>
        <Script
          async
          src="https://www.googletagmanager.com/gtag/js?id=G-L2SWP1F1TF"
        ></Script>
        <script>
          window.dataLayer = window.dataLayer || [];
          function gtag(){dataLayer.push(arguments);}
          gtag('js', new Date());
          gtag('config', 'G-L2SWP1F1TF');
        </script>
      </Head>
      <body>
        <Main />
        <NextScript />
      </body>
    </Html>
  );
}
