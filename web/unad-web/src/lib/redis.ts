import {
  createClient,
  createCluster,
  type RedisClientType as _RedisClientType,
  RedisDefaultModules,
  RedisFunctions,
  RedisScripts,
} from 'redis';
import { ulid } from 'ulid';
import { v4 as uuidv4 } from 'uuid';

import { Client, type ClientData } from '../models/Client';
import { Subscriber } from '../models/Subscriber';
import { ITransactionContext } from './transactions';

export type RedisClientType = ReturnType<
  typeof createClient<RedisDefaultModules, RedisFunctions, RedisScripts>
>;

export type RedisTransactionType = ReturnType<_RedisClientType['multi']>;

/**
 * Creates a model factory for Redis.
 * @returns A new instance of ModelFactory.
 */
export function createModelFactory() {
  const client =
    process.env.REDIS_CLUSTER_NODES === undefined
      ? createClient({
          url: `redis://${process.env.REDIS_HOST}:${process.env.REDIS_PORT}`,
        })
      : createCluster({
          rootNodes: process.env.REDIS_CLUSTER_NODES!.split(',').map((n) => ({
            url: `rediss://${n.split(':')[0]}:${n.split(':')[1]}`,
          })),
          useReplicas: false,
          defaults: {
            socket: {
              tls: true,
            },
            username: process.env.REDIS_USER,
            password: process.env.REDIS_KEY,
          },
        });
  return new ModelFactory(client as RedisClientType);
}

/**
 * Represents the interface for a model factory.
 * This interface provides methods for creating clients, subscribers, sessions, and managing OTP secrets.
 */
export interface ModelFactoryInterface extends Disposable {
  createClient(name: string, phone: string): Client;
  createSubscriber(phone: string): Subscriber;
  getSubscriberByPhone(phone: string): Subscriber;
  createSession(jwt: string): Promise<string>;
  getSession(token: string): Promise<string | null>;
  setOtpSecret(phone: string, secret: string): Promise<void>;
  getOtpSecret(phone: string): Promise<string | null>;
  deleteOtpSecret(phone: string): Promise<void>;
  getClientByPhone(phoneNumber: string): Promise<Client | null>;
  getClientById(id: string): Promise<Client | null>;
  getClientByClientId(clientId: string): Promise<Client | null>;
  healthCheck(): Promise<boolean>;
}

export type TransactionalModelFactoryInterface = ModelFactoryInterface &
  ITransactionContext;

/*
  TODO: it's currently a little weird to use this class
        when you want to use transactions.
        You have to call .beginTransaction() before you create the model,
        which is not ideal.

        Maybe it would be better to have a ModelFactory class that
        doesn't implement ITransactionContext,
        and a TransactionalModelFactory class that does,
        and then you can use whichever one you need.

        Or maybe keep track of the models that have been created
        and then when you call .beginTransaction() on the factory,
        it sets the transaction on all the models that have been created.
*/

/**
 * Represents a ModelFactory that provides methods for interacting with Redis and creating model instances.
 */
class ModelFactory implements TransactionalModelFactoryInterface {
  public static keys = {
    otpSecret(phone: string) {
      return `otp:${phone}:secret`;
    },
    sessionToken(token: string) {
      return `session:${token}`;
    },
    announcementSid(guid: string) {
      return `link:${guid}`;
    },
    confirmationCode() {
      return `confirmations`;
    },
  };

  private isDisposed = false;
  private transaction?: RedisTransactionType;

  constructor(private readonly redisClient: RedisClientType) {}

  public healthCheck(): Promise<boolean> {
    return new Promise((resolve, reject) => {
      if (typeof this.redisClient.ping === 'function') {
        this.redisClient.ping().then((res) => {
          if (res === 'PONG') {
            resolve(true);
          } else {
            reject(new Error('Failed to ping redis'));
          }
        });
      } else {
        resolve(true); // TODO: how do I verify the health of a connection to a cluster?
      }
    });
  }

  public beginTransaction(): void {
    this.transaction = this.redisClient.multi();
  }
  public async commitTransaction(): Promise<void> {
    if (!this.transaction) {
      throw new Error('No transaction in progress');
    }
    await this.transaction.exec();
  }
  public rollbackTransaction(): void {
    if (!this.transaction) {
      throw new Error('No transaction in progress');
    }
    this.transaction.discard();
    this.transaction = undefined;
  }
  /**
   * open the connection to redis
   */
  public async connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.redisClient.on('ready', () => {
        resolve();
      });

      this.redisClient.on('error', (error) => {
        reject(error);
      });
      this.redisClient.connect();
    });
  }

  /*

    TODO: figure out how to implement transactions
          maybe return an implementation of this class,
          but dealing with .multi() and .exec() instead of
          the redis client directly
          and maybe returning Proxies for the models that also deal in transactions

  */
  // private transaction?: any;
  // public beginTransaction(): void {
  //   // TODO: check if transaction is already in progress
  //   this.transaction = this.redisClient.multi();
  // }
  // public async commitTransaction(): Promise<void> {
  //   await this.transaction.exec();
  // }
  // public rollbackTransaction(): void {
  //   this.transaction.discard();
  //   this.transaction = undefined;
  // }

  /**
   * close the connection to redis
   */
  public disconnect(): void {
    if (this.redisClient) {
      this.transaction?.discard();
      this.redisClient.quit();
    }
  }

  /**
   * Create a new Client model instance
   * @param name Client name
   * @param phone Client phone number
   * @returns Client instance
   */
  public createClient(name: string, phone: string): Client {
    return new Client(name, phone, this.redisClient, this.transaction);
  }

  /**
   * Create a new Subscriber model instance
   * @param phone Subscriber phone number
   * @returns Subscriber instance
   */
  public createSubscriber(phone: string): Subscriber {
    return new Subscriber(phone, this.redisClient, this.transaction);
  }

  /**
   * Get a Subscriber model instance by phone number
   * @param phone Subscriber phone number
   * @returns Subscriber instance
   */
  public getSubscriberByPhone(phone: string): Subscriber {
    return new Subscriber(phone, this.redisClient as RedisClientType);
  }

  public async getTotalSubscribers(): Promise<number> {
    return await this.redisClient.sCard(Subscriber.keys.subscribersSet());
  }

  public async getAnnouncementSmsSidByGuid(
    guid: string
  ): Promise<string | undefined> {
    return await this.redisClient.hGet(
      ModelFactory.keys.announcementSid(guid),
      'sid'
    );
  }

  public async getTotalClients(): Promise<number> {
    return await this.redisClient.sCard(Client.keys.clientsSet());
  }

  /**
   * Create a new session and store it in Redis
   * @param jwt JSON Web Token
   * @returns Session token
   */
  public async createSession(jwt: string): Promise<string> {
    const token = uuidv4();
    await this.redisClient.set(ModelFactory.keys.sessionToken(token), jwt, {
      EX: Number(process.env.SESSION_LENGTH),
    });
    return token;
  }

  /**
   * TODO
   */
  public async createSubscriberConfirmation(
    subPhone: string,
    clientId: string
  ): Promise<string> {
    const code = ulid();
    await this.redisClient.hSet(
      ModelFactory.keys.confirmationCode(),
      code,
      `${subPhone}:${clientId}`
    );
    return code;
  }

  /**
   * TODO
   */
  public async getClientPhoneFromSubscriberConfirmation(
    code: string
  ): Promise<string> {
    const pair = await this.redisClient.hGet(
      ModelFactory.keys.confirmationCode(),
      code
    );
    if (!pair) {
      throw new Error('Invalid confirmation code');
    }
    const [, phone] = pair.split(':');
    return phone;
  }

  /**
   * Get a session by its token
   * @param token Session token
   * @returns Session JWT or null if not found
   */
  public async getSession(token: string): Promise<string | null> {
    return await this.redisClient.get(ModelFactory.keys.sessionToken(token));
  }

  /**
   * Store an OTP secret in Redis
   * @param phone Phone number associated with the OTP secret
   * @returns OTP secret
   */
  public async setOtpSecret(phone: string, secret: string): Promise<void> {
    await this.redisClient.set(ModelFactory.keys.otpSecret(phone), secret, {
      EX: 60 * 5,
    });
  }

  public async getOtpSecret(phone: string): Promise<string | null> {
    return await this.redisClient.get(ModelFactory.keys.otpSecret(phone));
  }

  public async deleteOtpSecret(phone: string): Promise<void> {
    await this.redisClient.del(ModelFactory.keys.otpSecret(phone));
  }

  public async getClientByPhone(phoneNumber: string): Promise<Client | null> {
    const data = await this.redisClient.hGetAll(
      Client.keys.clientHash(phoneNumber)
    );
    if (Object.keys(data).length === 0) {
      return null;
    }
    const { name } = data as ClientData;
    return new Client(name, phoneNumber, this.redisClient, this.transaction);
  }

  public async getClientById(id: string): Promise<Client | null> {
    const phoneNumber = await this.redisClient.get(
      Client.keys.clientIdToPhone(id)
    );
    if (phoneNumber === null) {
      return null;
    }
    const { name, phone } = await this.redisClient.hGetAll(
      Client.keys.clientHash(phoneNumber)
    );
    return new Client(
      name,
      phone,
      this.redisClient as RedisClientType,
      this.transaction
    );
  }

  public async getClientByClientId(clientId: string): Promise<Client | null> {
    const id = await this.redisClient.get(Client.keys.clientHashToId(clientId));
    if (id === null) {
      return null;
    }
    const phoneNumber = await this.redisClient.get(
      Client.keys.clientIdToPhone(id)
    );
    if (phoneNumber === null) {
      return null;
    }
    const { name, phone } = await this.redisClient.hGetAll(
      Client.keys.clientHash(phoneNumber)
    );
    return new Client(
      name,
      phone,
      this.redisClient as RedisClientType,
      this.transaction
    );
  }

  [Symbol.dispose]() {
    if (!this.isDisposed) {
      this.disconnect();
      this.isDisposed = true;
    }
  }
}

export { type ModelFactory };
