#!/bin/bash
# QOPIQ Monitor - Script de Despliegue Automatizado
# Bash Script para Linux/Mac

set -e  # Exit on any error

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Variables por defecto
ENVIRONMENT=${1:-production}
BUILD_ONLY=false
SKIP_TESTS=false

# Procesar argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        --build-only)
            BUILD_ONLY=true
            shift
            ;;
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        *)
            ENVIRONMENT=$1
            shift
            ;;
    esac
done

echo -e "${GREEN}🚀 QOPIQ Monitor - Despliegue Automatizado${NC}"
echo -e "${YELLOW}Entorno: $ENVIRONMENT${NC}"

# Función para verificar si Docker está instalado
check_docker() {
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}❌ Docker no está instalado${NC}"
        echo -e "${RED}Por favor instale Docker desde https://docs.docker.com/get-docker/${NC}"
        exit 1
    fi
    return 0
}

# Función para verificar si Docker Compose está disponible
check_docker_compose() {
    if ! docker compose version &> /dev/null; then
        echo -e "${RED}❌ Docker Compose no está disponible${NC}"
        echo -e "${RED}Asegúrese de tener Docker actualizado con Compose V2${NC}"
        exit 1
    fi
    return 0
}

# Verificar prerrequisitos
echo -e "${CYAN}🔍 Verificando prerrequisitos...${NC}"

check_docker
check_docker_compose

echo -e "${GREEN}✅ Docker y Docker Compose están disponibles${NC}"

# Limpiar builds anteriores
echo -e "${CYAN}🧹 Limpiando builds anteriores...${NC}"
if command -v dotnet &> /dev/null; then
    dotnet clean &> /dev/null || true
fi
rm -rf ./publish 2>/dev/null || true

# Ejecutar tests (si no se omiten)
if [ "$SKIP_TESTS" = false ] && command -v dotnet &> /dev/null; then
    echo -e "${CYAN}🧪 Ejecutando tests...${NC}"
    if ! dotnet test --configuration Release --logger "console;verbosity=minimal"; then
        echo -e "${RED}❌ Tests fallaron. Abortando despliegue.${NC}"
        exit 1
    fi
    echo -e "${GREEN}✅ Tests pasaron exitosamente${NC}"
fi

# Build de aplicaciones (si dotnet está disponible)
if command -v dotnet &> /dev/null; then
    echo -e "${CYAN}🔨 Compilando aplicaciones...${NC}"

    echo -e "${YELLOW}📦 Publicando Backend...${NC}"
    if ! dotnet publish QOPIQ.API -c Release -o ./publish/Backend --self-contained false; then
        echo -e "${RED}❌ Error al publicar Backend${NC}"
        exit 1
    fi

    echo -e "${YELLOW}📦 Publicando Frontend...${NC}"
    if ! dotnet publish QOPIQ.Frontend -c Release -o ./publish/Frontend --self-contained false; then
        echo -e "${RED}❌ Error al publicar Frontend${NC}"
        exit 1
    fi

    echo -e "${GREEN}✅ Aplicaciones compiladas exitosamente${NC}"
fi

# Si solo es build, terminar aquí
if [ "$BUILD_ONLY" = true ]; then
    echo -e "${GREEN}✅ Build completado. Archivos disponibles en ./publish/${NC}"
    exit 0
fi

# Configurar variables de entorno
echo -e "${CYAN}⚙️ Configurando variables de entorno...${NC}"
if [ -f ".env.$ENVIRONMENT" ]; then
    cp ".env.$ENVIRONMENT" ".env"
    echo -e "${GREEN}✅ Variables de entorno cargadas desde .env.$ENVIRONMENT${NC}"
else
    echo -e "${YELLOW}⚠️ Archivo .env.$ENVIRONMENT no encontrado. Usando valores por defecto.${NC}"
fi

# Construir contenedores Docker
echo -e "${CYAN}🐳 Construyendo contenedores Docker...${NC}"
if ! docker compose build --no-cache; then
    echo -e "${RED}❌ Error al construir contenedores Docker${NC}"
    exit 1
fi

# Iniciar servicios
echo -e "${CYAN}🚀 Iniciando servicios...${NC}"
if ! docker compose up -d; then
    echo -e "${RED}❌ Error al iniciar servicios${NC}"
    exit 1
fi

# Esperar a que los servicios estén listos
echo -e "${CYAN}⏳ Esperando a que los servicios estén listos...${NC}"
sleep 30

# Verificar estado de servicios
echo -e "${CYAN}🔍 Verificando estado de servicios...${NC}"
docker compose ps

# Health checks
echo -e "${CYAN}🏥 Ejecutando health checks...${NC}"

API_HEALTHY=false
FRONTEND_HEALTHY=false

# Check API Health
for i in {1..10}; do
    if curl -f -s http://localhost:5278/health > /dev/null 2>&1; then
        API_HEALTHY=true
        echo -e "${GREEN}✅ API Health Check: OK${NC}"
        break
    else
        echo -e "${YELLOW}⏳ API Health Check: Esperando... (intento $i/10)${NC}"
        sleep 5
    fi
done

# Check Frontend Health
for i in {1..10}; do
    if curl -f -s http://localhost:5000 > /dev/null 2>&1; then
        FRONTEND_HEALTHY=true
        echo -e "${GREEN}✅ Frontend Health Check: OK${NC}"
        break
    else
        echo -e "${YELLOW}⏳ Frontend Health Check: Esperando... (intento $i/10)${NC}"
        sleep 5
    fi
done

# Resumen final
echo ""
echo -e "${GREEN}🎉 DESPLIEGUE COMPLETADO${NC}"
echo -e "${GREEN}=========================${NC}"

if [ "$API_HEALTHY" = true ]; then
    echo -e "${GREEN}✅ API: http://localhost:5278${NC}"
    echo -e "${GREEN}✅ Swagger: http://localhost:5278/swagger${NC}"
else
    echo -e "${RED}❌ API: No responde en http://localhost:5278${NC}"
fi

if [ "$FRONTEND_HEALTHY" = true ]; then
    echo -e "${GREEN}✅ Frontend: http://localhost:5000${NC}"
else
    echo -e "${RED}❌ Frontend: No responde en http://localhost:5000${NC}"
fi

echo -e "${CYAN}📊 Base de Datos: PostgreSQL en puerto 5432${NC}"
echo -e "${CYAN}🔄 Redis Cache: Puerto 6379${NC}"

echo ""
echo -e "${YELLOW}📋 Comandos útiles:${NC}"
echo -e "${NC}docker compose logs -f          # Ver logs en tiempo real${NC}"
echo -e "${NC}docker compose stop             # Detener servicios${NC}"
echo -e "${NC}docker compose restart          # Reiniciar servicios${NC}"
echo -e "${NC}docker compose down             # Detener y eliminar contenedores${NC}"

if [ "$API_HEALTHY" = true ] && [ "$FRONTEND_HEALTHY" = true ]; then
    echo ""
    echo -e "${GREEN}🎯 Sistema QOPIQ Monitor desplegado exitosamente!${NC}"
    exit 0
else
    echo ""
    echo -e "${YELLOW}⚠️ Algunos servicios no están respondiendo. Revise los logs.${NC}"
    echo -e "${NC}Ejecute: docker compose logs -f${NC}"
    exit 1
fi
