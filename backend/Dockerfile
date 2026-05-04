# Build and run ASP.NET Core API (Render / Fly.io / etc.)
# linux/amd64: Render runs x86_64; building on Apple Silicon without this can produce wrong-arch images.
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY DogQueue.WebApi/DogQueue.WebApi.csproj DogQueue.WebApi/
RUN dotnet restore DogQueue.WebApi/DogQueue.WebApi.csproj
COPY DogQueue.WebApi/ DogQueue.WebApi/
WORKDIR /src/DogQueue.WebApi
RUN dotnet publish -c Release -o /app/publish

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# SQL Server only: set ConnectionStrings__DefaultConnection (and Jwt__SecretKey) at runtime.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "DogQueue.WebApi.dll"]
