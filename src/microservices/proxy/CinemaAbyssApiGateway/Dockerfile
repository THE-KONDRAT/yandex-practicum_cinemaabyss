FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CinemaAbyssApiGateway.csproj", "./"]
RUN dotnet restore "CinemaAbyssApiGateway.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./CinemaAbyssApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN apt-get update && apt-get install -y --no-install-recommends clang zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*
RUN dotnet publish "./CinemaAbyssApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    -r linux-x64 --self-contained /p:PublishAot=true /p:StripSymbols=true

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "CinemaAbyssApiGateway.dll"]
ENTRYPOINT ["./CinemaAbyssApiGateway"]