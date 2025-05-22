# First stage: restore
FROM docker.xuanyuan.me/fallenwood/garnet:20250522.1 AS base
WORKDIR /pigeonhorde

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN cd src/PigeonHorde && dotnet publish -c Release -o /pigeonhorde && rm -f /pigeonhorde/appsettings.Development.json
COPY docker-entrypoint.sh /pigeonhorde/docker-entrypoint.sh
RUN chmod +x /pigeonhorde/docker-entrypoint.sh

FROM base AS final
WORKDIR /pigeonhorde
COPY --from=build /pigeonhorde .
ENTRYPOINT ["/pigeonhorde/docker-entrypoint.sh"]