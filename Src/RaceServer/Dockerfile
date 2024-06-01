#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim-arm64v8 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RaceServer/RaceServer.csproj", "RaceServer/"]
COPY ["MqttService/MqttService.csproj", "MqttService/"]
RUN dotnet restore "./RaceServer/RaceServer.csproj"
COPY . .
WORKDIR "/src/RaceServer"
RUN dotnet build "./RaceServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RaceServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -r linux-arm64 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceServer.dll"]