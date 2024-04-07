dotnet ef database update `
    --context UserDbContext `
    --project ../UnAd.Data/UnAd.Data.csproj `
    --startup-project .\UnAd.Data.Migrator\UnAd.Data.Migrator.csproj `
    --verbose
