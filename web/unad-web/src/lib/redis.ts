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
  if (process.env.REDIS_CLUSTER_NODES === undefined) {
    const client = createClient({
      url: process.env.REDIS_URL,
    });

    return new ModelFactory(client as RedisClientType);
  }
  const rootNodes = process.env.REDIS_CLUSTER_NODES!.split(',').map((n) => ({
    url: `redis${process.env.REDIS_USE_TLS === 'true' ? 's' : ''}://${n}`,
  }));
  // console.log('rootNodes', rootNodes);
  const cluster = createCluster({
    rootNodes,
    useReplicas: true, // TODO: Look into whether this make sense for our use case
    defaults: {
      socket: {
        tls: process.env.REDIS_USE_TLS === 'true' ? true : undefined,
      },
      username: process.env.REDIS_USER,
      password: process.env.REDIS_KEY,
    },
  });

  return new ModelFactory(cluster as unknown as RedisClientType);
}

/**
 * Represents the interface for a model factory.
 * This interface provides methods for creating clients, subscribers, sessions, and managing OTP secrets.
 */
export interface ModelFactoryInterface extends Disposable {
  createSession(jwt: string): Promise<string>;
  getSession(token: string): Promise<string | null>;
  setOtpSecret(phone: string, secret: string, countdown: number): Promise<void>;
  getOtpSecret(phone: string): Promise<string | null>;
  deleteOtpSecret(phone: string): Promise<void>;
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
  public static readonly keys = {
    otpSecret(phone: string) {
      return `otp:${phone}:secret`;
    },
    otpSecretTimeout(phone: string) {
      return `otp:${phone}:timeout`;
    },
    sessionToken(token: string) {
      return `session:${token}`;
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
    if (this.transaction) {
      this.transaction.discard();
      this.transaction = undefined;
    }
  }
  /**
   * open the connection to redis
   */
  public async connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.redisClient.on('ready', () => {
        if (typeof this.redisClient.ping === 'function') {
          resolve();
        }
      });

      this.redisClient.on('error', (error) => {
        reject(error);
      });
      if (typeof this.redisClient.ping === 'function') {
        this.redisClient.connect().then(() => {
          resolve();
        });
      } else {
        this.redisClient.connect();
      }
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
  public async setOtpSecret(
    phone: string,
    secret: string,
    countdown: number
  ): Promise<void> {
    const timeout = await this.redisClient.ttl(
      ModelFactory.keys.otpSecretTimeout(phone)
    );
    if (timeout > 0) {
      throw new Error(
        `Please wait ${timeout} seconds before requesting a new OTP`
      );
    }
    await this.redisClient.set(ModelFactory.keys.otpSecret(phone), secret, {
      EX: 60 * 5,
    });
    await this.redisClient.set(
      ModelFactory.keys.otpSecretTimeout(phone),
      secret,
      {
        EX: countdown,
      }
    );
  }

  public async getOtpSecret(phone: string): Promise<string | null> {
    return await this.redisClient.get(ModelFactory.keys.otpSecret(phone));
  }

  public async deleteOtpSecret(phone: string): Promise<void> {
    await this.redisClient.del(ModelFactory.keys.otpSecret(phone));
  }

  [Symbol.dispose]() {
    if (!this.isDisposed) {
      this.disconnect();
      this.isDisposed = true;
    }
  }
}

export { type ModelFactory };
