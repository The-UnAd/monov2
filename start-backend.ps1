docker-compose --file .\docker-compose.backend.yml up -d redis
docker-compose --file .\docker-compose.backend.yml up -d postgres
docker-compose --file .\docker-compose.backend.yml run --rm db_script
docker-compose --file .\docker-compose.backend.yml run --rm migrator

