#!/bin/bash
if [ -z "$SLACK_API_BASE_URL" ]
then
      echo "Warning: \$SLACK_API_BASE_URL is empty"
fi
if [ -z "$SLACK_API_AUTH_TOKEN" ]
then
      echo "Warning: \$SLACK_API_AUTH_TOKEN is empty"
fi



HARALD_DATABASE_CONNECTIONSTRING="User ID=postgres;Password=p;Host=localhost;Port=5432;Database=harald_db;" \
HARALD_KAFKA_BOOTSTRAP_SERVERS=localhost:9092 \
HARALD_KAFKA_GROUP_ID=harald-consumer \
HARALD_KAFKA_ENABLE_AUTO_COMMIT=false \
HARALD_START_METRIC_SERVER=false \
SLACK_API_BOT_USER_ID="This-is-not-an-user-id" \
SLACK_DEFAULT_NOTIFICATION_CHANNEL_ID="this-is-not-a-channel-id" \
dotnet watch --project ./../src/Harald.WebApi/Harald.WebApi.csproj \
run --server.urls "http://*:5123" --no-launch-profile