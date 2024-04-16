dotnet ef database update `
    --context $args[0] `
    --project ../UnAd.Data/UnAd.Data.csproj `
    --startup-project .\UnAd.Data.Migrator\UnAd.Data.Migrator.csproj `
    --verbose
