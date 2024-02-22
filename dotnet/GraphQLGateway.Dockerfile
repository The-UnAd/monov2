FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG GRAPH_MONITOR_URL

WORKDIR /src

COPY GraphQLGateway.Docker.sln ./
COPY UnAd.Data/*.csproj ./UnAd.Data/
COPY UnAd.Auth.Web/*.csproj ./UnAd.Auth.Web/
COPY UserApi/*.csproj ./UserApi/
COPY GraphQLGateway/GraphQLGateway/*.csproj ./GraphQLGateway/GraphQLGateway/

RUN dotnet restore
COPY . .

WORKDIR /src/UserApi
RUN dotnet restore

WORKDIR /src
RUN dotnet tool restore
############################################################################################################
# TODO: So this is what we need:
#       1. When a subgraph is successfully deployed, we need to communicate it's graphql endpoint to
#          a centralized location. This will allow us to generate the subgraph config for that project.
#       2. When we come here to build the gateway, we need to pull in that subgraph config and use it
#          to generate the gateway schema.
############################################################################################################

RUN dotnet fusion subgraph config set name "UserApi" -w ./UserApi -c ./UserApi/subgraph-config.json
RUN --mount=type=secret,id=graphmonitorheaders,target=/run/secrets/headers \
    URL=$(curl -sb -f -H @/run/secrets/headers ${GRAPH_MONITOR_URL}/user-api) && \
    dotnet fusion subgraph config set http --url "$URL" -w ./UserApi -c ./UserApi/subgraph-config.json

RUN dotnet run --project UserApi/UserApi.csproj -- schema export --output schema.graphql
RUN dotnet fusion subgraph pack -w ./UserApi
RUN dotnet fusion compose -p ./GraphQLGateway/GraphQLGateway/gateway -s ./UserApi

FROM build AS publish
WORKDIR /src/GraphQLGateway/GraphQLGateway/
RUN dotnet publish "GraphQLGateway.csproj" -c Release -o /src/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /src/publish .
ENTRYPOINT ["dotnet", "GraphQLGateway.dll"]



