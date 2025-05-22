# First stage: restore
FROM fallenwood/garnet:20250522.1 AS base
WORKDIR /app
ENV LANG zh_CN.UTF-8

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN cd src/PigeonHorde && dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained true -o /out && rm /out/PigeonHorde/PigeonHorde.staticwebassets.endpoints.json

FROM base AS final
WORKDIR /app

COPY --from=build /out .
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["/app/PigeonHorde"]