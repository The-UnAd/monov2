# Get the private IP address
$privateIP = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -ne "Loopback Pseudo-Interface 1" -and $_.IPAddress -notlike "169.*" } | Select-Object -First 1).IPAddress

$env:HOST_IP = $privateIP
Write-Host "HOST_IP: $env:HOST_IP"
docker-compose --file docker-compose.redis.yml up -d --wait
