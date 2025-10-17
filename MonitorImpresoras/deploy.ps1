# QOPIQ Monitor - Script de Despliegue Automatizado
# PowerShell Script para Windows

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "production",
    
    [Parameter(Mandatory=$false)]
    [switch]$BuildOnly = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests = $false
)

Write-Host "🚀 QOPIQ Monitor - Despliegue Automatizado" -ForegroundColor Green
Write-Host "Entorno: $Environment" -ForegroundColor Yellow

# Función para verificar si Docker está instalado
function Test-Docker {
    try {
        docker --version | Out-Null
        return $true
    }
    catch {
        Write-Host "❌ Docker no está instalado o no está en el PATH" -ForegroundColor Red
        return $false
    }
}

# Función para verificar si Docker Compose está disponible
function Test-DockerCompose {
    try {
        docker compose version | Out-Null
        return $true
    }
    catch {
        Write-Host "❌ Docker Compose no está disponible" -ForegroundColor Red
        return $false
    }
}

# Verificar prerrequisitos
Write-Host "🔍 Verificando prerrequisitos..." -ForegroundColor Cyan

if (-not (Test-Docker)) {
    Write-Host "Por favor instale Docker Desktop desde https://www.docker.com/products/docker-desktop" -ForegroundColor Red
    exit 1
}

if (-not (Test-DockerCompose)) {
    Write-Host "Docker Compose no está disponible. Asegúrese de tener Docker Desktop actualizado." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Docker y Docker Compose están disponibles" -ForegroundColor Green

# Limpiar builds anteriores
Write-Host "🧹 Limpiando builds anteriores..." -ForegroundColor Cyan
dotnet clean
Remove-Item -Path "./publish" -Recurse -Force -ErrorAction SilentlyContinue

# Ejecutar tests (si no se omiten)
if (-not $SkipTests) {
    Write-Host "🧪 Ejecutando tests..." -ForegroundColor Cyan
    $testResult = dotnet test --configuration Release --logger "console;verbosity=minimal"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tests fallaron. Abortando despliegue." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Tests pasaron exitosamente" -ForegroundColor Green
}

# Build de aplicaciones
Write-Host "🔨 Compilando aplicaciones..." -ForegroundColor Cyan

Write-Host "📦 Publicando Backend..." -ForegroundColor Yellow
dotnet publish QOPIQ.API -c Release -o ./publish/Backend --self-contained false
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al publicar Backend" -ForegroundColor Red
    exit 1
}

Write-Host "📦 Publicando Frontend..." -ForegroundColor Yellow
dotnet publish QOPIQ.Frontend -c Release -o ./publish/Frontend --self-contained false
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al publicar Frontend" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Aplicaciones compiladas exitosamente" -ForegroundColor Green

# Si solo es build, terminar aquí
if ($BuildOnly) {
    Write-Host "✅ Build completado. Archivos disponibles en ./publish/" -ForegroundColor Green
    exit 0
}

# Configurar variables de entorno
Write-Host "⚙️ Configurando variables de entorno..." -ForegroundColor Cyan
if (Test-Path ".env.$Environment") {
    Copy-Item ".env.$Environment" ".env"
    Write-Host "✅ Variables de entorno cargadas desde .env.$Environment" -ForegroundColor Green
} else {
    Write-Host "⚠️ Archivo .env.$Environment no encontrado. Usando valores por defecto." -ForegroundColor Yellow
}

# Construir contenedores Docker
Write-Host "🐳 Construyendo contenedores Docker..." -ForegroundColor Cyan
docker compose build --no-cache
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al construir contenedores Docker" -ForegroundColor Red
    exit 1
}

# Iniciar servicios
Write-Host "🚀 Iniciando servicios..." -ForegroundColor Cyan
docker compose up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al iniciar servicios" -ForegroundColor Red
    exit 1
}

# Esperar a que los servicios estén listos
Write-Host "⏳ Esperando a que los servicios estén listos..." -ForegroundColor Cyan
Start-Sleep -Seconds 30

# Verificar estado de servicios
Write-Host "🔍 Verificando estado de servicios..." -ForegroundColor Cyan
docker compose ps

# Health checks
Write-Host "🏥 Ejecutando health checks..." -ForegroundColor Cyan

$apiHealthy = $false
$frontendHealthy = $false

for ($i = 1; $i -le 10; $i++) {
    try {
        $apiResponse = Invoke-WebRequest -Uri "http://localhost:5278/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($apiResponse.StatusCode -eq 200) {
            $apiHealthy = $true
            Write-Host "✅ API Health Check: OK" -ForegroundColor Green
            break
        }
    }
    catch {
        Write-Host "⏳ API Health Check: Esperando... (intento $i/10)" -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
}

for ($i = 1; $i -le 10; $i++) {
    try {
        $frontendResponse = Invoke-WebRequest -Uri "http://localhost:5000" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($frontendResponse.StatusCode -eq 200) {
            $frontendHealthy = $true
            Write-Host "✅ Frontend Health Check: OK" -ForegroundColor Green
            break
        }
    }
    catch {
        Write-Host "⏳ Frontend Health Check: Esperando... (intento $i/10)" -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
}

# Resumen final
Write-Host "`n🎉 DESPLIEGUE COMPLETADO" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

if ($apiHealthy) {
    Write-Host "✅ API: http://localhost:5278" -ForegroundColor Green
    Write-Host "✅ Swagger: http://localhost:5278/swagger" -ForegroundColor Green
} else {
    Write-Host "❌ API: No responde en http://localhost:5278" -ForegroundColor Red
}

if ($frontendHealthy) {
    Write-Host "✅ Frontend: http://localhost:5000" -ForegroundColor Green
} else {
    Write-Host "❌ Frontend: No responde en http://localhost:5000" -ForegroundColor Red
}

Write-Host "📊 Base de Datos: PostgreSQL en puerto 5432" -ForegroundColor Cyan
Write-Host "🔄 Redis Cache: Puerto 6379" -ForegroundColor Cyan

Write-Host "`n📋 Comandos útiles:" -ForegroundColor Yellow
Write-Host "docker compose logs -f          # Ver logs en tiempo real" -ForegroundColor White
Write-Host "docker compose stop             # Detener servicios" -ForegroundColor White
Write-Host "docker compose restart          # Reiniciar servicios" -ForegroundColor White
Write-Host "docker compose down             # Detener y eliminar contenedores" -ForegroundColor White

if ($apiHealthy -and $frontendHealthy) {
    Write-Host "`n🎯 Sistema QOPIQ Monitor desplegado exitosamente!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n⚠️ Algunos servicios no están respondiendo. Revise los logs." -ForegroundColor Yellow
    Write-Host "Ejecute: docker compose logs -f" -ForegroundColor White
    exit 1
}
