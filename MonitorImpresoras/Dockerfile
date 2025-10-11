# Dockerfile para Monitor de Impresoras
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Instalar dependencias del sistema para SNMP y WMI
RUN apt-get update && apt-get install -y \
    snmp \
    iputils-ping \
    net-tools \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["QOPIQ.API/QOPIQ.API.csproj", "QOPIQ.API/"]
COPY ["QOPIQ.Application/QOPIQ.Application.csproj", "QOPIQ.Application/"]
COPY ["QOPIQ.Domain/QOPIQ.Domain.csproj", "QOPIQ.Domain/"]
COPY ["QOPIQ.Infrastructure/QOPIQ.Infrastructure.csproj", "QOPIQ.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "QOPIQ.API/QOPIQ.API.csproj"

# Copiar código fuente
COPY . .

# Compilar aplicación
WORKDIR "/src/QOPIQ.API"
RUN dotnet build "QOPIQ.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QOPIQ.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "QOPIQ.API.dll"]
