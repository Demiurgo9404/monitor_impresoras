# Script para ejecutar pruebas End-to-End con matriz de resultados
# Uso: .\run-e2e-tests.ps1 [opcional: -Environment "staging"]

param(
    [string]$Environment = "staging",
    [string]$BaseUrl = "https://$Environment.monitorimpresoras.com",
    [string]$ResultsDir = "./e2e-results",
    [switch]$Headless = $true,
    [switch]$GenerateReport = $true
)

Write-Host "🧪 Ejecutando pruebas End-to-End para $Environment..." -ForegroundColor Green

# 1. Preparar entorno
Write-Host "🔧 Preparando entorno de pruebas..." -ForegroundColor Yellow

# Crear directorio de resultados
New-Item -ItemType Directory -Path $ResultsDir -Force
$Timestamp = Get-Date -Format 'yyyy-MM-dd-HHmmss'
$RunDir = Join-Path $ResultsDir "run-$Timestamp"
New-Item -ItemType Directory -Path $RunDir -Force

# 2. Verificar dependencias
Write-Host "📦 Verificando dependencias..." -ForegroundColor Yellow

# Verificar Node.js
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ Node.js no está instalado o no está en el PATH"
    exit 1
}

# Verificar npm
try {
    $npmVersion = npm --version
    Write-Host "✅ npm: $npmVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ npm no está instalado o no está en el PATH"
    exit 1
}

# 3. Instalar dependencias de Cypress (si no existen)
Write-Host "📥 Instalando dependencias de Cypress..." -ForegroundColor Yellow

Set-Location "e2e-tests"
if (!(Test-Path "node_modules")) {
    npm install
}

if (!(Test-Path "node_modules/cypress")) {
    npx cypress install
}

# 4. Configurar variables de entorno para pruebas
Write-Host "⚙️ Configurando variables de entorno..." -ForegroundColor Yellow

$envConfig = @{
    CYPRESS_BASE_URL = $BaseUrl
    CYPRESS_API_URL = $BaseUrl
    CYPRESS_ADMIN_EMAIL = "admin@monitorimpresoras.com"
    CYPRESS_ADMIN_PASSWORD = "Admin123!"
    CYPRESS_TEST_USER_EMAIL = "test@monitorimpresoras.com"
    CYPRESS_TEST_USER_PASSWORD = "Test123!"
}

# Exportar variables de entorno
foreach ($key in $envConfig.Keys) {
    $value = $envConfig[$key]
    [Environment]::SetEnvironmentVariable($key, $value, "Process")
}

# 5. Ejecutar pruebas E2E
Write-Host "🚀 Ejecutando pruebas E2E..." -ForegroundColor Green

$cypressCommand = if ($Headless) { "run" } else { "open" }

try {
    if ($Headless) {
        # Ejecutar en modo headless y guardar resultados
        npx cypress run `
            --env baseUrl=$BaseUrl,apiUrl=$BaseUrl `
            --reporter json `
            --reporter-options output="$RunDir/results.json" `
            --record false

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "⚠️ Algunas pruebas E2E fallaron. Revisar resultados."
            $TestsPassed = $false
        } else {
            Write-Host "✅ Todas las pruebas E2E pasaron exitosamente" -ForegroundColor Green
            $TestsPassed = $true
        }
    } else {
        # Abrir Cypress en modo interactivo
        npx cypress open --env baseUrl=$BaseUrl,apiUrl=$BaseUrl
        Write-Host "🔍 Cypress abierto en modo interactivo" -ForegroundColor Cyan
        return
    }
} catch {
    Write-Error "❌ Error ejecutando pruebas E2E: $($_.Exception.Message)"
    $TestsPassed = $false
}

Set-Location ".."

# 6. Generar matriz de resultados
Write-Host "📊 Generando matriz de resultados..." -ForegroundColor Yellow

$MatrixFile = Join-Path $RunDir "test-matrix.md"

$MatrixContent = @"
# 📋 MATRIZ DE RESULTADOS - PRUEBAS END-TO-END
## Entorno: $Environment
## Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
## URL: $BaseUrl

## 🎯 Resumen Ejecutivo

| Categoría | Total | ✅ Pasaron | ❌ Fallaron | ⏭️ Omitidas | Estado |
|-----------|-------|------------|-------------|--------------|--------|
| Autenticación | TBD | TBD | TBD | TBD | TBD |
| Dashboard | TBD | TBD | TBD | TBD | TBD |
| CRUD Impresoras | TBD | TBD | TBD | TBD | TBD |
| Gestión Usuarios | TBD | TBD | TBD | TBD | TBD |
| Políticas | TBD | TBD | TBD | TBD | TBD |
| Reportes | TBD | TBD | TBD | TBD | TBD |
| Health Checks | TBD | TBD | TBD | TBD | TBD |
| **TOTAL** | **TBD** | **TBD** | **TBD** | **TBD** | **TBD** |

## 📋 Detalle por Caso de Prueba

### 🔐 Autenticación y Autorización
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Login válido | Login con credenciales correctas | TBD | TBD | TBD |
| Login inválido | Rechazo de credenciales incorrectas | TBD | TBD | TBD |
| Refresh token | Manejo correcto de refresh de token | TBD | TBD | TBD |

### 📊 Dashboard y Navegación
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Métricas básicas | Visualización correcta de métricas | TBD | TBD | TBD |
| Navegación módulos | Navegación fluida entre secciones | TBD | TBD | TBD |

### 🖨️ Gestión de Impresoras (CRUD)
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Crear impresora | Creación exitosa de nueva impresora | TBD | TBD | TBD |
| Editar impresora | Modificación correcta de datos | TBD | TBD | TBD |
| Eliminar impresora | Eliminación segura con confirmación | TBD | TBD | TBD |
| Ver detalles | Información completa y gráficos | TBD | TBD | TBD |

### 👥 Gestión de Usuarios
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Crear usuario | Nuevo usuario con roles asignados | TBD | TBD | TBD |
| Editar usuario | Modificación de datos de usuario | TBD | TBD | TBD |

### 📋 Políticas de Impresión
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Crear política | Nueva política con límites y costos | TBD | TBD | TBD |

### 📈 Reportes y Descargas
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Generar reporte | Creación de reporte básico | TBD | TBD | TBD |
| Descargar archivos | Exportación CSV/PDF | TBD | TBD | TBD |

### 🔍 Verificaciones de Health Checks
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Health básico | Respuesta correcta /health | TBD | TBD | TBD |
| Health detallado | Información completa /health/extended | TBD | TBD | TBD |
| Health BD | Estado de base de datos | TBD | TBD | TBD |

### 🚨 Manejo de Errores
| Caso | Descripción | Estado | Tiempo | Observaciones |
|------|-------------|--------|--------|---------------|
| Página 404 | Página no encontrada elegante | TBD | TBD | TBD |
| Errores de red | Manejo correcto de fallos de red | TBD | TBD | TBD |

## 📊 Métricas de Rendimiento

### Tiempos de Respuesta (P95)
| Operación | Tiempo (ms) | Estado |
|-----------|-------------|---------|
| Carga de página | TBD | TBD |
| API /printers | TBD | TBD |
| API /users | TBD | TBD |
| Login | TBD | TBD |

### Recursos Utilizados
| Métrica | Valor | Estado |
|---------|-------|---------|
| CPU máximo | TBD | TBD |
| Memoria máxima | TBD | TBD |
| Requests por segundo | TBD | TBD |

## 🚨 Errores y Problemas Encontrados

### Errores Críticos (P0)
- TBD

### Problemas Mayores (P1)
- TBD

### Observaciones (P2)
- TBD

## ✅ Criterios de Aceptación

### Estado General
- [ ] Todas las pruebas críticas pasan (autenticación, CRUD básico)
- [ ] Tiempo de respuesta P95 < 2000ms para operaciones críticas
- [ ] No hay errores críticos no documentados
- [ ] Navegación fluida sin bloqueos

### Métricas de Calidad
- [ ] Cobertura de funcionalidades principales > 80%
- [ ] Tasa de éxito de pruebas > 90%
- [ ] Tiempo promedio de ejecución < 5 minutos

## 📁 Archivos de Resultados

### Reportes Generados
- Resultados JSON: `$RunDir/results.json`
- Screenshots de fallos: `$RunDir/screenshots/`
- Videos de ejecución: `$RunDir/videos/`
- Logs detallados: `$RunDir/logs/`

### Artefactos para Análisis
- Archivo de configuración: `$RunDir/cypress.config.js`
- Variables de entorno: `$RunDir/env-config.json`

## 🎯 Próximos Pasos

1. **Corregir errores críticos** identificados
2. **Optimizar rendimiento** si P95 > 2000ms
3. **Aumentar cobertura** de pruebas si < 80%
4. **Ejecutar pruebas de carga** con k6
5. **Realizar escaneo de seguridad** con OWASP ZAP

## 📞 Información de Contacto

**Equipo de QA:**
- Email: qa@monitorimpresoras.com
- Slack: #qa-monitor-impresoras
- Jira: Proyecto MONITOR-QA

---
*Reporte generado automáticamente por script E2E*
"@

$MatrixContent | Out-File $MatrixFile

# 7. Crear archivo de configuración para esta ejecución
$ConfigFile = Join-Path $RunDir "execution-config.json"
@{
    Environment = $Environment
    BaseUrl = $BaseUrl
    ExecutionTime = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    Headless = $Headless
    TestsPassed = $TestsPassed
    ResultsDirectory = $RunDir
    MatrixFile = $MatrixFile
} | ConvertTo-Json | Out-File $ConfigFile

# 8. Mostrar resumen final
Write-Host ""
Write-Host "📋 EJECUCIÓN DE PRUEBAS E2E COMPLETADA" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

if ($TestsPassed) {
    Write-Host "✅ Todas las pruebas pasaron exitosamente" -ForegroundColor Green
} else {
    Write-Host "⚠️ Algunas pruebas fallaron - revisar matriz de resultados" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📁 Resultados disponibles en: $RunDir" -ForegroundColor Cyan
Write-Host "📊 Matriz de resultados: $MatrixFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 Siguiente paso: Ejecutar pruebas de carga con k6" -ForegroundColor Yellow

# 9. Abrir matriz de resultados si estamos en entorno interactivo
if (!$Headless) {
    Write-Host ""
    Write-Host "🔍 Abriendo matriz de resultados..." -ForegroundColor Cyan
    Start-Process $MatrixFile
}
