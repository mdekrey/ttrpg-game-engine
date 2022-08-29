#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GameEngine.Web/GameEngine.Web.csproj", "GameEngine.Web/"]
COPY ["GameEngine/GameEngine.csproj", "GameEngine/"]
RUN dotnet restore "GameEngine.Web/GameEngine.Web.csproj"
COPY ./schemas/ ./schemas/
COPY ./GameEngine.Web/ ./GameEngine.Web/
COPY ./GameEngine/ ./GameEngine/
WORKDIR "/src/GameEngine.Web"
RUN dotnet build "GameEngine.Web.csproj" -c Release -o /app/build /p:SkipUiBuild=true

FROM build AS publish
RUN dotnet publish "GameEngine.Web.csproj" -c Release -o /app/publish /p:SkipUiBuild=true

FROM node:16.14 AS build-ui
WORKDIR /src
RUN wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
  && dpkg -i packages-microsoft-prod.deb \
  && rm packages-microsoft-prod.deb \
  && apt-get update \
  && apt-get install -y apt-transport-https aspnetcore-runtime-6.0 \
  && apt-get clean

COPY ["./game-powers-ui/package.json", "./game-powers-ui/"]
COPY ["./game-powers-ui/package-lock.json", "./game-powers-ui/"]
RUN cd ./game-powers-ui/ && npm ci

COPY ./schemas/ ./schemas/
COPY ./game-powers-ui/ ./game-powers-ui/

WORKDIR "/src/game-powers-ui"
RUN npm run generate-openapi
RUN CI=true npm test
RUN npm run lint
RUN npm run build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build-ui /src/GameEngine.Web/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "GameEngine.Web.dll"]