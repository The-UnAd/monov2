dotnet ef dbcontext scaffold `
    Name=ConnectionStrings:UserDb `
    --context UserDbContext `
    --namespace UnAd.Data.Users.Models `
    --output-dir ..\UnAd.Data\Users\Models `
    --context-dir ..\UnAd.Data\Users `
    --context-namespace UnAd.Data.Users `
    --project ..\UnAd.Data\UnAd.Data.csproj `
    --startup-project .\UnAd.Data.Migrator\UnAd.Data.Migrator.csproj `
    --no-onconfiguring `
    --force `
    --verbose `
    Npgsql.EntityFrameworkCore.PostgreSQL
