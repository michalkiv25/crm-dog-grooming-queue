# Build and run ASP.NET Core API (Render / Fly.io / etc.)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY DogQueueApi.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish DogQueueApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Render sets PORT; map via ASPNETCORE_URLS in dashboard (http://0.0.0.0:$PORT)
EXPOSE 8080
ENTRYPOINT ["dotnet", "DogQueueApi.dll"]
