# Script de preparación del entorno de staging para QA
# Ejecutar antes de iniciar las pruebas de validación

param(
    [string]$Environment = "Staging",
    [switch]$Force = $false
)

Write-Host "🚀 Configurando entorno de $Environment para pruebas QA..." -ForegroundColor Green

# 1. Verificar entorno actual
$CurrentDir = Get-Location
if (!(Test-Path "MonitorImpresoras.sln")) {
    Write-Error "❌ No se encuentra el archivo de solución. Ejecutar desde la raíz del proyecto."
    exit 1
}

# 2. Limpiar builds anteriores
Write-Host "🧹 Limpiando builds anteriores..." -ForegroundColor Yellow
if (Test-Path "./publish") {
    Remove-Item "./publish" -Recurse -Force
}

if (Test-Path "./**/bin/Release") {
    Get-ChildItem "./**/bin/Release" -Recurse | Remove-Item -Recurse -Force
}

# 3. Restaurar dependencias
Write-Host "📦 Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Error restaurando dependencias"
    exit 1
}

# 4. Compilar solución
Write-Host "🔨 Compilando solución en Release..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Error compilando solución"
    exit 1
}

# 5. Ejecutar tests unitarios
Write-Host "🧪 Ejecutando tests unitarios..." -ForegroundColor Yellow
dotnet test --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=staging-unit-tests.trx"

if ($LASTEXITCODE -ne 0) {
    Write-Warning "⚠️ Algunos tests unitarios fallaron. Revisar logs para detalles."
}

# 6. Ejecutar tests de integración
Write-Host "🔗 Ejecutando tests de integración..." -ForegroundColor Yellow
dotnet test --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=staging-integration-tests.trx" --filter "Category=Integration"

if ($LASTEXITCODE -ne 0) {
    Write-Warning "⚠️ Algunos tests de integración fallaron. Revisar logs para detalles."
}

# 7. Publicar aplicación
Write-Host "📦 Publicando aplicación..." -ForegroundColor Yellow
dotnet publish "MonitorImpresoras.API/MonitorImpresoras.API.csproj" -c Release -o "./publish"

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Error publicando aplicación"
    exit 1
}

# 8. Verificar archivos publicados
Write-Host "✅ Verificando archivos publicados..." -ForegroundColor Green
$PublishedFiles = Get-ChildItem "./publish" -Recurse
Write-Host "📊 Archivos publicados: $($PublishedFiles.Count)"

# Verificar archivos críticos
$CriticalFiles = @(
    "MonitorImpresoras.API.exe",
    "MonitorImpresoras.API.dll",
    "appsettings.Staging.json",
    "Web.config"
)

foreach ($file in $CriticalFiles) {
    if (Test-Path "./publish/$file") {
        Write-Host "✅ $file" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ Archivo faltante: $file"
    }
}

# 9. Configurar base de datos staging (si aplica)
Write-Host "🗄️ Configurando base de datos $Environment..." -ForegroundColor Yellow

try {
    # Cambiar a directorio de infraestructura
    Set-Location "MonitorImpresoras/MonitorImpresoras.Infrastructure"

    # Crear migraciones si no existen
    if (!(Test-Path "Migrations")) {
        Write-Host "📋 Creando migraciones iniciales..." -ForegroundColor Yellow
        dotnet ef migrations add "StagingInit" --context ApplicationDbContext
    }

    # Aplicar migraciones
    Write-Host "🔄 Aplicando migraciones..." -ForegroundColor Yellow
    dotnet ef database update --context ApplicationDbContext

    Set-Location $CurrentDir
    Write-Host "✅ Base de datos configurada correctamente" -ForegroundColor Green

} catch {
    Set-Location $CurrentDir
    Write-Warning "⚠️ Error configurando base de datos: $($_.Exception.Message)"
    Write-Warning "   Continuar manualmente con: cd MonitorImpresoras/MonitorImpresoras.Infrastructure && dotnet ef database update"
}

# 10. Crear directorio de resultados de QA
Write-Host "📊 Creando directorio de resultados QA..." -ForegroundColor Yellow
$QaResultsDir = "./qa-results/$Environment-$(Get-Date -Format 'yyyy-MM-dd-HHmmss')"
New-Item -ItemType Directory -Path $QaResultsDir -Force

# Copiar resultados de tests
if (Test-Path "*.trx") {
    Copy-Item "*.trx" $QaResultsDir/
}

# Crear archivo de configuración de entorno
$EnvConfig = @{
    Environment = $Environment
    BuildTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    GitCommit = $(git rev-parse HEAD 2>$null) ?? "unknown"
    GitBranch = $(git branch --show-current 2>$null) ?? "unknown"
    PublishedFiles = $PublishedFiles.Count
    StagingUrl = "https://staging.monitorimpresoras.com"
    AdminCredentials = "admin@monitorimpresoras.com / Admin123!"
}

$EnvConfig | ConvertTo-Json | Out-File "$QaResultsDir/environment-config.json"

Write-Host "✅ Entorno $Environment preparado exitosamente" -ForegroundColor Green
Write-Host "📁 Resultados de QA: $QaResultsDir" -ForegroundColor Cyan
Write-Host "🔗 URL de staging: $($EnvConfig.StagingUrl)" -ForegroundColor Cyan
Write-Host "👤 Credenciales admin: $($EnvConfig.AdminCredentials)" -ForegroundColor Cyan

# 11. Crear checklist de verificación
$Checklist = @"
✅ CHECKLIST DE VERIFICACIÓN PRE-QA
=====================================

📋 Aplicación:
   [ ] Aplicación compila correctamente en Release
   [ ] Todos los tests unitarios pasan
   [ ] Tests de integración pasan (>95% OK)
   [ ] Aplicación publica correctamente
   [ ] Swagger accesible y funcionando

🗄️ Base de Datos:
   [ ] Conexión a BD funciona
   [ ] Migraciones aplicadas correctamente
   [ ] Datos de prueba inicializados
   [ ] Índices creados y optimizados

🔒 Seguridad:
   [ ] Headers de seguridad presentes
   [ ] Rate limiting funcionando
   [ ] Autenticación JWT operativa
   [ ] CORS configurado correctamente

📊 Logs:
   [ ] Serilog configurado para $Environment
   [ ] Logs escribiendo correctamente
   [ ] Rotación de logs funcionando

🌐 Despliegue:
   [ ] Archivos publicados en ./publish
   [ ] Web.config configurado correctamente
   [ ] Configuración de entorno aplicada

📝 Testing:
   [ ] Scripts de test listos para ejecutar
   [ ] Entorno de resultados preparado
   [ ] Métricas de baseline establecidas
"@

$Checklist | Out-File "$QaResultsDir/qa-checklist.md"

Write-Host ""
Write-Host "📋 Checklist creado: $QaResultsDir/qa-checklist.md" -ForegroundColor Cyan
Write-Host ""
Write-Host "🎯 Entorno listo para ejecutar pruebas QA" -ForegroundColor Green
Write-Host "💡 Siguiente paso: Ejecutar tests automatizados con 'dotnet test'" -ForegroundColor Yellow
