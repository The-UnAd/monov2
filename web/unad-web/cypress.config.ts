import { defineConfig } from 'cypress';

export default defineConfig({
  projectId: 'v4155x',
  e2e: {
    baseUrl: 'http://localhost:3000',
  },
  component: {
    devServer: {
      framework: 'next',
      bundler: 'webpack',
    },
  },
  chromeWebSecurity: false,
});
