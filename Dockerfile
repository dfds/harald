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
RUN curl -o /tmp/rds-combined-ca-bundle.pem https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem \
    && mv /tmp/rds-combined-ca-bundle.pem /usr/local/share/ca-certificates/rds-combined-ca-bundle.crt \
    && update-ca-certificates


#Env settings
ENV KAFKA_TO_SIGNALR_RELAY_START_KAFKA_CONSUMER=false
ENV ASPNETCORE_URLS="http://*:50900"

#Copy binaries from publish container to final container
COPY --from=build-env /build/out .

# OpenSSL cert for Kafka
RUN curl -sS -o /app/cert.pem https://curl.se/ca/cacert.pem
ENV HARALD_KAFKA_SSL_CA_LOCATION=/app/cert.pem

#Non-root user settings
ARG USERNAME=harald
ARG USER_UID=1000
ARG USER_GID=$USER_UID

RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME 


USER $USERNAME

#Run dotnet executable
ENTRYPOINT ["dotnet", "Harald.WebApi.dll"]
