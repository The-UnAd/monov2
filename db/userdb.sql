CREATE TABLE IF NOT EXISTS "role" (
  "id" varchar(50) NOT NULL,
  PRIMARY KEY ("id")
);

INSERT INTO "role" (id) VALUES ('patient');
INSERT INTO "role" (id) VALUES ('provider');
INSERT INTO "role" (id) VALUES ('labtech');


CREATE TABLE IF NOT EXISTS "user" (
  "id" varchar(100) NOT NULL,
  "name" varchar(100) NOT NULL,
  "email" varchar(100) NOT NULL,
  "role_id" varchar(50) NOT NULL,
  
  PRIMARY KEY ("id"),
  CONSTRAINT fk_role FOREIGN KEY(role_id)  REFERENCES role(id)
);

INSERT INTO "user" (id, name, email, role_id) VALUES (:TEST_USER_ID, 'Test User', :TEST_USER_USERNAME, 'patient');


















