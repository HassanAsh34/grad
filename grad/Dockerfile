# Use .NET 9 runtime as base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Expose both HTTP and HTTPS
EXPOSE 80
EXPOSE 443

# Let Kestrel listen on all interfaces for both HTTP and HTTPS
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore first (caching)
COPY ["grad.csproj", "./"]
RUN dotnet restore "./grad.csproj"

# Copy the rest and build
COPY . .
RUN dotnet build "./grad.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./grad.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Copy certificate into container
COPY ./certs/cert.pfx ./certs/cert.pfx

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "grad.dll"]
