import { createClient } from 'redis';

function redisErrorHandler(err: Error) {
  console.error('Redis Client', err);
}

export function createRedisClient() {
  const client = createClient({
    url: process.env.REDIS_URL,
  });
  client.on('error', redisErrorHandler);

  return client;
}
