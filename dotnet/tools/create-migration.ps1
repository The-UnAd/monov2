dotnet ef migrations add $args[0] `
    --context UserDbContext `
    --output-dir ..\UnAd.Data\Users\Migrations `
    --project ..\UnAd.Data\UnAd.Data.csproj `
    --startup-project .\UnAd.Data.Migrator\UnAd.Data.Migrator.csproj `
    --verbose
