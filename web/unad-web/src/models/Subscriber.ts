import type { RedisClientType,RedisTransactionType } from '@/lib/redis';

export class Subscriber {
  public static keys = {
    subscribersSet() {
      return 'subscribers';
    },
    subscriberHash(phone: string) {
      return `subscriber:${phone}`;
    },
    subscriberClientsSet(phone: string) {
      return `subscriber:${phone}:clients`;
    },
  };

  constructor(
    public readonly phone: string,
    private readonly redisClient: RedisClientType,
    private readonly transaction?: RedisTransactionType
  ) {}

  public save() {
    if (!this.transaction) {
      throw new Error('Cannot save subscriber without transaction');
    }
    this.transaction.sAdd(Subscriber.keys.subscribersSet(), this.phone);
  }

  public async delete() {
    if (!this.transaction) {
      throw new Error('Cannot delete subscriber without transaction');
    }
    this.transaction.sRem(Subscriber.keys.subscribersSet(), this.phone);
  }

  public subscribeToClient(id: string) {
    if (!this.transaction) {
      throw new Error('Cannot subscribe to client without transaction');
    }
    this.transaction.sAdd(Subscriber.keys.subscriberClientsSet(this.phone), id);
  }

  public setLocale(locale: string) {
    if (!this.transaction) {
      throw new Error('Cannot set locale without transaction');
    }
    this.transaction.hSet(
      Subscriber.keys.subscriberHash(this.phone),
      'locale',
      locale
    );
  }

  public async getLocale(): Promise<string | undefined> {
    return this.redisClient.hGet(
      Subscriber.keys.subscriberHash(this.phone),
      'locale'
    );
  }

  public unsubscribe(id: string) {
    if (!this.transaction) {
      throw new Error('Cannot unsubscribe without transaction');
    }
    this.transaction.sRem(Subscriber.keys.subscriberClientsSet(this.phone), id);
  }
}
