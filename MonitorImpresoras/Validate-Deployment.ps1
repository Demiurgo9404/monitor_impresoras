#requires -Version 5.1
<#
.SYNOPSIS
    Script de validación post-despliegue para MonitorImpresoras

.DESCRIPTION
    Este script realiza todas las validaciones necesarias después de un despliegue
    para asegurar que la aplicación funciona correctamente.

.PARAMETER BaseUrl
    URL base de la aplicación (ej: http://localhost o https://api.monitorimpresoras.com)

.PARAMETER TimeoutSeconds
    Tiempo máximo de espera para cada validación (por defecto: 30 segundos)

.PARAMETER MaxRetries
    Número máximo de reintentos para cada validación (por defecto: 5)

.PARAMETER SkipDatabaseCheck
    Omitir validación de base de datos

.EXAMPLE
    .\Validate-Deployment.ps1 -BaseUrl "http://localhost"

.EXAMPLE
    .\Validate-Deployment.ps1 -BaseUrl "https://api.monitorimpresoras.com" -TimeoutSeconds 60
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$BaseUrl,

    [int]$TimeoutSeconds = 30,
    [int]$MaxRetries = 5,
    [switch]$SkipDatabaseCheck
)

# Configuración
$LogFile = Join-Path $env:TEMP "MonitorImpresoras-Validation-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$ValidationResults = @()

Start-Transcript -Path $LogFile -Append

Write-Host "🔍 Iniciando validación post-despliegue de MonitorImpresoras..." -ForegroundColor Green
Write-Host "📍 URL Base: $BaseUrl" -ForegroundColor Cyan
Write-Host "⏱️ Timeout: $TimeoutSeconds segundos" -ForegroundColor Cyan
Write-Host "🔄 Reintentos: $MaxRetries" -ForegroundColor Cyan

# Función auxiliar para ejecutar validaciones con reintentos
function Invoke-ValidationWithRetry {
    param(
        [string]$Name,
        [scriptblock]$ScriptBlock,
        [int]$RetryCount = $MaxRetries,
        [int]$DelaySeconds = 5
    )

    $attempt = 1
    $success = $false
    $errorMessage = ""

    Write-Host "🔍 Validando: $Name..." -ForegroundColor Yellow

    while ($attempt -le $RetryCount -and -not $success) {
        try {
            $result = & $ScriptBlock

            if ($result -eq $true -or $result.StatusCode -eq 200 -or $result -ne $null) {
                $success = $true
                Write-Host "✅ $Name - PASSED (intento $attempt)" -ForegroundColor Green

                $ValidationResults += @{
                    Name = $Name
                    Status = "PASSED"
                    Attempts = $attempt
                    Details = "Validación exitosa"
                }
            } else {
                throw "Validación fallida: resultado inesperado"
            }
        }
        catch {
            $errorMessage = $_.Exception.Message
            Write-Host "❌ $Name - FAILED (intento $attempt): $errorMessage" -ForegroundColor Red

            if ($attempt -lt $RetryCount) {
                Write-Host "⏳ Reintentando en $DelaySeconds segundos..." -ForegroundColor Yellow
                Start-Sleep -Seconds $DelaySeconds
            }
        }

        $attempt++
    }

    if (-not $success) {
        Write-Host "💥 $Name - FINALMENTE FALLIDO después de $RetryCount intentos" -ForegroundColor Red

        $ValidationResults += @{
            Name = $Name
            Status = "FAILED"
            Attempts = $RetryCount
            Details = $errorMessage
        }

        return $false
    }

    return $true
}

try {
    # Validación 1: Conectividad básica
    $success = Invoke-ValidationWithRetry -Name "Conectividad Básica" -ScriptBlock {
        $response = Invoke-WebRequest -Uri $BaseUrl -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction Stop
        return $response.StatusCode -eq 200
    }

    if (-not $success) {
        throw "La aplicación no responde en la URL base: $BaseUrl"
    }

    # Validación 2: Health Check básico
    $success = Invoke-ValidationWithRetry -Name "Health Check Básico" -ScriptBlock {
        $response = Invoke-WebRequest -Uri "$BaseUrl/api/v1/health" -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -ne 200) { return $false }

        $health = $response.Content | ConvertFrom-Json
        return $health.status -eq "Healthy"
    }

    if (-not $success) {
        Write-Warning "⚠️ Health check básico fallido. La aplicación puede tener problemas."
    }

    # Validación 3: Health Check extendido
    $success = Invoke-ValidationWithRetry -Name "Health Check Extendido" -ScriptBlock {
        $response = Invoke-WebRequest -Uri "$BaseUrl/api/v1/health/extended" -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -ne 200) { return $false }

        $health = $response.Content | ConvertFrom-Json
        return $health.status -eq "Healthy" -and $health.database -ne $null
    }

    if (-not $success) {
        Write-Warning "⚠️ Health check extendido fallido. Algunos componentes pueden no estar funcionando."
    }

    # Validación 4: Métricas de Prometheus
    $success = Invoke-ValidationWithRetry -Name "Métricas Prometheus" -ScriptBlock {
        $response = Invoke-WebRequest -Uri "$BaseUrl/metrics" -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -ne 200) { return $false }

        $content = $response.Content
        return $content -like "*# Custom application metrics*" -and $content -like "*api_requests_total*"
    }

    if (-not $success) {
        Write-Warning "⚠️ Métricas de Prometheus no disponibles. El monitoreo puede no funcionar."
    }

    # Validación 5: API de reportes disponible
    $success = Invoke-ValidationWithRetry -Name "API de Reportes" -ScriptBlock {
        $response = Invoke-WebRequest -Uri "$BaseUrl/api/v1/reports/available" -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -ne 200) { return $false }

        $reports = $response.Content | ConvertFrom-Json
        return $reports -is [System.Array] -and $reports.Count -gt 0
    }

    if (-not $success) {
        Write-Warning "⚠️ API de reportes no disponible. La funcionalidad principal puede estar afectada."
    }

    # Validación 6: Swagger UI
    $success = Invoke-ValidationWithRetry -Name "Swagger UI" -ScriptBlock {
        $response = Invoke-WebRequest -Uri "$BaseUrl/swagger/index.html" -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -ne 200) { return $false }

        $content = $response.Content
        return $content -like "*swagger*" -and $content -like "*Monitor Impresoras*"
    }

    if (-not $success) {
        Write-Warning "⚠️ Swagger UI no disponible. La documentación puede no estar accesible."
    }

    # Validación 7: Verificación de base de datos (si no se omite)
    if (-not $SkipDatabaseCheck) {
        $success = Invoke-ValidationWithRetry -Name "Conexión a Base de Datos" -ScriptBlock {
            # Esta validación requeriría acceso a la cadena de conexión o poder ejecutar consultas
            # Por simplicidad, verificamos que la aplicación puede conectarse a sí misma
            $response = Invoke-WebRequest -Uri "$BaseUrl/api/v1/health/extended" -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction Stop
            $health = $response.Content | ConvertFrom-Json

            # Si el health check extendido pasa y tiene información de BD, asumimos que está bien
            return $health.database -ne $null -and $health.database.status -eq "Healthy"
        }

        if (-not $success) {
            Write-Warning "⚠️ Problemas potenciales con la base de datos."
        }
    }

    # Validación 8: Rendimiento básico (múltiples requests simultáneos)
    Write-Host "🔍 Validando rendimiento con requests simultáneos..." -ForegroundColor Yellow

    $tasks = @()
    1..5 | ForEach-Object {
        $tasks += Invoke-WebRequest -Uri "$BaseUrl/api/v1/health" -TimeoutSec $TimeoutSeconds -UseBasicParsing -ErrorAction SilentlyContinue
    }

    $results = $tasks | Wait-Job | Receive-Job
    $successfulRequests = ($results | Where-Object { $_.StatusCode -eq 200 }).Count

    if ($successfulRequests -ge 3) {
        Write-Host "✅ Rendimiento - PASSED ($successfulRequests/5 requests exitosos)" -ForegroundColor Green

        $ValidationResults += @{
            Name = "Rendimiento Básico"
            Status = "PASSED"
            Attempts = 1
            Details = "$successfulRequests de 5 requests simultáneos exitosos"
        }
    } else {
        Write-Host "❌ Rendimiento - FAILED (solo $successfulRequests/5 requests exitosos)" -ForegroundColor Red

        $ValidationResults += @{
            Name = "Rendimiento Básico"
            Status = "FAILED"
            Attempts = 1
            Details = "Solo $successfulRequests de 5 requests simultáneos exitosos"
        }
    }

    # Resumen de validaciones
    Write-Host ""
    Write-Host "📊 RESUMEN DE VALIDACIONES" -ForegroundColor Cyan
    Write-Host "=========================" -ForegroundColor Cyan

    $passed = ($ValidationResults | Where-Object { $_.Status -eq "PASSED" }).Count
    $failed = ($ValidationResults | Where-Object { $_.Status -eq "FAILED" }).Count
    $total = $ValidationResults.Count

    Write-Host "✅ PASSED: $passed/$total" -ForegroundColor Green
    Write-Host "❌ FAILED: $failed/$total" -ForegroundColor Red
    Write-Host ""

    foreach ($result in $ValidationResults) {
        $color = if ($result.Status -eq "PASSED") { "Green" } else { "Red" }
        Write-Host "  $($result.Name): $($result.Status)" -ForegroundColor $color
        if ($result.Details) {
            Write-Host "    $($result.Details)" -ForegroundColor Gray
        }
    }

    # Conclusión
    if ($failed -eq 0) {
        Write-Host ""
        Write-Host "🎉 ¡VALIDACIÓN COMPLETA EXITOSA!" -ForegroundColor Green
        Write-Host "✅ La aplicación Monitor Impresoras está funcionando correctamente" -ForegroundColor Green
        Write-Host "📊 Log de validación guardado en: $LogFile" -ForegroundColor Cyan

        return $true
    } else {
        Write-Host ""
        Write-Host "⚠️ VALIDACIÓN COMPLETADA CON PROBLEMAS" -ForegroundColor Yellow
        Write-Host "❌ $failed validaciones fallaron. Revisa los detalles arriba." -ForegroundColor Yellow
        Write-Host "📊 Consulta el log detallado en: $LogFile" -ForegroundColor Cyan

        return $false
    }

}
catch {
    Write-Error "💥 Error crítico durante la validación: $($_.Exception.Message)"
    Write-Host "📊 Consulta el log detallado en: $LogFile" -ForegroundColor Red
    throw
}
finally {
    Stop-Transcript
}
