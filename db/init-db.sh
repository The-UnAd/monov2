#!/bin/bash

set -e
set -u

function create_user_and_database() {
	local database=$1
	echo "  Creating user and database '$database'"
	psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
	    CREATE USER $database;
	    CREATE DATABASE $database;
	    GRANT ALL PRIVILEGES ON DATABASE $database TO $database;
EOSQL
}

if [ -n "$POSTGRES_MULTIPLE_DATABASES" ]; then
	echo "Multiple database creation requested: $POSTGRES_MULTIPLE_DATABASES"
	for db in $(echo $POSTGRES_MULTIPLE_DATABASES | tr ',' ' '); do
		create_user_and_database $db
	done
	echo "Multiple databases created"
fi

psql -U $POSTGRES_USER -d $POSTGRES_USER_DB -a -f /scripts/userdb.sql \
     -v TEST_USER_USERNAME="'$TEST_USER_USERNAME'" -v TEST_USER_ID="'$TEST_USER_ID'" -v COGNITO_POOL_ID="'$COGNITO_POOL_ID'"



