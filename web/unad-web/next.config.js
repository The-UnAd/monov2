const nextConfig = {
  reactStrictMode: true,
  images: {
    unoptimized: true,
  },
  pageExtensions: [
    // `.page.tsx` for page components
    'page.tsx',
    // `.api.ts` for API routes
    'api.ts',
  ],
  typescript: {
    tsconfigPath: './tsconfig.build.json',
  },
  i18n: {
    locales: ['en-US', 'es'],
    defaultLocale: 'en-US',
    domains: [
      {
        domain: 'unad.tech',
        defaultLocale: 'en-US',
        locales: ['en-US', 'es'],
      },
      {
        domain: 'es.unad.tech',
        defaultLocale: 'es',
      },
    ],
  },
  eslint: {
    // Warning: This allows production builds to successfully complete even if
    // your project has ESLint errors.
    ignoreDuringBuilds: true,
  },
  webpack: (config, { dev }) => {
    config.module.rules.push({
      test: /\.(spec|test)\.[t|j]s[x]*$/,
      loader: 'ignore-loader',
    });
    return config;
  },
  async rewrites() {
    return [
      {
        source: '/:locale/:match*',
        destination: '/:locale/subscribe/:match*',
        has: [
          {
            type: 'host',
            value: 'unad.me',
          },
        ],
        locale: false,
      },
      {
        source: '/:locale/:match*',
        destination: '/:locale/subscribe/:match*',
        has: [
          {
            type: 'host',
            value: 'es.unad.me',
          },
        ],
        locale: false,
      },
    ];
  },
  async redirects() {
    return [
      {
        source: '/',
        has: [
          {
            type: 'host',
            value: 'unad.me',
          },
        ],
        destination: 'https://unad.tech/',
        basePath: false,
        permanent: true,
      },
    ];
  },
};

module.exports = nextConfig;
