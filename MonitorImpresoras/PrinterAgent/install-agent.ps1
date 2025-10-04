# Script de instalación del PrinterAgent
# Ejecutar como administrador

param(
    [Parameter(Mandatory=$true)]
    [string]$CentralApiUrl,
    
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [Parameter(Mandatory=$false)]
    [string]$AgentId = "agent-$(Get-Random -Maximum 9999)",
    
    [Parameter(Mandatory=$false)]
    [string]$AgentName = "PrinterAgent-$env:COMPUTERNAME",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "$env:COMPUTERNAME",
    
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\PrinterAgent",
    
    [Parameter(Mandatory=$false)]
    [string[]]$NetworkRanges = @("192.168.1.0/24")
)

Write-Host "=== Instalador de PrinterAgent ===" -ForegroundColor Green
Write-Host "Configuración:"
Write-Host "  Agent ID: $AgentId"
Write-Host "  Agent Name: $AgentName"
Write-Host "  Location: $Location"
Write-Host "  Central API: $CentralApiUrl"
Write-Host "  Install Path: $InstallPath"
Write-Host ""

# Verificar permisos de administrador
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Error "Este script debe ejecutarse como administrador"
    exit 1
}

# Verificar .NET 8.0
Write-Host "Verificando .NET 8.0..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -notmatch "^8\.") {
        Write-Error ".NET 8.0 no está instalado. Por favor instale .NET 8.0 Runtime"
        exit 1
    }
    Write-Host "✓ .NET 8.0 encontrado: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error ".NET no está instalado. Por favor instale .NET 8.0 Runtime"
    exit 1
}

# Crear directorio de instalación
Write-Host "Creando directorio de instalación..." -ForegroundColor Yellow
if (Test-Path $InstallPath) {
    Write-Host "Directorio ya existe, limpiando..." -ForegroundColor Yellow
    Remove-Item "$InstallPath\*" -Recurse -Force -ErrorAction SilentlyContinue
} else {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

# Compilar y publicar el agente
Write-Host "Compilando PrinterAgent..." -ForegroundColor Yellow
$currentPath = Get-Location
try {
    Set-Location "$PSScriptRoot\PrinterAgent.API"
    
    # Compilar
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "Error compilando el proyecto"
    }
    
    # Publicar
    dotnet publish --configuration Release --output $InstallPath --self-contained false
    if ($LASTEXITCODE -ne 0) {
        throw "Error publicando el proyecto"
    }
    
    Write-Host "✓ Compilación exitosa" -ForegroundColor Green
} catch {
    Write-Error "Error durante la compilación: $_"
    exit 1
} finally {
    Set-Location $currentPath
}

# Crear configuración personalizada
Write-Host "Configurando agente..." -ForegroundColor Yellow
$configPath = "$InstallPath\appsettings.Production.json"
$config = @{
    "Logging" = @{
        "LogLevel" = @{
            "Default" = "Information"
            "Microsoft.AspNetCore" = "Warning"
            "PrinterAgent" = "Information"
        }
    }
    "AllowedHosts" = "*"
    "Agent" = @{
        "AgentId" = $AgentId
        "AgentName" = $AgentName
        "Location" = $Location
        "CentralApiUrl" = $CentralApiUrl
        "ApiKey" = $ApiKey
        "ReportingInterval" = "00:05:00"
        "HealthCheckInterval" = "00:01:00"
        "Network" = @{
            "ScanRanges" = $NetworkRanges
            "SnmpCommunity" = "public"
            "SnmpTimeout" = 5000
            "MaxConcurrentScans" = 10
            "EnableAutoDiscovery" = $true
        }
        "Logging" = @{
            "Level" = "Information"
            "RetentionDays" = 30
            "EnableFileLogging" = $true
            "LogPath" = "logs"
        }
    }
}

$config | ConvertTo-Json -Depth 10 | Out-File -FilePath $configPath -Encoding UTF8
Write-Host "✓ Configuración creada en $configPath" -ForegroundColor Green

# Crear directorio de logs
New-Item -ItemType Directory -Path "$InstallPath\logs" -Force | Out-Null

# Crear servicio de Windows
Write-Host "Instalando servicio de Windows..." -ForegroundColor Yellow
$serviceName = "PrinterAgent"
$serviceDisplayName = "Printer Agent - $AgentName"
$serviceDescription = "Agente distribuido para monitoreo de impresoras"
$executablePath = "$InstallPath\PrinterAgent.API.exe"

# Detener y eliminar servicio existente si existe
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Deteniendo servicio existente..." -ForegroundColor Yellow
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    
    Write-Host "Eliminando servicio existente..." -ForegroundColor Yellow
    sc.exe delete $serviceName | Out-Null
    Start-Sleep -Seconds 2
}

# Crear nuevo servicio
Write-Host "Creando servicio..." -ForegroundColor Yellow
$serviceParams = @{
    Name = $serviceName
    BinaryPathName = "`"$executablePath`" --environment=Production"
    DisplayName = $serviceDisplayName
    Description = $serviceDescription
    StartupType = "Automatic"
}

New-Service @serviceParams | Out-Null
Write-Host "✓ Servicio '$serviceName' creado" -ForegroundColor Green

# Configurar recuperación del servicio
Write-Host "Configurando recuperación del servicio..." -ForegroundColor Yellow
sc.exe failure $serviceName reset= 86400 actions= restart/5000/restart/10000/restart/30000 | Out-Null

# Crear script de inicio manual
$startScript = @"
@echo off
echo Iniciando PrinterAgent...
cd /d "$InstallPath"
dotnet PrinterAgent.API.dll --environment=Production
pause
"@

$startScript | Out-File -FilePath "$InstallPath\start-agent.bat" -Encoding ASCII

# Crear script de configuración
$configScript = @"
@echo off
echo Abriendo configuración de PrinterAgent...
notepad "$configPath"
"@

$configScript | Out-File -FilePath "$InstallPath\configure-agent.bat" -Encoding ASCII

# Iniciar servicio
Write-Host "Iniciando servicio..." -ForegroundColor Yellow
try {
    Start-Service -Name $serviceName
    Start-Sleep -Seconds 3
    
    $service = Get-Service -Name $serviceName
    if ($service.Status -eq "Running") {
        Write-Host "✓ Servicio iniciado exitosamente" -ForegroundColor Green
    } else {
        Write-Warning "El servicio no se inició correctamente. Estado: $($service.Status)"
    }
} catch {
    Write-Warning "Error iniciando el servicio: $_"
    Write-Host "Puede iniciar el servicio manualmente o usar start-agent.bat" -ForegroundColor Yellow
}

# Configurar firewall
Write-Host "Configurando firewall..." -ForegroundColor Yellow
try {
    New-NetFirewallRule -DisplayName "PrinterAgent API" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow -ErrorAction SilentlyContinue | Out-Null
    Write-Host "✓ Regla de firewall creada" -ForegroundColor Green
} catch {
    Write-Warning "No se pudo configurar el firewall automáticamente"
}

Write-Host ""
Write-Host "=== Instalación Completada ===" -ForegroundColor Green
Write-Host ""
Write-Host "PrinterAgent ha sido instalado exitosamente en: $InstallPath"
Write-Host ""
Write-Host "Información del servicio:"
Write-Host "  Nombre: $serviceName"
Write-Host "  Estado: $($(Get-Service -Name $serviceName).Status)"
Write-Host ""
Write-Host "URLs de acceso:"
Write-Host "  API Local: http://localhost:5000"
Write-Host "  Swagger: http://localhost:5000/swagger"
Write-Host "  Health Check: http://localhost:5000/health"
Write-Host ""
Write-Host "Archivos importantes:"
Write-Host "  Configuración: $configPath"
Write-Host "  Logs: $InstallPath\logs\"
Write-Host "  Inicio manual: $InstallPath\start-agent.bat"
Write-Host "  Configurar: $InstallPath\configure-agent.bat"
Write-Host ""
Write-Host "Comandos útiles:"
Write-Host "  Iniciar servicio: Start-Service $serviceName"
Write-Host "  Detener servicio: Stop-Service $serviceName"
Write-Host "  Ver logs: Get-Content '$InstallPath\logs\agent-*.txt' -Tail 50"
Write-Host ""
Write-Host "Para desinstalar: sc.exe delete $serviceName" -ForegroundColor Yellow
