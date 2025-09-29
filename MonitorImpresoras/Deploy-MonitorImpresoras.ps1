#requires -Version 5.1
<#
.SYNOPSIS
    Script de despliegue para MonitorImpresoras en Windows Server + IIS

.DESCRIPTION
    Este script automatiza el despliegue de la aplicación MonitorImpresoras
    en un servidor Windows Server con IIS instalado.

.PARAMETER SourcePath
    Ruta local donde se encuentran los archivos compilados de la aplicación

.PARAMETER DestinationPath
    Ruta en el servidor IIS donde se desplegará la aplicación (ej: C:\inetpub\MonitorImpresoras)

.PARAMETER AppPoolName
    Nombre del Application Pool en IIS (por defecto: MonitorImpresorasPool)

.PARAMETER SiteName
    Nombre del sitio web en IIS (por defecto: MonitorImpresoras)

.PARAMETER BackupBeforeDeploy
    Si se debe crear una copia de seguridad antes del despliegue

.EXAMPLE
    .\Deploy-MonitorImpresoras.ps1 -SourcePath "C:\Build\publish" -DestinationPath "C:\inetpub\MonitorImpresoras"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourcePath,

    [Parameter(Mandatory=$true)]
    [string]$DestinationPath,

    [string]$AppPoolName = "MonitorImpresorasPool",
    [string]$SiteName = "MonitorImpresoras",
    [switch]$BackupBeforeDeploy,

    [string]$ConnectionString = "",
    [switch]$SkipHealthCheck
)

# Importar módulos necesarios
Import-Module WebAdministration -ErrorAction Stop

# Configuración de logging
$LogFile = Join-Path $env:TEMP "MonitorImpresoras-Deploy-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
Start-Transcript -Path $LogFile -Append

Write-Host "🚀 Iniciando despliegue de MonitorImpresoras..." -ForegroundColor Green

try {
    # 1. Verificar que el directorio fuente existe
    if (-not (Test-Path $SourcePath)) {
        throw "El directorio fuente no existe: $SourcePath"
    }

    Write-Host "✅ Directorio fuente verificado: $SourcePath" -ForegroundColor Green

    # 2. Crear copia de seguridad si se solicita
    if ($BackupBeforeDeploy) {
        $BackupPath = Join-Path $DestinationPath.Parent "Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        Write-Host "📦 Creando copia de seguridad en: $BackupPath" -ForegroundColor Yellow

        if (Test-Path $DestinationPath) {
            Copy-Item $DestinationPath $BackupPath -Recurse -Force
            Write-Host "✅ Copia de seguridad creada exitosamente" -ForegroundColor Green
        }
    }

    # 3. Crear directorio de destino si no existe
    if (-not (Test-Path $DestinationPath)) {
        New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
        Write-Host "📁 Directorio de destino creado: $DestinationPath" -ForegroundColor Green
    }

    # 4. Detener el sitio web si existe
    $existingSite = Get-IISSite | Where-Object { $_.Name -eq $SiteName }
    if ($existingSite) {
        Write-Host "⏹️ Deteniendo sitio web existente: $SiteName" -ForegroundColor Yellow
        Stop-IISSite -Name $SiteName -Confirm:$false

        # Esperar a que se detenga completamente
        do {
            Start-Sleep -Seconds 2
            $siteState = Get-IISSiteState -Name $SiteName
        } while ($siteState.Value -ne "Stopped")

        Write-Host "✅ Sitio web detenido" -ForegroundColor Green
    }

    # 5. Copiar archivos de la aplicación
    Write-Host "📋 Copiando archivos de la aplicación..." -ForegroundColor Yellow

    # Usar robocopy para copia eficiente
    $robocopyArgs = @(
        $SourcePath,
        $DestinationPath,
        "/MIR",        # Mirror: copia y elimina archivos obsoletos
        "/COPY:DAT",   # Copia datos, atributos y timestamps
        "/DCOPY:T",    # Copia timestamps de directorios
        "/R:3",        # Reintentos: 3
        "/W:5",        # Espera entre reintentos: 5 segundos
        "/LOG+:$LogFile",
        "/NP",         # Sin progreso (más limpio)
        "/NFL",        # Sin nombres de archivo en log
        "/NDL"         # Sin nombres de directorio en log
    )

    $result = & robocopy $robocopyArgs

    if ($LASTEXITCODE -ge 8) {
        throw "Error durante la copia de archivos. Código de salida: $LASTEXITCODE"
    }

    Write-Host "✅ Archivos copiados exitosamente" -ForegroundColor Green

    # 6. Crear o configurar Application Pool si no existe
    $appPool = Get-IISAppPool | Where-Object { $_.Name -eq $AppPoolName }
    if (-not $appPool) {
        Write-Host "🏊 Creando Application Pool: $AppPoolName" -ForegroundColor Yellow
        New-IISAppPool -Name $AppPoolName
        Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
        Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "managedPipelineMode" -Value 0
        Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value 2 # NetworkService
        Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"
        Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.requests" -Value 0
        Write-Host "✅ Application Pool creado y configurado" -ForegroundColor Green
    }

    # 7. Crear o configurar sitio web
    if ($existingSite) {
        Write-Host "🌐 Actualizando sitio web existente: $SiteName" -ForegroundColor Yellow
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name "applicationPool" -Value $AppPoolName
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name "physicalPath" -Value $DestinationPath
    } else {
        Write-Host "🌐 Creando nuevo sitio web: $SiteName" -ForegroundColor Yellow
        New-IISSite -Name $SiteName -PhysicalPath $DestinationPath -BindingInformation "*:80:" -ApplicationPool $AppPoolName
    }

    # 8. Configurar permisos NTFS
    Write-Host "🔐 Configurando permisos NTFS..." -ForegroundColor Yellow

    $acl = Get-Acl $DestinationPath
    $networkServiceRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "NETWORK SERVICE",
        "FullControl",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )
    $iisIusrsRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "IIS_IUSRS",
        "ReadAndExecute",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )

    $acl.SetAccessRule($networkServiceRule)
    $acl.SetAccessRule($iisIusrsRule)
    Set-Acl $DestinationPath $acl

    Write-Host "✅ Permisos NTFS configurados" -ForegroundColor Green

    # 9. Ejecutar migraciones de base de datos si hay conexión
    if ($ConnectionString -and (Test-Path (Join-Path $DestinationPath "*.dll"))) {
        Write-Host "🗄️ Ejecutando migraciones de base de datos..." -ForegroundColor Yellow

        $migrationPath = Join-Path $DestinationPath "dotnet-ef-database-update.ps1"
        $migrationScript = @"
cd '$DestinationPath'
dotnet ef database update --context ApplicationDbContext --connection-string '$ConnectionString'
"@

        $migrationScript | Out-File $migrationPath -Encoding UTF8

        try {
            & $migrationPath
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Migraciones ejecutadas exitosamente" -ForegroundColor Green
            } else {
                Write-Warning "⚠️ Migraciones completadas con warnings (código: $LASTEXITCODE)"
            }
        }
        catch {
            Write-Warning "⚠️ Error durante las migraciones: $($_.Exception.Message)"
        }
        finally {
            Remove-Item $migrationPath -ErrorAction SilentlyContinue
        }
    }

    # 10. Iniciar el sitio web
    Write-Host "▶️ Iniciando sitio web: $SiteName" -ForegroundColor Yellow
    Start-IISSite -Name $SiteName -Confirm:$false

    # Esperar a que se inicie completamente
    do {
        Start-Sleep -Seconds 3
        $siteState = Get-IISSiteState -Name $SiteName
    } while ($siteState.Value -ne "Started")

    Write-Host "✅ Sitio web iniciado exitosamente" -ForegroundColor Green

    # 11. Health check si no se omite
    if (-not $SkipHealthCheck) {
        Write-Host "🏥 Realizando health check..." -ForegroundColor Yellow

        $healthCheckUrl = "http://localhost/api/v1/health"
        $maxAttempts = 10
        $attempt = 1

        do {
            try {
                $response = Invoke-WebRequest -Uri $healthCheckUrl -TimeoutSec 10 -ErrorAction Stop
                if ($response.StatusCode -eq 200) {
                    Write-Host "✅ Health check exitoso" -ForegroundColor Green
                    break
                }
            }
            catch {
                Write-Host "⏳ Intento $attempt de $maxAttempts - Health check fallido, reintentando..." -ForegroundColor Yellow
                Start-Sleep -Seconds 5
            }
            $attempt++
        } while ($attempt -le $maxAttempts)

        if ($attempt -gt $maxAttempts) {
            throw "Health check fallido después de $maxAttempts intentos"
        }
    }

    Write-Host "🎉 ¡Despliegue completado exitosamente!" -ForegroundColor Green
    Write-Host "📊 Log de despliegue guardado en: $LogFile" -ForegroundColor Cyan

}
catch {
    Write-Error "❌ Error durante el despliegue: $($_.Exception.Message)"
    Write-Host "📊 Consulta el log detallado en: $LogFile" -ForegroundColor Red

    # Si hubo error, intentar restaurar desde backup si existe
    if ($BackupBeforeDeploy -and (Test-Path $BackupPath)) {
        Write-Host "🔄 Restaurando desde copia de seguridad..." -ForegroundColor Yellow

        if (Test-Path $DestinationPath) {
            Remove-Item $DestinationPath -Recurse -Force -ErrorAction SilentlyContinue
        }

        Copy-Item $BackupPath $DestinationPath -Recurse -Force
        Write-Host "✅ Restauración completada" -ForegroundColor Green
    }

    throw
}
finally {
    Stop-Transcript
}

# Función auxiliar para verificar si IIS está instalado
function Test-IISInstalled {
    return (Get-Service W3SVC -ErrorAction SilentlyContinue) -ne $null
}

# Función auxiliar para verificar permisos de administrador
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Verificaciones iniciales
if (-not (Test-Administrator)) {
    throw "Este script debe ejecutarse como Administrador"
}

if (-not (Test-IISInstalled)) {
    throw "IIS no está instalado en este servidor"
}
