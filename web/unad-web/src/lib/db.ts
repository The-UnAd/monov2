import * as Models from '@unad/models';
import { DataSource } from 'typeorm';

export const AppDataSource = new DataSource({
  type: 'postgres',
  host: process.env.DB_HOST,
  port: Number(process.env.DB_PORT),
  username: process.env.DB_USER,
  password: process.env.DB_PASS,
  database: process.env.DB_NAME,
  entities: Object.values(Models.Users),
  synchronize: false,
  logging: ['query', 'error'],
});

export async function getAppDataSource() {
  if (AppDataSource.manager.connection.isInitialized) {
    return AppDataSource;
  }
  return await AppDataSource.initialize();
}
