#requires -Version 5.1
<#
.SYNOPSIS
    Script de rollback para MonitorImpresoras en Windows Server + IIS

.DESCRIPTION
    Este script permite hacer rollback a una versión anterior de la aplicación
    MonitorImpresoras desplegada en IIS.

.PARAMETER BackupPath
    Ruta completa al backup que se quiere restaurar

.PARAMETER DestinationPath
    Ruta en el servidor IIS donde está desplegada la aplicación (ej: C:\inetpub\MonitorImpresoras)

.PARAMETER AppPoolName
    Nombre del Application Pool en IIS (por defecto: MonitorImpresorasPool)

.PARAMETER SiteName
    Nombre del sitio web en IIS (por defecto: MonitorImpresoras)

.PARAMETER Force
    Forzar el rollback sin confirmación

.EXAMPLE
    .\Rollback-MonitorImpresoras.ps1 -BackupPath "C:\Backups\MonitorImpresoras-20250128-143000"

.EXAMPLE
    .\Rollback-MonitorImpresoras.ps1 -BackupPath "C:\Backups\MonitorImpresoras-20250128-143000" -Force
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$BackupPath,

    [string]$DestinationPath = "C:\inetpub\MonitorImpresoras",
    [string]$AppPoolName = "MonitorImpresorasPool",
    [string]$SiteName = "MonitorImpresoras",
    [switch]$Force
)

# Importar módulos necesarios
Import-Module WebAdministration -ErrorAction Stop

# Configuración de logging
$LogFile = Join-Path $env:TEMP "MonitorImpresoras-Rollback-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
Start-Transcript -Path $LogFile -Append

Write-Host "🔄 Iniciando rollback de MonitorImpresoras..." -ForegroundColor Yellow

try {
    # 1. Verificaciones iniciales
    if (-not (Test-Path $BackupPath)) {
        throw "El directorio de backup no existe: $BackupPath"
    }

    if (-not (Test-Path $DestinationPath)) {
        throw "El directorio de destino no existe: $DestinationPath"
    }

    # 2. Confirmación de rollback (si no se fuerza)
    if (-not $Force) {
        $confirmation = Read-Host "⚠️ Esto reemplazará la versión actual con la del backup. ¿Continuar? (s/N)"
        if ($confirmation -ne "s" -and $confirmation -ne "S") {
            Write-Host "❌ Rollback cancelado por el usuario" -ForegroundColor Yellow
            return
        }
    }

    Write-Host "✅ Verificaciones iniciales completadas" -ForegroundColor Green

    # 3. Crear backup de la versión actual (por seguridad)
    $currentBackupPath = Join-Path $DestinationPath.Parent "Current-Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Host "📦 Creando backup de la versión actual en: $currentBackupPath" -ForegroundColor Yellow

    if (Test-Path $DestinationPath) {
        Copy-Item $DestinationPath $currentBackupPath -Recurse -Force
        Write-Host "✅ Backup de versión actual creado" -ForegroundColor Green
    }

    # 4. Detener el sitio web
    $existingSite = Get-IISSite | Where-Object { $_.Name -eq $SiteName }
    if ($existingSite) {
        Write-Host "⏹️ Deteniendo sitio web: $SiteName" -ForegroundColor Yellow
        Stop-IISSite -Name $SiteName -Confirm:$false

        # Esperar a que se detenga completamente
        do {
            Start-Sleep -Seconds 2
            $siteState = Get-IISSiteState -Name $SiteName
        } while ($siteState.Value -ne "Stopped")

        Write-Host "✅ Sitio web detenido" -ForegroundColor Green
    }

    # 5. Eliminar versión actual
    Write-Host "🗑️ Eliminando versión actual..." -ForegroundColor Yellow
    if (Test-Path $DestinationPath) {
        Remove-Item $DestinationPath -Recurse -Force
    }

    # 6. Restaurar desde backup
    Write-Host "📋 Restaurando desde backup..." -ForegroundColor Yellow

    $robocopyArgs = @(
        $BackupPath,
        $DestinationPath,
        "/COPY:DAT",   # Copia datos, atributos y timestamps
        "/DCOPY:T",    # Copia timestamps de directorios
        "/R:3",        # Reintentos: 3
        "/W:5",        # Espera entre reintentos: 5 segundos
        "/LOG+:$LogFile",
        "/NP",         # Sin progreso
        "/NFL",        # Sin nombres de archivo en log
        "/NDL"         # Sin nombres de directorio en log
    )

    $result = & robocopy $robocopyArgs

    if ($LASTEXITCODE -ge 8) {
        throw "Error durante la restauración desde backup. Código de salida: $LASTEXITCODE"
    }

    Write-Host "✅ Restauración completada exitosamente" -ForegroundColor Green

    # 7. Configurar permisos NTFS (por seguridad)
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

    # 8. Iniciar el sitio web
    Write-Host "▶️ Iniciando sitio web: $SiteName" -ForegroundColor Yellow
    Start-IISSite -Name $SiteName -Confirm:$false

    # Esperar a que se inicie completamente
    do {
        Start-Sleep -Seconds 3
        $siteState = Get-IISSiteState -Name $SiteName
    } while ($siteState.Value -ne "Started")

    Write-Host "✅ Sitio web iniciado exitosamente" -ForegroundColor Green

    # 9. Health check
    Write-Host "🏥 Realizando health check..." -ForegroundColor Yellow

    $healthCheckUrl = "http://localhost/api/v1/health"
    $maxAttempts = 10
    $attempt = 1

    do {
        try {
            $response = Invoke-WebRequest -Uri $healthCheckUrl -TimeoutSec 10 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ Health check exitoso - Rollback completado" -ForegroundColor Green
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
        Write-Warning "⚠️ Health check fallido después de $maxAttempts intentos. Verifica manualmente el estado del sitio."
    }

    # 10. Información del rollback
    Write-Host "🎉 ¡Rollback completado exitosamente!" -ForegroundColor Green
    Write-Host "📦 Backup actual guardado en: $currentBackupPath" -ForegroundColor Cyan
    Write-Host "📊 Log de rollback guardado en: $LogFile" -ForegroundColor Cyan
    Write-Host "🔙 Versión restaurada desde: $BackupPath" -ForegroundColor Cyan

}
catch {
    Write-Error "❌ Error durante el rollback: $($_.Exception.Message)"
    Write-Host "📊 Consulta el log detallado en: $LogFile" -ForegroundColor Red

    # Si hubo error crítico, intentar restaurar el backup actual
    if (Test-Path $currentBackupPath) {
        Write-Host "🔄 Intentando restaurar versión anterior..." -ForegroundColor Yellow

        if (Test-Path $DestinationPath) {
            Remove-Item $DestinationPath -Recurse -Force -ErrorAction SilentlyContinue
        }

        Copy-Item $currentBackupPath $DestinationPath -Recurse -Force
        Write-Host "✅ Restauración de emergencia completada" -ForegroundColor Green
    }

    throw
}
finally {
    Stop-Transcript
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
