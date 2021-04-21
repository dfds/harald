#Setup build image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

#Set container filesystem to /build (and create folder if it doesnt exist)
WORKDIR /build

#Copy files to container file system.
COPY ./src/Harald.WebApi ./src/Harald.WebApi
COPY ./src/Harald.Infrastructure.Slack ./src/Harald.Infrastructure.Slack

#Set workdir to current project folder
WORKDIR /build/src/Harald.WebApi

#Restore csproj packages.
RUN dotnet restore

#Compile source code using standard Release profile
RUN dotnet publish -c Release -o /build/out

#Setup final container images.
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app

# SSL
RUN curl -o /tmp/rds-combined-ca-bundle.pem https://s3.amazonaws.com/rds-downloads/rds-combined-ca-bundle.pem \
    && mv /tmp/rds-combined-ca-bundle.pem /usr/local/share/ca-certificates/rds-combined-ca-bundle.crt \
    && update-ca-certificates

# Datadog APM
ENV TRACER_VERSION=1.13.0
RUN curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v${TRACER_VERSION}/datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb \
    && dpkg -i ./datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb

#Env settings
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_INTEGRATIONS=/opt/datadog/integrations.json
ENV DD_DOTNET_TRACER_HOME=/opt/datadog
ENV KAFKA_TO_SIGNALR_RELAY_START_KAFKA_CONSUMER=false
ENV ASPNETCORE_URLS="http://*:50900"

#Copy binaries from publish container to final container
COPY --from=build-env /build/out .

# OpenSSL cert for Kafka
RUN curl -sS -o /app/cert.pem https://curl.se/ca/cacert.pem
ENV HARALD_KAFKA_SSL_CA_LOCATION=/app/cert.pem

#Run dotnet executable
ENTRYPOINT ["dotnet", "Harald.WebApi.dll"]
