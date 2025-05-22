# First stage: restore
FROM docker.xuanyuan.me/fallenwood/garnet:20250522.1 AS base
WORKDIR /pigeonhorde
ENV LANG zh_CN.UTF-8

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN cd src/PigeonHorde && dotnet publish -c Release -p:PublishSingleFile=true --self-contained true -o /pigeonhorde && rm -f /pigeonhorde/appsettings.Development.json

FROM base AS final
WORKDIR /pigeonhorde
COPY --from=build /pigeonhorde .
CMD ["/pigeonhorde/PigeonHorde"]