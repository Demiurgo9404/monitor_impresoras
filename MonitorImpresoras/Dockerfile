# Dockerfile para Monitor de Impresoras
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Instalar dependencias del sistema para SNMP y WMI
RUN apt-get update && apt-get install -y \
    snmp \
    snmp-mibs-downloader \
    iputils-ping \
    net-tools \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["MonitorImpresoras.API/MonitorImpresoras.API.csproj", "MonitorImpresoras.API/"]
COPY ["MonitorImpresoras.Application/MonitorImpresoras.Application.csproj", "MonitorImpresoras.Application/"]
COPY ["MonitorImpresoras.Domain/MonitorImpresoras.Domain.csproj", "MonitorImpresoras.Domain/"]
COPY ["MonitorImpresoras.Infrastructure/MonitorImpresoras.Infrastructure.csproj", "MonitorImpresoras.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "MonitorImpresoras.API/MonitorImpresoras.API.csproj"

# Copiar código fuente
COPY . .

# Compilar aplicación
WORKDIR "/src/MonitorImpresoras.API"
RUN dotnet build "MonitorImpresoras.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MonitorImpresoras.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Crear usuario no privilegiado
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar aplicación publicada
COPY --from=publish /app/publish .

# Crear directorios para logs
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Cambiar a usuario no privilegiado
USER appuser

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "MonitorImpresoras.API.dll"]
