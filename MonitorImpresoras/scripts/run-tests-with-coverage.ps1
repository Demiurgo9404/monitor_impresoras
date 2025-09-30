# Script para ejecutar tests automatizados con cobertura de código
# Uso: .\run-tests-with-coverage.ps1 [opcional: -Filter "Category!=Integration"]

param(
    [string]$Filter = "",
    [string]$Configuration = "Release",
    [string]$ResultsDir = "./test-results",
    [switch]$SkipIntegration = $false,
    [switch]$GenerateHtmlReport = $true
)

Write-Host "🧪 Ejecutando tests automatizados con cobertura de código..." -ForegroundColor Green

# Crear directorio de resultados
New-Item -ItemType Directory -Path $ResultsDir -Force

# 1. Ejecutar tests unitarios con cobertura
Write-Host "📊 Ejecutando tests unitarios con cobertura..." -ForegroundColor Yellow

$UnitTestFilter = if ($Filter) { "$Filter AND Category!=Integration" } else { "Category!=Integration" }

dotnet test `
    --configuration $Configuration `
    --no-build `
    --verbosity normal `
    --logger "trx;LogFileName=unit-tests.trx" `
    --logger "html;LogFileName=unit-tests.html" `
    --collect:"XPlat Code Coverage" `
    --settings coverlet.runsettings `
    --filter "$UnitTestFilter" `
    --results-directory $ResultsDir

if ($LASTEXITCODE -ne 0) {
    Write-Warning "⚠️ Algunos tests unitarios fallaron. Revisar logs para detalles."
    $UnitTestsPassed = $false
} else {
    Write-Host "✅ Tests unitarios completados exitosamente" -ForegroundColor Green
    $UnitTestsPassed = $true
}

# 2. Ejecutar tests de integración (si no se omite)
if (!$SkipIntegration) {
    Write-Host "🔗 Ejecutando tests de integración..." -ForegroundColor Yellow

    $IntegrationFilter = if ($Filter) { "$Filter AND Category=Integration" } else { "Category=Integration" }

    dotnet test `
        --configuration $Configuration `
        --no-build `
        --verbosity normal `
        --logger "trx;LogFileName=integration-tests.trx" `
        --logger "html;LogFileName=integration-tests.html" `
        --filter "$IntegrationFilter" `
        --results-directory $ResultsDir

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "⚠️ Algunos tests de integración fallaron. Revisar logs para detalles."
        $IntegrationTestsPassed = $false
    } else {
        Write-Host "✅ Tests de integración completados exitosamente" -ForegroundColor Green
        $IntegrationTestsPassed = $true
    }
}

# 3. Generar reporte HTML de cobertura (si está habilitado)
if ($GenerateHtmlReport) {
    Write-Host "📈 Generando reporte HTML de cobertura..." -ForegroundColor Yellow

    # Buscar archivos de cobertura generados
    $CoverageFiles = Get-ChildItem -Path $ResultsDir -Name "*.xml" -Recurse | Where-Object { $_.Name -like "*coverage*" }

    if ($CoverageFiles.Count -gt 0) {
        $CoverageFile = $CoverageFiles[0]
        $CoveragePath = Join-Path $ResultsDir $CoverageFile

        # Crear directorio para reporte HTML
        $ReportDir = Join-Path $ResultsDir "coverage-report"

        # Instalar y ejecutar reportgenerator (si no está instalado)
        try {
            dotnet tool install dotnet-reportgenerator-globaltool --tool-path . 2>$null
        } catch {
            # Ya está instalado
        }

        # Generar reporte HTML
        .\reportgenerator.exe `
            -reports:$CoveragePath `
            -targetdir:$ReportDir `
            -reporttypes:Html `
            -title:"Monitor Impresoras - Cobertura de Código"

        if (Test-Path $ReportDir) {
            Write-Host "✅ Reporte HTML de cobertura generado: $ReportDir/index.html" -ForegroundColor Green
        }
    } else {
        Write-Warning "⚠️ No se encontraron archivos de cobertura para generar reporte HTML"
    }
}

# 4. Analizar resultados y generar resumen
Write-Host "📋 Generando resumen de resultados..." -ForegroundColor Yellow

$Summary = @"
🏗️ RESUMEN DE TESTS AUTOMATIZADOS
====================================

📊 Tests Unitarios: $($UnitTestsPassed ? '✅ PASSED' : '❌ FAILED')
🔗 Tests de Integración: $($IntegrationTestsPassed ? '✅ PASSED' : '❌ FAILED')
📈 Reporte de Cobertura: $($GenerateHtmlReport ? '✅ GENERADO' : '❌ NO GENERADO')

📁 Ubicación de Resultados:
   - TRX Files: $ResultsDir/*.trx
   - HTML Reports: $ResultsDir/*.html
   - Coverage Report: $ResultsDir/coverage-report/index.html

📋 Archivos de Cobertura Encontrados:
$(if ($CoverageFiles.Count -gt 0) { $CoverageFiles | ForEach-Object { "   - $_" } } else { "   - Ninguno encontrado" })

⏰ Ejecutado: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
"@

$Summary | Out-File (Join-Path $ResultsDir "test-summary.md")

Write-Host ""
Write-Host "📊 RESULTADOS DE TESTS" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host $Summary

# 5. Crear matriz de resultados para análisis
$TestResults = @{
    UnitTestsPassed = $UnitTestsPassed
    IntegrationTestsPassed = $IntegrationTestsPassed
    HtmlReportGenerated = $GenerateHtmlReport -and (Test-Path $ReportDir)
    CoverageFilesFound = $CoverageFiles.Count -gt 0
    ResultsDirectory = $ResultsDir
    ExecutionTime = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
}

$TestResults | ConvertTo-Json | Out-File (Join-Path $ResultsDir "test-results.json")

# 6. Verificar criterios de aceptación
Write-Host ""
Write-Host "🎯 VERIFICACIÓN DE CRITERIOS DE ACEPTACIÓN" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$CriteriaMet = $true

if (!$UnitTestsPassed) {
    Write-Host "❌ CRITERIO FALLADO: Tests unitarios deben pasar al 100%" -ForegroundColor Red
    $CriteriaMet = $false
}

if (!$SkipIntegration -and !$IntegrationTestsPassed) {
    Write-Host "❌ CRITERIO FALLADO: Tests de integración deben pasar >95%" -ForegroundColor Red
    $CriteriaMet = $false
}

if (!$GenerateHtmlReport) {
    Write-Host "⚠️ ADVERTENCIA: No se generó reporte HTML de cobertura" -ForegroundColor Yellow
}

if ($CriteriaMet) {
    Write-Host "✅ CRITERIOS DE ACEPTACIÓN CUMPLIDOS" -ForegroundColor Green
    Write-Host "   ¡El código está listo para continuar con pruebas E2E!" -ForegroundColor Green
} else {
    Write-Host "❌ CRITERIOS DE ACEPTACIÓN NO CUMPLIDOS" -ForegroundColor Red
    Write-Host "   Revisar y corregir errores antes de continuar." -ForegroundColor Red
}

Write-Host ""
Write-Host "📁 Reportes disponibles en: $ResultsDir" -ForegroundColor Cyan
Write-Host "💡 Siguiente paso: Ejecutar pruebas end-to-end" -ForegroundColor Yellow
