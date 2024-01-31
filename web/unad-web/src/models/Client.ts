import { randomUUID } from 'node:crypto';

import { RedisClientType, type RedisTransactionType } from '@/lib/redis';

import { createSha1Hash } from '../lib/crypto';

interface ClientHashData {
  id: string;
  name: string;
  phone: string;
  clientId: string;
  customerId?: string;
  subscriptionId?: string;
  locale?: string;
}
export type ClientHashKey = keyof ClientHashData;
export type ClientData = ClientHashData & {
  [x: string]: string;
};

export class Client {
  public static hashId(name: string): string {
    return createSha1Hash(name).substring(0, 8);
  }

  public static keys = {
    clientsSet() {
      return 'clients';
    },
    clientHash(phone: string) {
      return `client:${phone}`;
    },
    clientIdToPhone(id: string) {
      return `client:${id}`;
    },
    clientHashToId(clientId: string) {
      return `client:${clientId}`;
    },
    clientSubscriberSet(id: string) {
      return `client:${id}:subscribers`;
    },
    clientAnnouncementsSortedSet(phone: string) {
      return `client:${phone}:announcements`;
    },
    subscriptionClientPhone(subscriptionId: string) {
      return `subscription:${subscriptionId}`;
    },
  };

  public readonly id: string | null | undefined = null;
  public readonly clientId: string | null | undefined = null;

  constructor(
    public readonly name: string,
    public readonly phone: string,
    private readonly redisClient: RedisClientType,
    private readonly transaction?: RedisTransactionType
  ) {
    this.transaction = transaction;
    this.id = randomUUID();
    this.clientId = Client.hashId(this.id);
  }

  public save() {
    if (!this.transaction) {
      throw new Error('Cannot save client without transaction');
    }
    // Save client object in Redis
    this.transaction.hSet(Client.keys.clientHash(this.phone), {
      name: this.name,
      phone: this.phone,
      clientId: this.clientId as string,
      id: this.id as string,
    });
    this.transaction.set(
      Client.keys.clientHashToId(this.clientId as string),
      this.id as string
    );
    this.transaction.set(
      Client.keys.clientIdToPhone(this.id as string),
      this.phone
    );
    this.transaction.sAdd(Client.keys.clientsSet(), this.id as string);
  }

  public setLocale(locale: string) {
    if (!this.transaction) {
      throw new Error('Cannot set locale without transaction');
    }
    this.transaction.hSet(Client.keys.clientHash(this.phone), 'locale', locale);
  }

  public async getLocale(): Promise<string | undefined> {
    return await this.redisClient.hGet(
      Client.keys.clientHash(this.phone),
      'locale'
    );
  }

  public async getStripeCustomerId(): Promise<string | undefined> {
    return await this.redisClient.hGet(
      Client.keys.clientHash(this.phone),
      'customerId'
    );
  }

  public async getStripeSubscriptionId(): Promise<string | undefined> {
    return await this.redisClient.hGet(
      Client.keys.clientHash(this.phone),
      'subscriptionId'
    );
  }

  public addSubscriber(phone: string) {
    if (!this.transaction) {
      throw new Error('Cannot add subscriber without transaction');
    }
    this.transaction.sAdd(
      Client.keys.clientSubscriberSet(this.id as string),
      phone
    );
  }

  public removeSubscriber(phone: string) {
    if (!this.transaction) {
      throw new Error('Cannot remove subscriber without transaction');
    }
    this.transaction.sRem(
      Client.keys.clientSubscriberSet(this.id as string),
      phone
    );
  }

  public async getSubscriberCount(): Promise<number> {
    return await this.redisClient.sCard(
      Client.keys.clientSubscriberSet(this.id as string)
    );
  }

  public async getAnnouncementCount(): Promise<number> {
    return await this.redisClient.zCard(
      Client.keys.clientAnnouncementsSortedSet(this.phone)
    );
  }

  // TODO: what do I even want to do when "deleting" a client?
  public async deleteAllKeys() {
    if (!this.transaction) {
      throw new Error('Cannot delete client without transaction');
    }
    // const subscriptionId = await this.redisClient.hGet(Client.keys.clientHash(this.phone), 'subscriptionId');
    this.transaction
      .sRem(Client.keys.clientsSet(), this.id as string)
      .del(Client.keys.clientIdToPhone(this.id as string))
      .del(Client.keys.clientHash(this.phone))
      .del(Client.keys.clientSubscriberSet(this.id as string))
      .del(Client.keys.clientAnnouncementsSortedSet(this.phone));
    // .del(Client.keys.subscriptionClientPhone(subscriptionId)) // TODO: delete subscription
  }
}
