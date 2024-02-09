import { Column, Entity, PrimaryColumn } from 'typeorm';

@Entity({ name: 'product' })
export class Product {
  @PrimaryColumn({ type: 'varchar', name: 'product_id' })
  productId: string;

  @Column({ type: 'text' })
  description: string;
}
