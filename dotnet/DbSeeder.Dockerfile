FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG GRAPH_MONITOR_URL

WORKDIR /src
COPY DbSeeder.sln ./
COPY UnAd.Redis/*.csproj ./UnAd.Redis/
COPY UnAd.Data/*.csproj ./UnAd.Data/
COPY DbSeeder/*.csproj ./DbSeeder/

RUN dotnet restore
COPY . .
WORKDIR /src/DbSeeder
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish DbSeeder.csproj -c Release -o /src/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /src/publish .
ENTRYPOINT /app/DbSeeder



