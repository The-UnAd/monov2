FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG GRAPH_MONITOR_URL

WORKDIR /src
COPY UserApi.sln ./
COPY UnAd.Data/*.csproj ./UnAd.Data/
COPY UnAd.Redis/*.csproj ./UnAd.Redis/
COPY UnAd.Auth.Web/*.csproj ./UnAd.Auth.Web/
COPY UserApi/*.csproj ./UserApi/
COPY UserApi/tests/UserApi.Tests.Integration/*.csproj ./UserApi/tests/UserApi.Tests.Integration/

RUN dotnet restore
COPY . .
WORKDIR /src/UserApi
RUN dotnet build -c Release -o /app

FROM build AS submit
RUN --mount=type=secret,id=graphmonitorheaders,target=/run/secrets/headers \
    curl -sSf -H @/run/secrets/headers -d "http://user-api:5300/graphql" ${GRAPH_MONITOR_URL}/user-api

FROM submit AS publish
RUN dotnet publish UserApi.csproj -c Release -o /src/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /src/publish .
ENTRYPOINT ["dotnet", "UserApi.dll"]



