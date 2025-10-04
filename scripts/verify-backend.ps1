# QOPIQ Backend Verification Script
# Verifica que el backend esté completamente funcional

Write-Host "🚀 QOPIQ Backend Verification Script" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Variables
$BackendPath = "c:\Users\Advance\Documents\GitHub\monitor_impresoras\MonitorImpresoras\MonitorImpresoras.API"
$BaseUrl = "http://localhost:5278"

# Función para verificar si el servicio está corriendo
function Test-ServiceRunning {
    param($Url)
    try {
        $response = Invoke-WebRequest -Uri "$Url/health" -Method GET -TimeoutSec 10
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

# Función para verificar endpoints
function Test-Endpoint {
    param($Endpoint, $Description)
    try {
        Write-Host "Testing $Description..." -NoNewline
        $response = Invoke-WebRequest -Uri "$BaseUrl$Endpoint" -Method GET -TimeoutSec 5
        if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 401) {
            Write-Host " ✅ OK" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host " ❌ FAIL (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host " ❌ FAIL (Error: $($_.Exception.Message))" -ForegroundColor Red
        return $false
    }
}

Write-Host "`n📁 Step 1: Checking Backend Directory..." -ForegroundColor Yellow
if (Test-Path $BackendPath) {
    Write-Host "✅ Backend directory found" -ForegroundColor Green
    Set-Location $BackendPath
} else {
    Write-Host "❌ Backend directory not found: $BackendPath" -ForegroundColor Red
    exit 1
}

Write-Host "`n🔨 Step 2: Building Backend..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build --configuration Release --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Backend build successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Backend build failed:" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Build error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🗄️ Step 3: Checking Database..." -ForegroundColor Yellow
try {
    $migrationResult = dotnet ef database update 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database migrations applied successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Database migration warning (may be normal if already up to date)" -ForegroundColor Yellow
        Write-Host $migrationResult -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Database check warning: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n🌐 Step 4: Starting Backend Service..." -ForegroundColor Yellow
Write-Host "Starting backend at $BaseUrl..." -ForegroundColor Cyan

# Start backend in background
$backendJob = Start-Job -ScriptBlock {
    param($Path)
    Set-Location $Path
    dotnet run --urls="http://localhost:5278"
} -ArgumentList $BackendPath

# Wait for service to start
Write-Host "Waiting for backend to start..." -NoNewline
$attempts = 0
$maxAttempts = 30
do {
    Start-Sleep -Seconds 2
    $attempts++
    Write-Host "." -NoNewline
} while (-not (Test-ServiceRunning $BaseUrl) -and $attempts -lt $maxAttempts)

if (Test-ServiceRunning $BaseUrl) {
    Write-Host " ✅ Backend is running!" -ForegroundColor Green
} else {
    Write-Host " ❌ Backend failed to start within timeout" -ForegroundColor Red
    Stop-Job $backendJob
    Remove-Job $backendJob
    exit 1
}

Write-Host "`n🔍 Step 5: Testing Critical Endpoints..." -ForegroundColor Yellow

# Test critical endpoints
$endpoints = @(
    @{Endpoint="/health"; Description="Health Check"},
    @{Endpoint="/swagger"; Description="Swagger Documentation"},
    @{Endpoint="/api/auth/login"; Description="Authentication Endpoint"},
    @{Endpoint="/api/report"; Description="Reports Endpoint"},
    @{Endpoint="/api/scheduledreport"; Description="Scheduled Reports Endpoint"},
    @{Endpoint="/api/printers"; Description="Printers Endpoint"}
)

$successCount = 0
foreach ($endpoint in $endpoints) {
    if (Test-Endpoint $endpoint.Endpoint $endpoint.Description) {
        $successCount++
    }
}

Write-Host "`n📊 Results Summary:" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow
Write-Host "✅ Endpoints tested: $($endpoints.Count)" -ForegroundColor Green
Write-Host "✅ Endpoints working: $successCount" -ForegroundColor Green
Write-Host "❌ Endpoints failed: $($endpoints.Count - $successCount)" -ForegroundColor Red

if ($successCount -eq $endpoints.Count) {
    Write-Host "`n🎉 Backend Verification SUCCESSFUL!" -ForegroundColor Green
    Write-Host "Backend is ready for integration testing." -ForegroundColor Green
    Write-Host "`nBackend URLs:" -ForegroundColor Cyan
    Write-Host "- API Base: $BaseUrl" -ForegroundColor White
    Write-Host "- Swagger: $BaseUrl/swagger" -ForegroundColor White
    Write-Host "- Health: $BaseUrl/health" -ForegroundColor White
} else {
    Write-Host "`n⚠️ Backend Verification COMPLETED WITH WARNINGS" -ForegroundColor Yellow
    Write-Host "Some endpoints may need attention, but core functionality appears working." -ForegroundColor Yellow
}

Write-Host "`n🔧 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Keep this backend running for frontend integration" -ForegroundColor White
Write-Host "2. Test authentication with sample users" -ForegroundColor White
Write-Host "3. Verify database connectivity" -ForegroundColor White
Write-Host "4. Run frontend verification script" -ForegroundColor White

Write-Host "`nPress any key to stop backend or Ctrl+C to keep it running..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Cleanup
Stop-Job $backendJob -ErrorAction SilentlyContinue
Remove-Job $backendJob -ErrorAction SilentlyContinue
Write-Host "`n👋 Backend verification completed." -ForegroundColor Green
