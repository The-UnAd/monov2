$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

$envFile = Join-Path $scriptDir ".env.default"
$vaultFile = Join-Path $scriptDir ".env.vault"
$envMeFile = Join-Path $scriptDir ".env.me"
  
if (-not (Test-Path $vaultFile)) {
    Write-Host "The .env.vault file doesn't exist."
    exit 1
}
  
if (-not (Test-Path $envMeFile)) {
    Write-Host "The .env.me file doesn't exist. Run 'npx dotenv-vault login'."
    exit 1
}

if (-not (Test-Path $envFile)) {
    Write-Host "The .env file at '$envFile' doesn't exist. Run 'npx dotenv-vault pull'."
    exit 1
}

# Read the .env file line by line
$envLines = Get-Content $envFile
foreach ($line in $envLines) {
    # Ignore lines starting with '#' (comments) and empty lines
    if ($line -match "^\s*#") {
        continue
    }
    if (-not $line.Trim()) {
        continue
    }

    # Split the line into key and value
    $key, $value = $line -split '=', 2

    $key = $key.Trim()
    $value = $value.Trim() -replace '"', ''
  
    # Set the environment variable
    [Environment]::SetEnvironmentVariable($key, $value.Trim())
}

# Print the environment variables (optional)
Write-Host "Environment variables set:"
Get-ChildItem -Path "Env:" | Sort-Object Name | ForEach-Object {
    Write-Host "$($_.Name) = $($_.Value)"
}



