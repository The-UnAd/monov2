# wait-for-postgres.ps1

function Wait-For-Postgres {
    param(
        [string]$dbHost,
        [string]$dbUser
    )

    while ((pg_isready -h $dbHost -p 5432 -U $dbUser) -ne "accepting connections") {
        Start-Sleep -s 1
    }
}

Write-Host "Waiting for PostgreSQL to start..."
Wait-For-Postgres -dbHost $env:DB_HOST -dbUser $env:POSTGRES_USER

Write-Host "PostgreSQL started. Attempting to drop the '$env:POSTGRES_USER' database..."

psql -h $env:DB_HOST -p 5432 -U $env:POSTGRES_USER -d postgres -c "DROP DATABASE IF EXISTS $env:POSTGRES_USER;"

Write-Host "'unad' database dropped."
