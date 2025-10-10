# Script de Despliegue QOPIQ - PowerShell
# Versión: 1.0.0
# Fecha: $(Get-Date -Format "yyyy-MM-dd")

<#
DESCRIPCIÓN:
Este script automatiza el proceso de despliegue del sistema QOPIQ en un entorno de producción.
Incluye validación de requisitos, instalación de dependencias, configuración y puesta en marcha.

REQUISITOS:
- Windows 10/11 o Windows Server 2016+
- PowerShell 5.1 o superior
- Docker Desktop (para despliegue en contenedores)
- Acceso de administrador
#>

# Configuración
$ErrorActionPreference = "Stop"
$ProgressPreference = 'SilentlyContinue'

# Constantes
$APP_NAME = "QOPIQ"
$VERSION = "1.0.0"
$DOCKER_COMPOSE_FILE = "docker-compose.prod.yml"
$ENV_FILE = ".env.production"
$BACKUP_DIR = "backups\$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# Funciones auxiliares
function Write-Header {
    param([string]$Title)
    Write-Host "`n===== $Title =====" -ForegroundColor Cyan
}

function Test-CommandExists {
    param([string]$command)
    return (Get-Command $command -ErrorAction SilentlyContinue) -ne $null
}

function Test-Admin {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Backup-ExistingInstallation {
    Write-Header "REALIZANDO COPIA DE SEGURIDAD"
    
    if (Test-Path $BACKUP_DIR) {
        Remove-Item -Path $BACKUP_DIR -Recurse -Force
    }
    
    New-Item -ItemType Directory -Path $BACKUP_DIR | Out-Null
    
    $backupItems = @(
        "appsettings.*.json",
        ".env*",
        "docker-compose.*.yml",
        "certificados"
    )
    
    foreach ($item in $backupItems) {
        if (Test-Path $item) {
            Copy-Item -Path $item -Destination $BACKUP_DIR -Recurse -Force
        }
    }
    
    Write-Host "✅ Copia de seguridad completada en: $BACKUP_DIR" -ForegroundColor Green
}

function Install-Dependencies {
    Write-Header "VERIFICANDO DEPENDENCIAS"
    
    # Verificar Docker
    if (-not (Test-CommandExists "docker")) {
        Write-Host "❌ Docker no está instalado. Por favor instale Docker Desktop desde: https://www.docker.com/products/docker-desktop/" -ForegroundColor Red
        exit 1
    }
    
    # Verificar Docker Compose
    if (-not (Test-CommandExists "docker-compose")) {
        Write-Host "❌ Docker Compose no está instalado. Por favor instale la última versión de Docker Desktop." -ForegroundColor Red
        exit 1
    }
    
    # Verificar versión de Docker
    $dockerVersion = docker --version | Select-String -Pattern "(\d+\.\d+\.\d+)" | ForEach-Object { $_.Matches.Groups[1].Value }
    Write-Host "✅ Docker versión: $dockerVersion" -ForegroundColor Green
    
    # Verificar que Docker esté en ejecución
    try {
        docker info | Out-Null
        Write-Host "✅ Docker está en ejecución" -ForegroundColor Green
    } catch {
        Write-Host "❌ Docker no está en ejecución. Por favor inicie Docker Desktop." -ForegroundColor Red
        exit 1
    }
}

function Initialize-Environment {
    Write-Header "INICIALIZANDO ENTORNO"
    
    # Crear archivo .env si no existe
    if (-not (Test-Path $ENV_FILE)) {
        Write-Host "Creando archivo de configuración $ENV_FILE..."
        @"
# Configuración de la base de datos
POSTGRES_DB=qopiq
POSTGRES_USER=postgres
POSTGRES_PASSWORD=TuContraseñaSegura123!

# Configuración de la aplicación
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80

# Configuración JWT
JWT__Key=TuClaveSecretaMuySegura1234567890
JWT__Issuer=QOPIQ-API
JWT__Audience=QOPIQ-Client
JWT__ExpireDays=7

# Configuración de red
DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLE_HTTP2UNENCRYPTEDSUPPORT=1
"@ | Out-File -FilePath $ENV_FILE -Encoding utf8
        
        Write-Host "⚠️  Se ha creado el archivo $ENV_FILE con valores por defecto. Por favor, revíselo y configure los valores adecuados." -ForegroundColor Yellow
    } else {
        Write-Host "✅ Archivo de configuración $ENV_FILE encontrado" -ForegroundColor Green
    }
    
    # Cargar variables de entorno
    Get-Content $ENV_FILE | ForEach-Object {
        $name, $value = $_.Split('=', 2)
        if ($name -and $value) {
            [System.Environment]::SetEnvironmentVariable($name.Trim(), $value.Trim())
        }
    }
}

function Deploy-Application {
    Write-Header "DESPLEGANDO APLICACIÓN"
    
    # Detener y eliminar contenedores existentes
    Write-Host "Deteniendo contenedores existentes..."
    docker-compose -f $DOCKER_COMPOSE_FILE down --remove-orphans
    
    # Reconstruir y levantar los contenedores
    Write-Host "Iniciando los servicios..."
    docker-compose -f $DOCKER_COMPOSE_FILE up -d --build
    
    # Verificar que los contenedores estén en ejecución
    $containers = docker ps --format '{{.Names}} {{.Status}}' | Select-String "qopiq"
    if ($containers) {
        Write-Host "`n✅ Contenedores en ejecución:" -ForegroundColor Green
        $containers | ForEach-Object { Write-Host "- $_" }
    } else {
        Write-Host "❌ No se encontraron contenedores en ejecución" -ForegroundColor Red
        exit 1
    }
}

function Test-Application {
    Write-Header "VERIFICANDO LA APLICACIÓN"
    
    # Esperar a que la aplicación esté lista
    $maxAttempts = 30
    $attempt = 0
    $isReady = $false
    
    Write-Host "Esperando a que la aplicación esté lista..."
    
    while ($attempt -lt $maxAttempts -and -not $isReady) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                $isReady = $true
                $health = $response.Content | ConvertFrom-Json
                Write-Host "✅ Aplicación en línea y saludable:" -ForegroundColor Green
                $health | Format-Table -AutoSize | Out-String | ForEach-Object { Write-Host $_ }
                break
            }
        } catch {
            Write-Host "." -NoNewline -ForegroundColor Gray
            Start-Sleep -Seconds 2
            $attempt++
        }
    }
    
    if (-not $isReady) {
        Write-Host "`n❌ La aplicación no responde correctamente después de $($maxAttempts * 2) segundos" -ForegroundColor Red
        Write-Host "Revisa los logs con: docker-compose logs" -ForegroundColor Yellow
        exit 1
    }
}

function Show-Summary {
    Write-Header "RESUMEN DEL DESPLIEGUE"
    
    $summary = @{
        'Aplicación' = $APP_NAME
        'Versión' = $VERSION
        'Fecha' = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        'URL de Acceso' = 'http://localhost:5000'
        'Estado' = '✅ Despliegue exitoso'
        'Contenedores' = (docker ps --format '{{.Names}}' | Select-String 'qopiq' | Measure-Object).Count
    }
    
    $summary.GetEnumerator() | Format-Table @{Label='Parámetro';Expression={$_.Key}}, @{Label='Valor';Expression={$_.Value}} -AutoSize
    
    Write-Host "`n📋 Comandos útiles:" -ForegroundColor Cyan
    Write-Host "- Ver logs: docker-compose logs -f"
    Write-Host "- Detener la aplicación: docker-compose down"
    Write-Host "- Actualizar la aplicación: git pull && .\deploy.ps1"
    
    Write-Host "`n🎉 ¡QOPIQ se ha desplegado correctamente!" -ForegroundColor Green
}

# Punto de entrada principal
try {
    # Verificar permisos de administrador
    if (-not (Test-Admin)) {
        Write-Host "Este script requiere privilegios de administrador. Por favor, ejecútelo como administrador." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "`n🚀 INICIANDO DESPLIEGUE DE $APP_NAME v$VERSION`n" -ForegroundColor Cyan
    
    # Ejecutar pasos de despliegue
    Install-Dependencies
    Initialize-Environment
    Backup-ExistingInstallation
    Deploy-Application
    Test-Application
    Show-Summary
    
} catch {
    Write-Host "`n❌ ERROR durante el despliegue: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
}
