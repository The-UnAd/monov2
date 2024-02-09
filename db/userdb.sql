CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE client (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR NOT NULL,
    phone_number VARCHAR(15) NOT NULL UNIQUE CHECK (phone_number ~ '^\+\d{1,14}$'),
    customer_id VARCHAR NULL,
    subscription_id VARCHAR NULL,
    locale VARCHAR(5) NOT NULL DEFAULT 'en-US'
);

CREATE INDEX idx_client_phone_number ON client(phone_number);

CREATE TABLE subscriber (
    phone_number VARCHAR(15) PRIMARY KEY CHECK (phone_number ~ '^\+\d{1,14}$'),
    joined_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE client_subscriber (
    client_id UUID,
    subscriber_phone_number VARCHAR(15),
    PRIMARY KEY (client_id, subscriber_phone_number),
    FOREIGN KEY (client_id) REFERENCES client (id) ON DELETE CASCADE,
    FOREIGN KEY (subscriber_phone_number) REFERENCES subscriber (phone_number) ON DELETE CASCADE
);

CREATE TABLE product (
    product_id VARCHAR PRIMARY KEY,
    description TEXT
);
