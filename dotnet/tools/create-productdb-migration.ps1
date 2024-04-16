dotnet ef migrations add $args[0] `
    --context ProductDbContext `
    --output-dir ..\UnAd.Data\Products\Migrations `
    --project ..\UnAd.Data\UnAd.Data.csproj `
    --startup-project .\UnAd.Data.Migrator\UnAd.Data.Migrator.csproj `
    --verbose
