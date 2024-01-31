#!/bin/bash

curl -X POST -H @./dotnet/GraphMonitor/headers -d 'http://user-api:5300/graphql' http://graph-monitor:5145/graph/UserApi
docker-compose --file docker-compose.yml up -d



