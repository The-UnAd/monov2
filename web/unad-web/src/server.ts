import { createServer } from 'http';
import next from 'next';
import { parse } from 'url';

import { prisma } from './lib/db';

const dev = process.env.NODE_ENV !== 'production';
const hostname = process.env.HOST || 'localhost';
const port = parseInt(process.env.PORT as string);
// when using middleware `hostname` and `port` must be provided below
const app = next({ dev, hostname, port });
const handle = app.getRequestHandler();

process.on('SIGINT', () => {
  console.log('Caught interrupt signal.  Exiting. 😵');
  prisma.$disconnect().then(() => {
    console.log('📉 Database disconnected!');
    process.exit();
  });
});

process.on('SIGUSR2', () => {
  console.log('Caught restart signal. Cleaning up and restarting. 🔄');
  prisma.$disconnect().then(() => {
    console.log('📉 Database disconnected!');
    process.exit();
  });
});

console.log('Preparing server...');
app.prepare().then(() => {
  console.log('Starting server...');
  prisma.$connect().then(() => {
    console.log('📈 Database connected!');
    createServer(async (req, res) => {
      try {
        // Be sure to pass `true` as the second argument to `url.parse`.
        // This tells it to parse the query portion of the URL.
        const parsedUrl = parse(req.url as string, true);
        await handle(req, res, parsedUrl);
      } catch (err) {
        console.error('Error occurred handling', req.url, err);
        res.statusCode = 500;
        res.end('internal server error');
      }
    })
      .once('error', (err) => {
        console.error(err);
        process.exit(1);
      })
      .listen(port, () => {
        console.log(`🚀 Server listening on http://${hostname}:${port}`);
      })
      .on('error', (err) => {
        console.error(err);
        prisma.$disconnect().then(() => {
          console.log('📉 Database disconnected!');
          process.exit(1);
        });
      });
  });
});
