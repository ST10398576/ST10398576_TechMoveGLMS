FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
 
# Copy the API project in full
COPY TechMoveGLMS.API/. ./TechMoveGLMS.API/
 
# Copy ONLY the MVC project's Models folder. The API now compiles these .cs
# files directly (see TechMoveGLMS.API.csproj) instead of referencing the
# whole MVC project, so nothing else from that project is needed here.
COPY ST10398576_TechMoveGLMS/Models/. ./ST10398576_TechMoveGLMS/Models/
 
# Remove any Windows-generated obj/bin that may have been copied in with the API project.
RUN rm -rf ./TechMoveGLMS.API/obj ./TechMoveGLMS.API/bin
 
WORKDIR /src/TechMoveGLMS.API
RUN dotnet restore TechMoveGLMS.API.csproj
RUN dotnet publish TechMoveGLMS.API.csproj -c Release -o /app
 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
 
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
 
ENTRYPOINT ["dotnet", "TechMoveGLMS.API.dll"]
