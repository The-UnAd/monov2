docker-compose --file .\docker-compose.backend.yml up -d redis
docker-compose --file .\docker-compose.backend.yml up -d postgres
# docker-compose --file .\docker-compose.backend.yml up -d graph-monitor
# docker-compose --file .\docker-compose.backend.yml up -d unad-functions
docker-compose --file .\docker-compose.backend.yml run --rm db-migrator
# docker-compose --file .\docker-compose.backend.yml run --rm db-seeder

