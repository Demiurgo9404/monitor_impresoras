#!/bin/bash

echo "🚀 Probando el sistema de reportes de PrintHub"
echo "=============================================="

# Colores para output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para imprimir mensajes
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Verificar que la aplicación compile
print_status "Verificando compilación..."
cd "c:\Users\Demiurgo\Documents\GitHub\monitor_impresoras\MonitorImpresoras"

if dotnet build --no-restore > build_output.log 2>&1; then
    print_status "✅ Compilación exitosa"
else
    print_error "❌ Error en compilación"
    cat build_output.log
    exit 1
fi

# Verificar que la API inicie correctamente
print_status "Iniciando API en modo desarrollo..."
timeout 30s dotnet run --project MonitorImpresoras.API/MonitorImpresoras.API.csproj > api_output.log 2>&1 &
API_PID=$!

# Esperar un poco para que la API inicie
sleep 10

# Verificar si la API está respondiendo
print_status "Verificando que la API responda..."
if curl -s http://localhost:5000/swagger > /dev/null; then
    print_status "✅ API respondiendo correctamente"
else
    print_warning "⚠️ API no responde en puerto 5000, verificando puerto 5001..."
    if curl -s http://localhost:5001/swagger > /dev/null; then
        print_status "✅ API respondiendo en puerto 5001"
        API_PORT=5001
    else
        print_error "❌ API no responde en ningún puerto"
        kill $API_PID 2>/dev/null
        exit 1
    fi
fi

# Probar el endpoint de prueba
print_status "Ejecutando prueba del sistema de reportes..."
if [ -z "$API_PORT" ]; then API_PORT=5000; fi

TEST_RESPONSE=$(curl -s -X POST http://localhost:$API_PORT/api/report/test -H "Content-Type: application/json")

if echo "$TEST_RESPONSE" | grep -q "Prueba completada exitosamente"; then
    print_status "✅ Prueba del sistema de reportes exitosa"
    echo "$TEST_RESPONSE" | grep -E "(TemplateId|ExecutionId|ScheduledFor)" | sed 's/^[[:space:]]*//' | sed 's/"//g'
else
    print_error "❌ Error en la prueba del sistema de reportes"
    echo "Respuesta: $TEST_RESPONSE"
fi

# Probar endpoints individuales
print_status "Probando endpoints individuales..."

# Crear plantilla
TEMPLATE_RESPONSE=$(curl -s -X POST http://localhost:$API_PORT/api/report/templates \
    -H "Content-Type: application/json" \
    -d '{
        "name": "Test Template",
        "description": "Template for testing",
        "reportType": 0,
        "format": 0,
        "isActive": true,
        "createdBy": "test"
    }')

if echo "$TEMPLATE_RESPONSE" | grep -q "id"; then
    print_status "✅ Creación de plantilla exitosa"
else
    print_warning "⚠️ Error creando plantilla: $TEMPLATE_RESPONSE"
fi

# Obtener plantillas
TEMPLATES_RESPONSE=$(curl -s http://localhost:$API_PORT/api/report/templates)

if echo "$TEMPLATES_RESPONSE" | grep -q "\[\]"; then
    print_status "✅ Obtención de plantillas exitosa"
else
    print_warning "⚠️ Error obteniendo plantillas: $TEMPLATES_RESPONSE"
fi

# Detener la API
print_status "Deteniendo API..."
kill $API_PID 2>/dev/null
sleep 2

print_status "🎉 Pruebas completadas"
print_status "📋 Revisa api_output.log y build_output.log para detalles"
print_status "💡 Para ejecutar manualmente: cd MonitorImpresoras && dotnet run --project MonitorImpresoras.API/MonitorImpresoras.API.csproj"
