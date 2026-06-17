# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ST10398576_TechMoveGLMS/*.csproj ./ST10398576_TechMoveGLMS/
RUN dotnet restore ./ST10398576_TechMoveGLMS/ST10398576_TechMoveGLMS.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/ST10398576_TechMoveGLMS
RUN dotnet build ST10398576_TechMoveGLMS.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish ST10398576_TechMoveGLMS.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ST10398576_TechMoveGLMS.dll"]
