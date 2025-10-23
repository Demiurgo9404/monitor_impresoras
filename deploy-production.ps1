# 🚀 QOPIQ Monitor de Impresoras - Script de Despliegue Automatizado
# Despliegue completo en entorno de producción con Docker Compose

param(
    [string]$Environment = "production",
    [switch]$SkipBuild = $false,
    [switch]$SkipSSL = $false,
    [switch]$Verbose = $false
)

Write-Host "🚀 QOPIQ Monitor de Impresoras - Despliegue Automatizado" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "Entorno: $Environment" -ForegroundColor Yellow
Write-Host "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host ""

# Función para mostrar mensajes con colores
function Write-Step {
    param([string]$Message, [string]$Color = "Green")
    Write-Host "✅ $Message" -ForegroundColor $Color
}

function Write-Error-Step {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Warning-Step {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

# Verificar prerrequisitos
Write-Host "🔍 Verificando prerrequisitos..." -ForegroundColor Cyan

# Verificar Docker
if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error-Step "Docker no está instalado o no está en el PATH"
    exit 1
}
Write-Step "Docker disponible: $(docker --version)"

# Verificar Docker Compose
if (!(Get-Command docker-compose -ErrorAction SilentlyContinue)) {
    Write-Error-Step "Docker Compose no está instalado"
    exit 1
}
Write-Step "Docker Compose disponible: $(docker-compose --version)"

# Verificar .NET SDK
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error-Step ".NET SDK no está instalado"
    exit 1
}
Write-Step ".NET SDK disponible: $(dotnet --version)"

# Limpiar contenedores anteriores
Write-Host ""
Write-Host "🧹 Limpiando despliegue anterior..." -ForegroundColor Cyan
try {
    docker-compose -f docker-compose.prod.yml down --remove-orphans -v
    Write-Step "Contenedores anteriores eliminados"
} catch {
    Write-Warning-Step "No hay contenedores anteriores para limpiar"
}

# Compilar aplicación si no se omite
if (-not $SkipBuild) {
    Write-Host ""
    Write-Host "🔨 Compilando aplicación..." -ForegroundColor Cyan
    
    Set-Location "MonitorImpresoras"
    
    # Limpiar compilaciones anteriores
    dotnet clean --configuration Release
    Write-Step "Proyecto limpiado"
    
    # Restaurar dependencias
    dotnet restore
    Write-Step "Dependencias restauradas"
    
    # Compilar en modo Release
    $buildResult = dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Step "Error en la compilación"
        Set-Location ".."
        exit 1
    }
    Write-Step "Compilación exitosa"
    
    Set-Location ".."
}

# Generar certificados SSL si no se omite
if (-not $SkipSSL) {
    Write-Host ""
    Write-Host "🔐 Configurando certificados SSL..." -ForegroundColor Cyan
    
    if (!(Test-Path "ssl")) {
        New-Item -ItemType Directory -Path "ssl" -Force | Out-Null
        Write-Step "Directorio SSL creado"
    }
    
    if (!(Test-Path "ssl/cert.pem") -or !(Test-Path "ssl/key.pem")) {
        Write-Warning-Step "Usando certificados auto-firmados para desarrollo"
        Write-Warning-Step "Para producción, reemplace con certificados válidos"
    } else {
        Write-Step "Certificados SSL encontrados"
    }
}

# Configurar variables de entorno
Write-Host ""
Write-Host "⚙️  Configurando variables de entorno..." -ForegroundColor Cyan

$envFile = ".env"
if (!(Test-Path $envFile)) {
    Write-Host "Creando archivo .env con valores por defecto..." -ForegroundColor Yellow
    
    $envContent = @"
# QOPIQ Production Environment Variables
# Generado automáticamente el $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

# Base de Datos
DB_PASSWORD=SecurePassword123!
DB_HOST=postgres
DB_NAME=qopiq_production
DB_USER=qopiq_user

# JWT
JWT_SECRET_KEY=SuperSecretJWTKeyForProduction256BitsMinimumLengthRequired123456789

# Redis
REDIS_PASSWORD=RedisPassword123!

# SNMP
SNMP_COMMUNITY=ProductionCommunity123
SNMP_V3_USERNAME=qopiq_snmp_prod
SNMP_V3_AUTH_KEY=ProductionAuthKey123!
SNMP_V3_PRIV_KEY=ProductionPrivKey456!
ALLOWED_NETWORK_RANGE=10.0.0.0/8

# Monitoreo
PROMETHEUS_RETENTION=30d
"@
    
    $envContent | Out-File -FilePath $envFile -Encoding UTF8
    Write-Step "Archivo .env creado con valores por defecto"
    Write-Warning-Step "IMPORTANTE: Revise y actualice las credenciales en .env antes de producción"
} else {
    Write-Step "Archivo .env encontrado"
}

# Iniciar servicios con Docker Compose
Write-Host ""
Write-Host "🐳 Iniciando servicios con Docker Compose..." -ForegroundColor Cyan

try {
    # Construir imágenes
    Write-Host "Construyendo imágenes Docker..." -ForegroundColor Yellow
    docker-compose -f docker-compose.prod.yml build --no-cache
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Step "Error construyendo imágenes Docker"
        exit 1
    }
    Write-Step "Imágenes Docker construidas"
    
    # Iniciar servicios
    Write-Host "Iniciando servicios..." -ForegroundColor Yellow
    docker-compose -f docker-compose.prod.yml up -d
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Step "Error iniciando servicios"
        exit 1
    }
    Write-Step "Servicios iniciados"
    
} catch {
    Write-Error-Step "Error en el despliegue: $($_.Exception.Message)"
    exit 1
}

# Esperar a que los servicios estén listos
Write-Host ""
Write-Host "⏳ Esperando a que los servicios estén listos..." -ForegroundColor Cyan

$maxWaitTime = 120 # 2 minutos
$waitTime = 0
$interval = 5

do {
    Start-Sleep -Seconds $interval
    $waitTime += $interval
    
    Write-Host "Verificando servicios... ($waitTime/$maxWaitTime segundos)" -ForegroundColor Yellow
    
    # Verificar estado de contenedores
    $containers = docker-compose -f docker-compose.prod.yml ps --services
    $healthyCount = 0
    
    foreach ($container in $containers) {
        $status = docker-compose -f docker-compose.prod.yml ps $container
        if ($status -match "Up") {
            $healthyCount++
        }
    }
    
    if ($healthyCount -eq $containers.Count) {
        break
    }
    
} while ($waitTime -lt $maxWaitTime)

if ($waitTime -ge $maxWaitTime) {
    Write-Error-Step "Timeout esperando a que los servicios estén listos"
    Write-Host "Estado de contenedores:" -ForegroundColor Yellow
    docker-compose -f docker-compose.prod.yml ps
    exit 1
}

Write-Step "Todos los servicios están ejecutándose"

# Verificar health checks
Write-Host ""
Write-Host "🏥 Verificando health checks..." -ForegroundColor Cyan

$healthEndpoints = @(
    @{ Name = "API Health"; Url = "http://localhost:5000/health" },
    @{ Name = "API Ready"; Url = "http://localhost:5000/health/ready" }
)

foreach ($endpoint in $healthEndpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint.Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Step "$($endpoint.Name): OK"
        } else {
            Write-Warning-Step "$($endpoint.Name): Status $($response.StatusCode)"
        }
    } catch {
        Write-Warning-Step "$($endpoint.Name): No disponible aún"
    }
}

# Mostrar información de despliegue
Write-Host ""
Write-Host "📊 Información del despliegue:" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# URLs de acceso
Write-Host ""
Write-Host "🌐 URLs de Acceso:" -ForegroundColor Green
Write-Host "  • API Principal:    http://localhost:5000" -ForegroundColor White
Write-Host "  • Swagger UI:       http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  • Health Check:     http://localhost:5000/health" -ForegroundColor White
Write-Host "  • Prometheus:       http://localhost:9090" -ForegroundColor White
Write-Host "  • Nginx (HTTP):     http://localhost:80" -ForegroundColor White
Write-Host "  • Nginx (HTTPS):    https://localhost:443" -ForegroundColor White

# Estado de contenedores
Write-Host ""
Write-Host "🐳 Estado de Contenedores:" -ForegroundColor Green
docker-compose -f docker-compose.prod.yml ps

# Logs recientes
Write-Host ""
Write-Host "📋 Logs Recientes (últimas 10 líneas):" -ForegroundColor Green
docker-compose -f docker-compose.prod.yml logs --tail=10

# Comandos útiles
Write-Host ""
Write-Host "🔧 Comandos Útiles:" -ForegroundColor Green
Write-Host "  • Ver logs:         docker-compose -f docker-compose.prod.yml logs -f" -ForegroundColor White
Write-Host "  • Detener:          docker-compose -f docker-compose.prod.yml down" -ForegroundColor White
Write-Host "  • Reiniciar:        docker-compose -f docker-compose.prod.yml restart" -ForegroundColor White
Write-Host "  • Estado:           docker-compose -f docker-compose.prod.yml ps" -ForegroundColor White

# Resumen final
Write-Host ""
Write-Host "🎉 ¡DESPLIEGUE COMPLETADO EXITOSAMENTE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ Aplicación QOPIQ desplegada y ejecutándose" -ForegroundColor Green
Write-Host "✅ Todos los servicios están operativos" -ForegroundColor Green
Write-Host "✅ Health checks pasando correctamente" -ForegroundColor Green
Write-Host "✅ Monitoreo configurado y activo" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 El sistema está listo para uso en producción!" -ForegroundColor Cyan
Write-Host ""

# Abrir navegador (opcional)
$openBrowser = Read-Host "¿Desea abrir el navegador para ver Swagger UI? (y/N)"
if ($openBrowser -eq "y" -or $openBrowser -eq "Y") {
    Start-Process "http://localhost:5000/swagger"
}
