# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY TechMoveGLMS.API/*.csproj ./TechMoveGLMS.API/
RUN dotnet restore ./TechMoveGLMS.API/ST10398576_TechMoveGLMS.API.csproj

# Copy everything else and build
COPY . .
WORKDIR /repos/TechMoveGLMS.API
RUN dotnet build ST10398576_TechMoveGLMS.API.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish ST10398576_TechMoveGLMS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy a small startup script that waits for the database to be reachable before starting the app.
COPY TechMoveGLMS.API/wait-for-db.sh /wait-for-db.sh
RUN chmod +x /wait-for-db.sh
ENTRYPOINT ["/wait-for-db.sh"]
