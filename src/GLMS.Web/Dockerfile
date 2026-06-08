# Multi-stage build for MVC Web App
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/GLMS.Web/GLMS.Web.csproj", "src/GLMS.Web/"]
RUN dotnet restore "src/GLMS.Web/GLMS.Web.csproj"

COPY . .
WORKDIR "/src/src/GLMS.Web"
RUN dotnet build "GLMS.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GLMS.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3     CMD wget --no-verbose --tries=1 --spider http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "GLMS.Web.dll"]