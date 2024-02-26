#!/bin/bash
set -e

dotnet fusion subgraph config set name "UserApi" -w "../../UserApi" -c "../../UserApi/subgraph-config.json"
dotnet fusion subgraph config set http --url "http://user-api:5300/graphql" -w ./UserApi -c ../../UserApi/subgraph-config.json
dotnet run --project ../../UserApi/UserApi.csproj -- schema export --output schema.graphql
dotnet fusion subgraph pack -w ../../UserApi
dotnet fusion compose -p gateway -s ../../UserApi
dotnet run -- schema export --output schema.graphql
