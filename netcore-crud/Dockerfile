# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

# Install Linux dependencies required by DevExpress Reporting
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libicu-dev \
        libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# Run the application as a non-root user
USER $APP_UID

EXPOSE 8080
EXPOSE 8081


# Stage 2: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy project file and restore dependencies
COPY ["netcore-crud.csproj", "./"]

RUN dotnet restore "./netcore-crud.csproj"

# Copy the remaining source code
COPY . .

# Build the application
RUN dotnet build "./netcore-crud.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build


# Stage 3: Publish the application
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./netcore-crud.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# Stage 4: Final runtime image
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "netcore-crud.dll"]