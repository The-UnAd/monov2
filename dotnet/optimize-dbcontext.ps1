dotnet ef dbcontext optimize `
    --project ./UnAd.Data/UnAd.Data.csproj `
    --startup-project ./tools/UnAd.Data.Migrator/UnAd.Data.Migrator.csproj `
    --output-dir UnAd.Models `
    --namespace UnAd.Data
