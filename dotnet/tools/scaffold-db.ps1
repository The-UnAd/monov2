dotnet ef dbcontext scaffold `
    Name=ConnectionStrings:UserDb `
    --context UserDbContext `
    --namespace UnAd.Data.Users `
    --output-dir ..\UnAd.Data\Users `
    --project ..\UnAd.Data\UnAd.Data.csproj `
    --startup-project .\UserApi.csproj `
    --no-onconfiguring `
    --force `
    --verbose `
    Npgsql.EntityFrameworkCore.PostgreSQL
