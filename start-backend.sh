#! /bin/bash

docker-compose --file docker-compose.backend.yml up -d --wait --timeout 3000 
