#!/bin/bash

# HOST_IP=$(getent hosts host.docker.internal | awk '{ print $1 }')
# Update the redis.conf file with the host IP
# Ensure this path is correct and matches where you intend to keep your config files
REDIS_CONF="/redis/redis.conf"

# Check if cluster-announce-ip already exists in the conf file
if grep -q "^cluster-announce-ip" $REDIS_CONF; then
    # Replace the existing IP with the new one
    sed -i "s/^cluster-announce-ip.*/cluster-announce-ip $HOST_IP/" $REDIS_CONF
else
    # Add the configuration if it doesn't exist
    echo "cluster-announce-ip $HOST_IP" >> $REDIS_CONF
fi

# Start Redis with the updated configuration
redis-server $REDIS_CONF
