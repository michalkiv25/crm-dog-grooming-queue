# Build and run ASP.NET Core API (Render / Fly.io / etc.)
# linux/amd64: Render runs x86_64; building on Apple Silicon without this can produce wrong-arch images.
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY DogQueueApi.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish DogQueueApi.csproj -c Release -o /app/publish

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Without Production, appsettings.Production.json is skipped → base appsettings may point at
# SQL Server LocalDB, which crashes on Linux (often exit 139). Render still injects PORT at runtime.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "DogQueueApi.dll"]
