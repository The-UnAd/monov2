import { PrismaClient as ProductDbClient } from '@unad/product-models';
import { PrismaClient as UserDbClient } from '@unad/user-models';

export const UserDb = new UserDbClient({
  datasourceUrl: process.env.USER_DATABASE_URL,
});
export const ProductDb = new ProductDbClient({
  datasourceUrl: process.env.PRODUCT_DATABASE_URL,
});
