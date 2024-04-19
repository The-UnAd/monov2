$env:HOST_IP = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -ne '127.0.0.1' } | Select-Object -First 1).IPAddress

docker-compose --file .\docker-compose.backend.yml up -d redis
docker-compose --file .\docker-compose.backend.yml up -d postgres
# docker-compose --file .\docker-compose.backend.yml up -d graph-monitor
# docker-compose --file .\docker-compose.backend.yml up -d unad-functions
# docker-compose --file .\docker-compose.backend.yml run --rm db-migrator
# docker-compose --file .\docker-compose.backend.yml run --rm db-seeder

