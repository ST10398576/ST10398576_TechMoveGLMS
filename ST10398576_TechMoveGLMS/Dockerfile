FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the full project
COPY ST10398576_TechMoveGLMS/. ./ST10398576_TechMoveGLMS/

# Remove any Windows-generated obj/bin folders that may have been copied in.
# These can contain a project.assets.json pointing at Visual Studio's local
# NuGet fallback folder, which doesn't exist inside this Linux container.
RUN rm -rf ./ST10398576_TechMoveGLMS/obj ./ST10398576_TechMoveGLMS/bin

WORKDIR /src/ST10398576_TechMoveGLMS
RUN dotnet restore ST10398576_TechMoveGLMS.csproj
RUN dotnet publish ST10398576_TechMoveGLMS.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ST10398576_TechMoveGLMS.dll"]
