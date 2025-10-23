# 🚀 QOPIQ Monitor de Impresoras - Dockerfile de Producción
# Optimizado para .NET 8 con múltiples etapas para reducir tamaño

# ==========================================
# Etapa 1: Build Environment
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para optimizar cache de Docker
COPY ["QOPIQ.API/QOPIQ.API.csproj", "QOPIQ.API/"]
COPY ["QOPIQ.Application/QOPIQ.Application.csproj", "QOPIQ.Application/"]
COPY ["QOPIQ.Domain/QOPIQ.Domain.csproj", "QOPIQ.Domain/"]
COPY ["QOPIQ.Infrastructure/QOPIQ.Infrastructure.csproj", "QOPIQ.Infrastructure/"]

# Restaurar dependencias (se cachea si no cambian los .csproj)
RUN dotnet restore "QOPIQ.API/QOPIQ.API.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar en modo Release
WORKDIR "/src/QOPIQ.API"
RUN dotnet build "QOPIQ.API.csproj" -c Release -o /app/build

# ==========================================
# Etapa 2: Publish
# ==========================================
FROM build AS publish
RUN dotnet publish "QOPIQ.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Etapa 3: Runtime Environment
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Instalar herramientas necesarias para health checks
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Crear usuario no privilegiado para seguridad
RUN addgroup --system --gid 1001 qopiq \
    && adduser --system --uid 1001 --gid 1001 --shell /bin/false qopiq

# Configurar directorio de trabajo
WORKDIR /app

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Crear directorio de logs con permisos correctos
RUN mkdir -p /app/logs && chown -R qopiq:qopiq /app/logs

# Configurar variables de entorno de producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Exponer puerto
EXPOSE 80
EXPOSE 443

# Cambiar a usuario no privilegiado
USER qopiq

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "QOPIQ.API.dll"]
