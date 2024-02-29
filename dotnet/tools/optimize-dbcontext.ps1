dotnet ef dbcontext optimize `
    --project ..\UnAd.Data\UnAd.Data.csproj `
    --startup-project .\UnAd.Data.Migrator\UnAd.Data.Migrator.csproj `
    --output-dir Models `
    --namespace UnAd.Data.Users
