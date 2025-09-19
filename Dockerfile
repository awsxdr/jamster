FROM mcr.microsoft.com/dotnet/sdk:8.0 AS apibuild

WORKDIR /api
COPY ["jamster", "./"]

RUN dotnet restore ./jamster.csproj
RUN dotnet publish -c Release -a x64 -o /api/publish --sc ./jamster.csproj

FROM node:24 AS uibuild

WORKDIR /ui
COPY ["jamster.ui", "./"]

RUN npm i
RUN npm run build

FROM debian:latest AS run

WORKDIR /jamster

RUN apt-get update && apt-get install -y libssl-dev

COPY --from=apibuild /api/publish ./bin/linux-x64
COPY --from=uibuild /ui/dist ./wwwroot

VOLUME /jamster/db

EXPOSE 80

ENTRYPOINT ["./bin/linux-x64/jamster", "-p", "80"]
