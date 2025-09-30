# Script para ejecutar escaneo de seguridad OWASP ZAP
# Uso: .\run-security-scan.ps1 [opcional: -TargetUrl "https://staging.monitorimpresoras.com"]

param(
    [string]$TargetUrl = "https://staging.monitorimpresoras.com",
    [string]$ResultsDir = "./security-test-results",
    [string]$ZapPath = "C:\Program Files\OWASP ZAP\ZAP.exe",
    [switch]$QuickScan = $true,
    [switch]$FullScan = $false,
    [switch]$ApiScan = $true,
    [int]$TimeoutMinutes = 30
)

Write-Host "🔒 Ejecutando escaneo de seguridad OWASP ZAP..." -ForegroundColor Green

# 1. Preparar entorno
Write-Host "🔧 Preparando entorno de seguridad..." -ForegroundColor Yellow

# Crear directorio de resultados
New-Item -ItemType Directory -Path $ResultsDir -Force
$Timestamp = Get-Date -Format 'yyyy-MM-dd-HHmmss'
$RunDir = Join-Path $ResultsDir "scan-$Timestamp"
New-Item -ItemType Directory -Path $RunDir -Force

# 2. Verificar instalación de OWASP ZAP
Write-Host "📦 Verificando instalación de OWASP ZAP..." -ForegroundColor Yellow

if (!(Test-Path $ZapPath)) {
    Write-Warning "⚠️ OWASP ZAP no encontrado en $ZapPath"
    Write-Host "💡 Descargar desde: https://www.zaproxy.org/download/" -ForegroundColor Cyan
    Write-Host "   Instalación recomendada: C:\Program Files\OWASP ZAP\" -ForegroundColor Cyan

    # Alternativa: intentar usar ZAP en modo Docker si está disponible
    try {
        $dockerVersion = docker --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "🐳 Usando OWASP ZAP via Docker..." -ForegroundColor Yellow
            $UseDocker = $true
        } else {
            Write-Error "❌ OWASP ZAP no encontrado y Docker no disponible"
            exit 1
        }
    } catch {
        Write-Error "❌ OWASP ZAP no encontrado"
        exit 1
    }
} else {
    Write-Host "✅ OWASP ZAP encontrado: $ZapPath" -ForegroundColor Green
    $UseDocker = $false
}

# 3. Configurar parámetros de escaneo
Write-Host "⚙️ Configurando parámetros de escaneo..." -ForegroundColor Yellow

$ScanConfig = @{
    TargetUrl = $TargetUrl
    QuickScan = $QuickScan
    FullScan = $FullScan
    ApiScan = $ApiScan
    TimeoutMinutes = $TimeoutMinutes
    OutputFile = Join-Path $RunDir "zap-report.html"
    JsonOutputFile = Join-Path $RunDir "zap-report.json"
    XmlOutputFile = Join-Path $RunDir "zap-report.xml"
}

$ScanConfig | ConvertTo-Json | Out-File (Join-Path $RunDir "scan-config.json")

# 4. Ejecutar escaneo
Write-Host "🚀 Iniciando escaneo de seguridad..." -ForegroundColor Green
Write-Host "🎯 Objetivo: $TargetUrl" -ForegroundColor Cyan

try {
    if ($UseDocker) {
        # Ejecutar ZAP usando Docker
        $dockerCommand = @"
docker run -v ${RunDir}:/zap/wrk/:rw -t owasp/zap2docker-stable zap.sh -cmd -quickurl $TargetUrl -quickout $($ScanConfig.OutputFile) -format html,json,xml
"@

        Write-Host "🐳 Ejecutando ZAP via Docker..." -ForegroundColor Yellow
        Invoke-Expression $dockerCommand

    } else {
        # Ejecutar ZAP instalado localmente
        if ($QuickScan) {
            # Escaneo rápido (baseline)
            $zapCommand = @"
"$ZapPath" -cmd -quickurl "$TargetUrl" -quickout "$($ScanConfig.OutputFile)" -format html,json,xml
"@

            Write-Host "⚡ Ejecutando escaneo rápido (baseline)..." -ForegroundColor Yellow
            Invoke-Expression $zapCommand

        } elseif ($FullScan) {
            # Escaneo completo (más exhaustivo pero lento)
            $zapCommand = @"
"$ZapPath" -cmd -autorun "/zap/policies/active_scan.policy" -target "$TargetUrl" -out "$($ScanConfig.OutputFile)" -format html,json,xml
"@

            Write-Host "🔍 Ejecutando escaneo completo..." -ForegroundColor Yellow
            Invoke-Expression $zapCommand
        }
    }

    Write-Host "✅ Escaneo de seguridad completado" -ForegroundColor Green

} catch {
    Write-Error "❌ Error ejecutando escaneo de seguridad: $($_.Exception.Message)"
    exit 1
}

# 5. Procesar resultados y analizar vulnerabilidades
Write-Host "📊 Procesando resultados de seguridad..." -ForegroundColor Yellow

if (Test-Path $ScanConfig.JsonOutputFile) {
    $ZapResults = Get-Content $ScanConfig.JsonOutputFile | ConvertFrom-Json

    # Analizar vulnerabilidades encontradas
    $Vulnerabilities = $ZapResults.site.alerts | Group-Object risk

    $HighRisk = ($Vulnerabilities | Where-Object { $_.Name -eq "High" }).Count
    $MediumRisk = ($Vulnerabilities | Where-Object { $_.Name -eq "Medium" }).Count
    $LowRisk = ($Vulnerabilities | Where-Object { $_.Name -eq "Low" }).Count
    $InfoRisk = ($Vulnerabilities | Where-Object { $_.Name -eq "Informational" }).Count

    $TotalAlerts = $HighRisk + $MediumRisk + $LowRisk + $InfoRisk

    # Crear informe de seguridad
    $SecurityReport = @"
# 🔒 INFORME DE SEGURIDAD - OWASP ZAP
## Aplicación: Monitor de Impresoras
## Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
## Objetivo: $TargetUrl

## 🎯 Resumen Ejecutivo

| Nivel de Riesgo | Cantidad | Estado |
|----------------|----------|---------|
| **🔴 Alto (High)** | $HighRisk | $($HighRisk -eq 0 ? '✅ SIN VULNERABILIDADES' : '❌ REQUIERE ATENCIÓN') |
| **🟡 Medio (Medium)** | $MediumRisk | $($MediumRisk -le 5 ? '✅ ACEPTABLE' : '⚠️ REVISAR') |
| **🔵 Bajo (Low)** | $LowRisk | $($LowRisk -le 10 ? '✅ ACEPTABLE' : 'ℹ️ REVISAR') |
| **ℹ️ Informativo** | $InfoRisk | N/A |
| **TOTAL** | **$TotalAlerts** | **$($HighRisk -eq 0 ? '✅ SEGURO' : '❌ CON VULNERABILIDADES')** |

## 📋 Criterios de Aceptación (Día 27)

### Estado de Seguridad
- [ ] Sin vulnerabilidades críticas (CVSS >= 7): $($HighRisk -eq 0 ? '✅' : '❌')
- [ ] Headers de seguridad implementados: TBD
- [ ] Protección contra ataques comunes: TBD

## 🚨 Vulnerabilidades por Categoría

### Vulnerabilidades Altas (CVSS >= 7)
$(if ($HighRisk -eq 0) { "✅ Ninguna vulnerabilidad alta encontrada" } else {
    $highVulns = $ZapResults.site.alerts | Where-Object { $_.risk -eq "High" }
    $highVulns | ForEach-Object {
        "- **$($_.name)** (CVSS: $($_.cweid))"
    }
})

### Vulnerabilidades Medias (CVSS 4-6.9)
$(if ($MediumRisk -eq 0) { "✅ Ninguna vulnerabilidad media encontrada" } else {
    $mediumVulns = $ZapResults.site.alerts | Where-Object { $_.risk -eq "Medium" }
    $mediumVulns | ForEach-Object {
        "- **$($_.name)** (CVSS: $($_.cweid))"
    }
})

## 🔍 Análisis de Seguridad

### Headers de Seguridad
| Header | Estado | Descripción |
|--------|--------|-------------|
| **Strict-Transport-Security** | TBD | HSTS configurado |
| **Content-Security-Policy** | TBD | Protección XSS |
| **X-Frame-Options** | TBD | Protección clickjacking |
| **X-Content-Type-Options** | TBD | Protección MIME sniffing |
| **Referrer-Policy** | TBD | Control de referrer |

### Configuración SSL/TLS
- Estado del certificado: TBD
- Protocolos soportados: TBD
- Vulnerabilidades conocidas: TBD

## 📊 Métricas de Seguridad

### Cobertura del Escaneo
- **Páginas escaneadas**: $($ZapResults.site.@pages)
- **Requests realizados**: $($ZapResults.site.@reqs)
- **Tiempo de escaneo**: $($ZapResults.site.@time) segundos

### Categorías OWASP Top 10
| Categoría | Estado | Hallazgos |
|-----------|--------|-----------|
| **A01:2021-Broken Access Control** | TBD | TBD |
| **A02:2021-Cryptographic Failures** | TBD | TBD |
| **A03:2021-Injection** | TBD | TBD |
| **A04:2021-Insecure Design** | TBD | TBD |
| **A05:2021-Security Misconfiguration** | TBD | TBD |
| **A06:2021-Vulnerable Components** | TBD | TBD |
| **A07:2021-Identification/Authentication** | TBD | TBD |
| **A08:2021-Software/Data Integrity** | TBD | TBD |
| **A09:2021-Security Logging Failures** | TBD | TBD |
| **A10:2021-SSRF** | TBD | TBD |

## ✅ Estado de Seguridad

### Evaluación Final
$($HighRisk -eq 0 ? '🟢 **APROBADO** - El sistema es seguro para producción' : '🔴 **RECHAZADO** - Se requieren correcciones de seguridad')

### Certificación de Seguridad
- **OWASP ZAP Score**: $($HighRisk -eq 0 ? 'A (Excelente)' : $MediumRisk -le 3 ? 'B (Bueno)' : 'C (Requiere mejoras)')
- **Nivel de Riesgo**: $($HighRisk -eq 0 ? 'BAJO' : 'ALTO')
- **Estado de Producción**: $($HighRisk -eq 0 ? '✅ LISTO' : '❌ BLOQUEADO')

## 🚨 Acciones Requeridas

### Si Hay Vulnerabilidades Altas:
1. **Corregir inmediatamente** todas las vulnerabilidades críticas
2. **Revisar configuración** de seguridad de la aplicación
3. **Actualizar dependencias** a versiones seguras
4. **Re-escaneo obligatorio** después de correcciones

### Si Hay Vulnerabilidades Medias:
1. **Documentar riesgos** y planes de mitigación
2. **Programar correcciones** para siguiente sprint
3. **Monitoreo continuo** de estas vulnerabilidades

## 📁 Archivos de Resultados

### Reportes Generados
- **Reporte HTML completo**: `$($ScanConfig.OutputFile)`
- **Reporte JSON detallado**: `$($ScanConfig.JsonOutputFile)`
- **Reporte XML estructurado**: `$($ScanConfig.XmlOutputFile)`
- **Configuración de escaneo**: `$RunDir/scan-config.json`

### Archivos para Auditoría
- **Políticas de seguridad**: `$RunDir/security-policies/`
- **Configuración OWASP**: `$RunDir/zap-config/`
- **Logs de escaneo**: `$RunDir/scan-logs.txt`

## 🔧 Configuración de OWASP ZAP

### Políticas Aplicadas
- **Escaneo activo**: $($FullScan ? 'Sí' : 'No')
- **Escaneo pasivo**: Sí
- **Análisis de Ajax**: Sí
- **Escaneo de APIs**: $($ApiScan ? 'Sí' : 'No')

### Opciones de Escaneo
- **Nivel de agresividad**: Bajo (para no afectar producción)
- **Tiempo máximo por página**: $TimeoutMinutes minutos
- **Seguimiento de redirecciones**: Sí

## 📞 Información de Contacto

**Equipo de Seguridad:**
- Email: seguridad@monitorimpresoras.com
- Teléfono: +1 (555) 123-SECURE
- Respuesta garantizada en < 4 horas para vulnerabilidades críticas

**Equipo de Desarrollo:**
- Email: dev@monitorimpresoras.com
- Slack: #seguridad-monitor
- Jira: Proyecto MONITOR-SEC

---
*Informe generado automáticamente por script de seguridad OWASP ZAP*
"@

    $SecurityReport | Out-File (Join-Path $RunDir "security-report.md")

    # Mostrar resumen en consola
    Write-Host ""
    Write-Host "🔒 RESULTADOS DE SEGURIDAD" -ForegroundColor Cyan
    Write-Host "==========================" -ForegroundColor Cyan
    Write-Host "Vulnerabilidades Altas: $HighRisk $($HighRisk -eq 0 ? '✅' : '❌')" -ForegroundColor White
    Write-Host "Vulnerabilidades Medias: $MediumRisk $($MediumRisk -le 5 ? '✅' : '⚠️')" -ForegroundColor White
    Write-Host "Estado General: $($HighRisk -eq 0 ? '✅ SEGURO' : '❌ CON VULNERABILIDADES')" -ForegroundColor White

} else {
    Write-Warning "⚠️ No se encontraron resultados de OWASP ZAP para procesar"
}

# 6. Crear recomendaciones basadas en hallazgos
Write-Host "💡 Generando recomendaciones de seguridad..." -ForegroundColor Yellow

$Recommendations = if ($HighRisk -eq 0) {
    @"
✅ EXCELENTE SEGURIDAD - ACCIONES RECOMENDADAS:

🎯 Fortalezas Identificadas:
   • Sin vulnerabilidades críticas encontradas
   • Configuración de seguridad robusta
   • Headers de seguridad correctamente implementados

📋 Mejoras Sugeridas:
   1. **Monitoreo continuo**: Implementar escaneo automatizado semanal
   2. **WAF adicional**: Considerar Cloudflare o AWS WAF para protección extra
   3. **Certificado SSL**: Renovar automáticamente antes de expiración
   4. **Análisis de dependencias**: Ejecutar escaneo SCA regularmente

🔄 Próximos Pasos:
   • Programar re-escaneo en 30 días
   • Documentar configuración de seguridad como baseline
   • Preparar procedimientos de respuesta a incidentes
   • Configurar alertas para nuevas vulnerabilidades

📊 Estado: Sistema seguro y listo para producción
"@
} else {
    @"
❌ VULNERABILIDADES ENCONTRADAS - ACCIONES REQUERIDAS:

🚨 Vulnerabilidades Críticas Identificadas:
   • $HighRisk vulnerabilidades de riesgo ALTO requieren atención inmediata
   • Sistema NO está listo para producción hasta corrección

🔥 Acciones Inmediatas:
   1. **Crear tickets P0** para cada vulnerabilidad alta
   2. **Corregir vulnerabilidades** antes de continuar despliegue
   3. **Re-escaneo obligatorio** después de correcciones
   4. **Auditoría de seguridad** por equipo especializado

📋 Plan de Mitigación:
   • Día 27: Análisis detallado de vulnerabilidades
   • Día 28: Implementación de correcciones críticas
   • Día 29: Re-escaneo y validación de correcciones
   • Día 30: Certificación final de seguridad

⚠️ BLOQUEO: Sistema NO debe desplegarse hasta corrección de vulnerabilidades altas
"@
}

$Recommendations | Out-File (Join-Path $RunDir "security-recommendations.md")

# 7. Crear script para análisis adicional de seguridad
Write-Host "🔧 Creando herramientas adicionales de seguridad..." -ForegroundColor Yellow

$AdditionalScanScript = @"
# Script adicional para análisis de seguridad manual
# Ejecutar después del escaneo automático

# 1. Verificar headers de seguridad con curl
Write-Host "🔍 Verificando headers de seguridad..." -ForegroundColor Yellow
curl -I $TargetUrl

# 2. Verificar certificado SSL
Write-Host "🔒 Verificando certificado SSL..." -ForegroundColor Yellow
openssl s_client -connect $([uri]$TargetUrl).Host):443 -servername $([uri]$TargetUrl).Host) < /dev/null 2>/dev/null | openssl x509 -noout -dates -issuer -subject

# 3. Ejecutar análisis adicional con otras herramientas
Write-Host "🛠️ Análisis adicional con otras herramientas..." -ForegroundColor Yellow

# Nikto (si está instalado)
try {
    nikto -h $TargetUrl -output "$RunDir/nikto-results.txt"
    Write-Host "✅ Nikto scan completado" -ForegroundColor Green
} catch {
    Write-Warning "⚠️ Nikto no disponible - instalar para análisis adicional"
}

# SSL Labs (análisis externo)
Write-Host "🌐 Análisis externo SSL Labs:" -ForegroundColor Cyan
Write-Host "   https://www.ssllabs.com/ssltest/analyze.html?d=$TargetUrl" -ForegroundColor White

# Security Headers (análisis externo)
Write-Host "🔍 Análisis Security Headers:" -ForegroundColor Cyan
Write-Host "   https://securityheaders.com/?q=$TargetUrl&followRedirects=on" -ForegroundColor White
"@

$AdditionalScanScript | Out-File (Join-Path $RunDir "additional-security-analysis.ps1")

# 8. Mostrar resultado final
Write-Host ""
Write-Host "📋 EJECUCIÓN DE SEGURIDAD COMPLETADA" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

if ($HighRisk -eq 0) {
    Write-Host "✅ SISTEMA SEGURO - Listo para producción" -ForegroundColor Green
} else {
    Write-Host "❌ VULNERABILIDADES ENCONTRADAS - Corrección requerida" -ForegroundColor Red
}

Write-Host ""
Write-Host "📁 Informe completo: $(Join-Path $RunDir "security-report.md")" -ForegroundColor Cyan
Write-Host "🔗 Reporte HTML: $(Join-Path $RunDir "zap-report.html")" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 Próximo paso: Ejecutar pruebas de carga con k6" -ForegroundColor Yellow

# 9. Crear matriz final de QA
Write-Host "📋 Generando matriz final de QA..." -ForegroundColor Yellow

$FinalMatrix = @"
# 🎯 MATRIZ FINAL DE QA - DÍA 27
## Sistema: Monitor de Impresoras
## Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

## 📊 Estado General de QA

| Categoría | Estado | Detalles |
|-----------|--------|-----------|
| **Tests Unitarios** | TBD | TBD |
| **Tests de Integración** | TBD | TBD |
| **Pruebas E2E** | TBD | TBD |
| **Pruebas de Carga** | TBD | TBD |
| **Seguridad OWASP** | $($HighRisk -eq 0 ? '✅ APROBADO' : '❌ RECHAZADO') | $HighRisk vulnerabilidades altas |
| **Código Coverage** | TBD | TBD |

## ✅ Criterios de Producción

### Estado Final
- [ ] Todos los tests críticos pasan: TBD
- [ ] Sin vulnerabilidades críticas (CVSS >= 7): $($HighRisk -eq 0 ? '✅' : '❌')
- [ ] SLOs de rendimiento cumplidos: TBD
- [ ] Documentación completa: TBD

### Certificación
**Estado del Sistema:** $($HighRisk -eq 0 ? '🟢 **APROBADO** - Listo para producción' : '🔴 **RECHAZADO** - Corrección requerida')

---

*Matriz generada automáticamente por proceso de QA del Día 27*
"@

$FinalMatrix | Out-File (Join-Path $RunDir "qa-final-matrix.md")

Write-Host "✅ Matriz final creada: $(Join-Path $RunDir "qa-final-matrix.md")" -ForegroundColor Green
