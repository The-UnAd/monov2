import {
  Column,
  CreateDateColumn,
  Entity,
  PrimaryColumn,
  PrimaryGeneratedColumn,
} from 'typeorm';

@Entity({ name: 'client' })
export class Client {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column({ type: 'varchar', nullable: false })
  name: string;

  @Column({ type: 'varchar', length: 15, unique: true, name: 'phone_number' })
  phoneNumber: string;

  @Column({ type: 'varchar', nullable: true, name: 'customer_id' })
  customerId: string | null;

  @Column({ type: 'varchar', nullable: true, name: 'subscription_id' })
  subscriptionId: string | null;

  @Column({ type: 'varchar', length: 5, nullable: false })
  locale: string | null;
}

@Entity({ name: 'client_subscriber' })
export class ClientSubscriber {
  @PrimaryColumn({ name: 'client_id' })
  clientId: string;

  @PrimaryColumn({ name: 'subscriber_phone_number' })
  subscriberPhoneNumber: string;
}

@Entity({ name: 'subscriber' })
export class Subscriber {
  @PrimaryColumn({ type: 'varchar', length: 15, name: 'phone_number' })
  phoneNumber: string;

  @CreateDateColumn({
    type: 'timestamptz',
    default: () => 'CURRENT_TIMESTAMP',
    name: 'joined_date',
  })
  joinedDate: Date;

  @Column({ type: 'varchar', length: 5, nullable: false })
  locale: string | null;
}

@Entity({ name: 'announcement' })
export class Announcement {
  @PrimaryColumn({ type: 'char', length: 34, name: 'message_sid' })
  messageSid: string;

  @CreateDateColumn({
    type: 'timestamptz',
    default: () => 'CURRENT_TIMESTAMP',
    name: 'sent_pn',
  })
  sentOn: Date;

  @Column({ type: 'varchar', length: 5, nullable: false })
  locale: string | null;
}
