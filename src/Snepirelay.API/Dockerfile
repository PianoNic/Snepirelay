# syntax=docker/dockerfile:1.7

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "global.json", "application.properties", "./"]
COPY ["src/Snepirelay.Domain/Snepirelay.Domain.csproj", "src/Snepirelay.Domain/"]
COPY ["src/Snepirelay.Infrastructure/Snepirelay.Infrastructure.csproj", "src/Snepirelay.Infrastructure/"]
COPY ["src/Snepirelay.Application/Snepirelay.Application.csproj", "src/Snepirelay.Application/"]
COPY ["src/Snepirelay.API/Snepirelay.API.csproj", "src/Snepirelay.API/"]
RUN dotnet restore "src/Snepirelay.API/Snepirelay.API.csproj" -a $TARGETARCH
COPY src ./src
RUN dotnet publish "src/Snepirelay.API/Snepirelay.API.csproj" -c $BUILD_CONFIGURATION -a $TARGETARCH --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-azurelinux3.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
USER app
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD curl -fsS http://localhost:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "Snepirelay.API.dll"]
