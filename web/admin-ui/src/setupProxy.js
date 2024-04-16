const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function (app) {
  app.use(
    '/auth',
    createProxyMiddleware({
      target: 'http://localhost:5555/login',
      changeOrigin: true,
      pathRewrite: { '^/auth': '' },
    })
  );
  app.use(
    '/graphql',
    createProxyMiddleware({
      target: 'http://localhost:5900/graphql',
      changeOrigin: true,
    })
  );
};
