FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /src

COPY ./UnAd.Data.Migrator.sln ./UnAd.Data.Migrator.sln
COPY ./tools/UnAd.Data.Migrator/ ./tools/UnAd.Data.Migrator/
COPY ./UnAd.Data/ ./UnAd.Data/

RUN dotnet restore

COPY ./tools/.config/ ./.config

RUN dotnet tool restore

RUN dotnet ef migrations bundle \
    --project ./UnAd.Data/UnAd.Data.csproj \
    --startup-project ./tools/UnAd.Data.Migrator/UnAd.Data.Migrator.csproj \
    --self-contained -r linux-x64

RUN chmod +x ./efbundle

FROM build as exec
WORKDIR /app
COPY --from=build /src/efbundle efbundle
# NOTE: make sure to set the environment variable DB_CONNECTIONSTRING to the connection string for the database
ENTRYPOINT /app/efbundle --connection $DB_CONNECTIONSTRING



