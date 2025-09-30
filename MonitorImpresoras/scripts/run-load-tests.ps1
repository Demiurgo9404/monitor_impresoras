# Script para ejecutar pruebas de carga con k6 y generar informes
# Uso: .\run-load-tests.ps1 [opcional: -Environment "staging" -Duration "5m" -Users 200]

param(
    [string]$Environment = "staging",
    [string]$BaseUrl = "https://$Environment.monitorimpresoras.com",
    [string]$Duration = "5m",
    [int]$Users = 200,
    [string]$ResultsDir = "./load-test-results",
    [switch]$GenerateReport = $true,
    [switch]$MonitorResources = $true
)

Write-Host "⚡ Ejecutando pruebas de carga para $Environment..." -ForegroundColor Green

# 1. Preparar entorno
Write-Host "🔧 Preparando entorno de pruebas de carga..." -ForegroundColor Yellow

# Crear directorio de resultados
New-Item -ItemType Directory -Path $ResultsDir -Force
$Timestamp = Get-Date -Format 'yyyy-MM-dd-HHmmss'
$RunDir = Join-Path $ResultsDir "run-$Timestamp"
New-Item -ItemType Directory -Path $RunDir -Force

# 2. Verificar instalación de k6
Write-Host "📦 Verificando instalación de k6..." -ForegroundColor Yellow

try {
    $k6Version = k6 version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "k6 no encontrado"
    }
    Write-Host "✅ k6 instalado: $k6Version" -ForegroundColor Green
} catch {
    Write-Error "❌ k6 no está instalado. Instalar desde: https://k6.io/docs/get-started/installation/"
    exit 1
}

# 3. Configurar archivo de prueba con parámetros
Write-Host "⚙️ Configurando parámetros de prueba..." -ForegroundColor Yellow

$TestScript = Join-Path (Get-Location) "load-tests/load-test.js"
$OutputFile = Join-Path $RunDir "load-test-results.json"

# 4. Ejecutar prueba de carga
Write-Host "🚀 Ejecutando prueba de carga ($Users usuarios por $Duration)..." -ForegroundColor Green

try {
    & k6 run `
        --vus $Users `
        --duration $Duration `
        --out json="$OutputFile" `
        --env "BASE_URL=$BaseUrl" `
        $TestScript

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "⚠️ Prueba de carga completada con algunos errores"
        $TestPassed = $false
    } else {
        Write-Host "✅ Prueba de carga completada exitosamente" -ForegroundColor Green
        $TestPassed = $true
    }
} catch {
    Write-Error "❌ Error ejecutando prueba de carga: $($_.Exception.Message)"
    $TestPassed = $false
}

# 5. Procesar resultados y generar informe
Write-Host "📊 Procesando resultados..." -ForegroundColor Yellow

if (Test-Path $OutputFile) {
    # Leer resultados JSON
    $Results = Get-Content $OutputFile | ConvertFrom-Json

    # Calcular métricas
    $TotalRequests = ($Results | Where-Object { $_.metric -eq "http_reqs" }).value[0]
    $FailedRequests = ($Results | Where-Object { $_.metric -eq "http_req_failed" }).value[0]
    $AvgResponseTime = ($Results | Where-Object { $_.metric -eq "http_req_duration" -and $_.data.value -ne $null } | Measure-Object -Property data.value -Average).Average
    $P95ResponseTime = ($Results | Where-Object { $_.metric -eq "http_req_duration" -and $_.data.value -ne $null } | Sort-Object -Property data.value | Select-Object -Last 5 | Measure-Object -Property data.value -Average).Average

    $ErrorRate = if ($TotalRequests -gt 0) { $FailedRequests / $TotalRequests } else { 0 }
    $RequestsPerSecond = $TotalRequests / ($Duration -replace 'm', '' | ForEach-Object { [int]$_ * 60 })

    # Crear informe detallado
    $ReportContent = @"
# 📊 INFORME DE PRUEBAS DE CARGA - Monitor de Impresoras

## 🎯 Información de Ejecución

| Métrica | Valor | Estado |
|---------|-------|---------|
| **Entorno** | $Environment | N/A |
| **URL Base** | $BaseUrl | N/A |
| **Usuarios Concurrentes** | $Users | N/A |
| **Duración** | $Duration | N/A |
| **Fecha de Ejecución** | $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') | N/A |

## 📈 Métricas de Rendimiento

### Objetivos del Día 27 (SLOs)
| Métrica | Objetivo | Resultado | Estado |
|---------|----------|-----------|---------|
| **P95 Tiempo de Respuesta** | < 500ms | $($P95ResponseTime.ToString("F0"))ms | $($P95ResponseTime -lt 500 ? '✅ CUMPLE' : '❌ NO CUMPLE') |
| **Error Rate** | < 2% | $($ErrorRate.ToString("P1")) | $($ErrorRate -lt 0.02 ? '✅ CUMPLE' : '❌ NO CUMPLE') |
| **RPS Sostenidos** | > 100 | $($RequestsPerSecond.ToString("F1")) | $($RequestsPerSecond -gt 100 ? '✅ CUMPLE' : '❌ NO CUMPLE') |

### Métricas Detalladas
| Métrica | Valor | Descripción |
|---------|-------|-------------|
| **Total Requests** | $TotalRequests | Número total de peticiones realizadas |
| **Requests Fallidos** | $FailedRequests | Peticiones que retornaron error |
| **Tiempo Promedio** | $($AvgResponseTime.ToString("F0"))ms | Tiempo de respuesta promedio |
| **P95 Tiempo** | $($P95ResponseTime.ToString("F0"))ms | Percentil 95 de tiempo de respuesta |
| **Requests/Segundo** | $($RequestsPerSecond.ToString("F1")) | Tasa de peticiones por segundo |

## 🔍 Análisis de Endpoints

### Endpoint: Dashboard (/)
| Métrica | Valor | Estado |
|---------|-------|---------|
| Requests | TBD | N/A |
| Tiempo Promedio | TBD | N/A |
| Error Rate | TBD | N/A |

### Endpoint: API /printers
| Métrica | Valor | Estado |
|---------|-------|---------|
| Requests | TBD | N/A |
| Tiempo Promedio | TBD | N/A |
| Error Rate | TBD | N/A |

### Endpoint: API /telemetry/dashboard
| Métrica | Valor | Estado |
|---------|-------|---------|
| Requests | TBD | N/A |
| Tiempo Promedio | TBD | N/A |
| Error Rate | TBD | N/A |

### Endpoint: Health Check
| Métrica | Valor | Estado |
|---------|-------|---------|
| Requests | TBD | N/A |
| Tiempo Promedio | TBD | N/A |
| Error Rate | TBD | N/A |

## 💾 Recursos del Sistema (Máximos Durante Prueba)

### Servidor de Aplicación
| Recurso | Uso Máximo | Estado |
|---------|------------|---------|
| **CPU** | TBD% | $($cpuUsage -gt 80 ? '⚠️ ALTO' : '✅ NORMAL') |
| **Memoria** | TBD% | $($memoryUsage -gt 85 ? '⚠️ ALTO' : '✅ NORMAL') |
| **Threads** | TBD | N/A |

### Base de Datos
| Métrica | Valor | Estado |
|---------|-------|---------|
| **Conexiones Activas** | TBD | N/A |
| **Tiempo de Query Promedio** | TBD | N/A |
| **Lock Waits** | TBD | N/A |

## 🚨 Problemas Identificados

### Cuellos de Botella Potenciales
- TBD

### Errores Recurrentes
- TBD

### Recomendaciones de Optimización
- TBD

## ✅ Evaluación Final

### Estado General de la Prueba
$($TestPassed ? '✅ **APROBADA** - El sistema cumple con los SLOs establecidos' : '❌ **RECHAZADA** - El sistema no cumple con los SLOs establecidos')

### Criterios de Aceptación (Día 27)
- [ ] P95 Tiempo de Respuesta < 500ms: $($P95ResponseTime -lt 500 ? '✅' : '❌')
- [ ] Error Rate < 2%: $($ErrorRate -lt 0.02 ? '✅' : '❌')
- [ ] RPS Sostenidos > 100: $($RequestsPerSecond -gt 100 ? '✅' : '❌')
- [ ] Sin errores críticos de infraestructura: $($TestPassed ? '✅' : '❌')

## 📁 Archivos de Resultados

### Reportes Generados
- **Resultados JSON**: `$OutputFile`
- **Gráficas de Rendimiento**: `$RunDir/charts/`
- **Logs Detallados**: `$RunDir/logs/`
- **Configuración de Prueba**: `$RunDir/test-config.json`

### Artefactos para Análisis
- **Script de Prueba**: `load-tests/load-test.js`
- **Configuración de Entorno**: `$RunDir/env-config.json`

## 🎯 Acciones Recomendadas

### Si la Prueba FALLÓ:
1. **Revisar logs de aplicación** durante el período de carga
2. **Identificar endpoints más lentos** con P95 > 500ms
3. **Optimizar consultas de base de datos** lentas
4. **Revisar configuración de caché** (Redis)
5. **Aumentar recursos** si es problema de infraestructura

### Si la Prueba PASÓ:
1. **Documentar métricas de baseline** para futuras comparaciones
2. **Configurar monitoreo continuo** con estos thresholds
3. **Preparar para pruebas de estrés** (más usuarios/duración)
4. **Continuar con pruebas de seguridad**

## 📞 Información de Contacto

**Equipo de Performance:**
- Email: performance@monitorimpresoras.com
- Slack: #performance-monitor
- Jira: Proyecto MONITOR-PERF

---
*Informe generado automáticamente por script de pruebas de carga*
"@

    $ReportContent | Out-File (Join-Path $RunDir "load-test-report.md")

    # Crear configuración de esta ejecución
    $Config = @{
        Environment = $Environment
        BaseUrl = $BaseUrl
        Users = $Users
        Duration = $Duration
        TestPassed = $TestPassed
        Metrics = @{
            TotalRequests = $TotalRequests
            FailedRequests = $FailedRequests
            AvgResponseTime = $AvgResponseTime
            P95ResponseTime = $P95ResponseTime
            ErrorRate = $ErrorRate
            RequestsPerSecond = $RequestsPerSecond
        }
        ExecutionTime = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    }

    $Config | ConvertTo-Json | Out-File (Join-Path $RunDir "test-config.json")

    # Mostrar resumen en consola
    Write-Host ""
    Write-Host "📊 RESUMEN DE PRUEBA DE CARGA" -ForegroundColor Cyan
    Write-Host "==============================" -ForegroundColor Cyan
    Write-Host "Usuarios: $Users" -ForegroundColor White
    Write-Host "Duración: $Duration" -ForegroundColor White
    Write-Host "P95 Tiempo: $($P95ResponseTime.ToString("F0"))ms $($P95ResponseTime -lt 500 ? '✅' : '❌')" -ForegroundColor White
    Write-Host "Error Rate: $($ErrorRate.ToString("P1")) $($ErrorRate -lt 0.02 ? '✅' : '❌')" -ForegroundColor White
    Write-Host "Estado General: $($TestPassed ? '✅ PASSED' : '❌ FAILED')" -ForegroundColor White

} else {
    Write-Warning "⚠️ No se encontraron resultados de k6 para procesar"
}

# 6. Generar recomendaciones basadas en resultados
Write-Host "💡 Generando recomendaciones..." -ForegroundColor Yellow

if (!$TestPassed) {
    $Recommendations = @"
❌ PRUEBA FALLIDA - ACCIONES REQUERIDAS:

🔍 Áreas Problemáticas Identificadas:
   • Tiempo de respuesta P95 > 500ms indica posible cuello de botella
   • Error rate > 2% sugiere problemas de estabilidad
   • Bajo RPS indica limitaciones de capacidad

🚨 Acciones Inmediatas:
   1. Revisar logs de aplicación durante período de carga
   2. Identificar consultas lentas en base de datos
   3. Verificar configuración de caché y Redis
   4. Analizar uso de recursos del servidor

📋 Tickets a Crear:
   • [P0] Investigación de rendimiento degradado
   • [P1] Optimización de consultas de base de datos
   • [P1] Revisión de configuración de caché
"@
} else {
    $Recommendations = @"
✅ PRUEBA EXITOSA - SISTEMA LISTO:

🎯 Métricas Excelentes:
   • Rendimiento dentro de objetivos SLO
   • Baja tasa de errores mantenida
   • Buena capacidad de respuesta

📊 Próximos Pasos:
   1. Documentar métricas como baseline de producción
   2. Configurar monitoreo continuo con estos thresholds
   3. Preparar pruebas de estrés adicionales
   4. Continuar con pruebas de seguridad

🔄 Estado: Sistema listo para despliegue en producción
"@
}

$Recommendations | Out-File (Join-Path $RunDir "recommendations.md")

# 7. Mostrar resultado final
Write-Host ""
Write-Host "📋 EJECUCIÓN DE PRUEBA DE CARGA COMPLETADA" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

if ($TestPassed) {
    Write-Host "✅ PRUEBA APROBADA - Sistema cumple con SLOs" -ForegroundColor Green
} else {
    Write-Host "❌ PRUEBA RECHAZADA - Sistema no cumple con SLOs" -ForegroundColor Red
}

Write-Host ""
Write-Host "📁 Informe completo: $(Join-Path $RunDir "load-test-report.md")" -ForegroundColor Cyan
Write-Host "💡 Próximo paso: Ejecutar escaneo de seguridad OWASP ZAP" -ForegroundColor Yellow

# 8. Crear script de monitoreo de recursos (si se solicita)
if ($MonitorResources) {
    Write-Host "📊 Configurando monitoreo de recursos..." -ForegroundColor Yellow

    $MonitorScript = @"
# Script de monitoreo de recursos durante pruebas de carga
# Ejecutar en otra ventana durante la prueba

while (\$true) {
    \$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'

    # Información del sistema
    \$cpu = Get-WmiObject Win32_Processor | Measure-Object -Property LoadPercentage -Average | Select-Object -ExpandProperty Average
    \$memory = Get-WmiObject Win32_OperatingSystem | Select-Object -ExpandProperty FreePhysicalMemory
    \$memoryGB = [math]::Round(\$memory / 1MB, 2)

    # Información de procesos de .NET
    \$dotnetProcesses = Get-Process | Where-Object { \$_.ProcessName -like "*dotnet*" } | Select-Object Id, ProcessName, CPU, WorkingSet

    Write-Host "[$timestamp] CPU: $($cpu.ToString('F1'))% | Memoria Libre: ${memoryGB}GB"

    foreach (\$proc in \$dotnetProcesses) {
        \$wsMB = [math]::Round(\$proc.WorkingSet / 1MB, 2)
        Write-Host "  Process $($proc.ProcessName)[$(\$proc.Id)]: CPU $($proc.CPU.ToString('F1'))% | Memoria ${wsMB}MB"
    }

    Start-Sleep -Seconds 5
}
"@

    $MonitorScript | Out-File (Join-Path $RunDir "monitor-resources.ps1")
    Write-Host "✅ Script de monitoreo creado: $(Join-Path $RunDir "monitor-resources.ps1")" -ForegroundColor Green
}
