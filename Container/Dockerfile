# AgilitySports API - multi-stage .NET 8 image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["agility-sports-api.csproj", "."]
RUN dotnet restore "agility-sports-api.csproj"
COPY . .
RUN dotnet build "agility-sports-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "agility-sports-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure log4net config is present even if publish globs omit it
COPY log4net.config ./log4net.config
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "agility-sports-api.dll"]