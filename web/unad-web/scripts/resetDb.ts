import { createRedisClient } from './util';
require('dotenv').config({
  path: require('path').resolve(__dirname, '../.env.test.cy'),
});

(async function () {
  const redis = createRedisClient();
  try {
    await redis.connect();
    await redis.flushAll();
    await redis.disconnect();
    console.log('Redis flushed');
    return process.exit(0);
  } catch (e: any) {
    await redis.disconnect();
    console.error(e.message);
    process.exit(1);
  }
})();
