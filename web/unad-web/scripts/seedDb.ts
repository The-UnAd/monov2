import { createModelFactory } from '../src/lib/redis';
import { sanitizePhoneNumber } from '../src/lib/util';
require('dotenv').config({
  path: require('path').resolve(__dirname, '../.env.test.cy'),
});

(async function () {
  try {
    const models = createModelFactory();
    await models.connect();
    console.log('Creating clients...');
    const seed1 = models.createClient(
      'seed1',
      sanitizePhoneNumber(
        process.env.TEST_SMS_NUMBER?.substring(1) as string
      ).replace(/"/g, '')
    );
    const seed2 = models.createClient(
      'seed2',
      sanitizePhoneNumber(
        process.env.MAILOSAUR_SMS_NUMBER?.substring(1) as string
      ).replace(/"/g, '')
    );
    await seed1.save();
    await seed2.save();
    console.log('Redis seeded successfully');
    models.disconnect();
    process.exit(0);
  } catch (e: any) {
    console.error(e.message);
    process.exit(1);
  }
})();
