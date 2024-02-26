param(
    [Parameter(Mandatory=$false)]
    [string]$environment = "Development"
)

type ".\appsettings.$environment.json" | dotnet user-secrets set
