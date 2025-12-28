# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["EcommerceAPI.csproj", "./"]
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install culture data for globalization
RUN apt-get update && apt-get install -y locales

# Create non-root user
RUN groupadd -g 1000 appuser && \
    useradd -r -u 1000 -g appuser appuser

# Copy published files
COPY --from=build /app/publish .

# Create directories for file uploads
RUN mkdir -p wwwroot/uploads && \
    chown -R appuser:appuser wwwroot

# Switch to non-root user
USER appuser

# Expose port (Koyeb uses 8080)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]