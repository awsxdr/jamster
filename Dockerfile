FROM mcr.microsoft.com/dotnet/sdk:8.0 AS apibuild

WORKDIR /api
COPY ["amethyst", "./"]

RUN dotnet restore ./amethyst.csproj
RUN dotnet publish -c Release -a x64 -o /api/publish --sc ./amethyst.csproj

FROM node:24 AS uibuild

WORKDIR /ui
COPY ["amethyst.ui", "./"]

RUN npm i
RUN npm run build

FROM debian:latest AS run

WORKDIR /amethyst

RUN apt-get update && apt-get install -y libssl-dev

COPY --from=apibuild /api/publish ./bin/linux-x64
COPY --from=uibuild /ui/dist ./wwwroot

VOLUME /amethyst/db

EXPOSE 80

ENTRYPOINT ["./bin/linux-x64/amethyst", "-p", "80"]
