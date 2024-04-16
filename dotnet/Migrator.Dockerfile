FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ./UnAd.Data.Migrator.sln ./UnAd.Data.Migrator.sln
COPY ./tools/UnAd.Data.Migrator/ ./tools/UnAd.Data.Migrator/
COPY ./UnAd.Data/ ./UnAd.Data/

RUN dotnet restore

COPY .config/ ./.config

RUN dotnet tool restore

RUN dotnet ef migrations bundle \
    --project ./UnAd.Data/UnAd.Data.csproj \
    --startup-project ./tools/UnAd.Data.Migrator/UnAd.Data.Migrator.csproj \
    --self-contained -r linux-x64

RUN chmod +x ./efbundle

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS exec

ARG ConnectionStrings__UserDb
ENV ConnectionStrings__UserDb=$ConnectionStrings__UserDb

WORKDIR /app
COPY --from=build /src/efbundle efbundle
ENTRYPOINT /app/efbundle

# TODO: figure out an elegant way to build this for each context
