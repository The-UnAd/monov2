docker-compose --file .\docker-compose.web.yml up -d auth-api
docker-compose --file .\docker-compose.web.yml up -d user-api
docker-compose --file .\docker-compose.web.yml up -d graphql-gateway
docker-compose --file .\docker-compose.web.yml up -d admin-ui
