# QOPIQ Frontend Verification Script
# Verifica que el frontend esté completamente funcional y conectado al backend

Write-Host "🎨 QOPIQ Frontend Verification Script" -ForegroundColor Blue
Write-Host "======================================" -ForegroundColor Blue

# Variables
$FrontendPath = "c:\Users\Advance\Documents\GitHub\monitor_impresoras\MonitorImpresoras\MonitorImpresoras.Web"
$FrontendUrl = "http://localhost:5000"
$BackendUrl = "http://localhost:5278"

# Función para verificar si el servicio está corriendo
function Test-ServiceRunning {
    param($Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 10
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

# Función para verificar archivos críticos
function Test-CriticalFiles {
    $criticalFiles = @(
        "Pages\Index.razor",
        "Pages\Reports.razor", 
        "Pages\ScheduledReports.razor",
        "Pages\Templates.razor",
        "Pages\Admin\SuperAdminDashboard.razor",
        "Pages\Admin\CompanyAdminDashboard.razor",
        "Pages\Projects\ProjectManagerDashboard.razor",
        "Pages\Viewer\ViewerDashboard.razor",
        "Services\ApiService.cs",
        "Services\AuthService.cs",
        "Services\TenantService.cs",
        "wwwroot\css\site.css",
        "wwwroot\js\site.js",
        "wwwroot\js\calendar.js"
    )
    
    Write-Host "`n📁 Checking Critical Frontend Files..." -ForegroundColor Yellow
    $missingFiles = @()
    
    foreach ($file in $criticalFiles) {
        $fullPath = Join-Path $FrontendPath $file
        if (Test-Path $fullPath) {
            Write-Host "✅ $file" -ForegroundColor Green
        } else {
            Write-Host "❌ $file (MISSING)" -ForegroundColor Red
            $missingFiles += $file
        }
    }
    
    return $missingFiles.Count -eq 0
}

Write-Host "`n📁 Step 1: Checking Frontend Directory..." -ForegroundColor Yellow
if (Test-Path $FrontendPath) {
    Write-Host "✅ Frontend directory found" -ForegroundColor Green
    Set-Location $FrontendPath
} else {
    Write-Host "❌ Frontend directory not found: $FrontendPath" -ForegroundColor Red
    exit 1
}

# Verificar archivos críticos
$filesOk = Test-CriticalFiles
if (-not $filesOk) {
    Write-Host "`n⚠️ Some critical files are missing. Frontend may not work properly." -ForegroundColor Yellow
}

Write-Host "`n🔨 Step 2: Building Frontend..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build --configuration Release --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Frontend build successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Frontend build failed:" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Build error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🔗 Step 3: Checking Backend Connection..." -ForegroundColor Yellow
if (Test-ServiceRunning $BackendUrl) {
    Write-Host "✅ Backend is accessible at $BackendUrl" -ForegroundColor Green
} else {
    Write-Host "⚠️ Backend is not running at $BackendUrl" -ForegroundColor Yellow
    Write-Host "Please start the backend first using verify-backend.ps1" -ForegroundColor Yellow
}

Write-Host "`n🌐 Step 4: Starting Frontend Service..." -ForegroundColor Yellow
Write-Host "Starting frontend at $FrontendUrl..." -ForegroundColor Cyan

# Start frontend in background
$frontendJob = Start-Job -ScriptBlock {
    param($Path)
    Set-Location $Path
    dotnet run --urls="http://localhost:5000"
} -ArgumentList $FrontendPath

# Wait for service to start
Write-Host "Waiting for frontend to start..." -NoNewline
$attempts = 0
$maxAttempts = 30
do {
    Start-Sleep -Seconds 2
    $attempts++
    Write-Host "." -NoNewline
} while (-not (Test-ServiceRunning $FrontendUrl) -and $attempts -lt $maxAttempts)

if (Test-ServiceRunning $FrontendUrl) {
    Write-Host " ✅ Frontend is running!" -ForegroundColor Green
} else {
    Write-Host " ❌ Frontend failed to start within timeout" -ForegroundColor Red
    Stop-Job $frontendJob
    Remove-Job $frontendJob
    exit 1
}

Write-Host "`n🔍 Step 5: Testing Frontend Pages..." -ForegroundColor Yellow

# Test critical pages
$pages = @(
    @{Page="/"; Description="Home Page"},
    @{Page="/reports"; Description="Reports Page"},
    @{Page="/scheduled-reports"; Description="Scheduled Reports Page"},
    @{Page="/templates"; Description="Templates Page"}
)

$successCount = 0
foreach ($page in $pages) {
    try {
        Write-Host "Testing $($page.Description)..." -NoNewline
        $response = Invoke-WebRequest -Uri "$FrontendUrl$($page.Page)" -Method GET -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host " ✅ OK" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host " ❌ FAIL (Status: $($response.StatusCode))" -ForegroundColor Red
        }
    }
    catch {
        Write-Host " ❌ FAIL (Error: $($_.Exception.Message))" -ForegroundColor Red
    }
}

Write-Host "`n🎨 Step 6: Checking UI Components..." -ForegroundColor Yellow

# Check if CSS and JS files are accessible
$assets = @(
    @{Asset="/css/site.css"; Description="Main CSS"},
    @{Asset="/js/site.js"; Description="Main JavaScript"},
    @{Asset="/js/calendar.js"; Description="Calendar JavaScript"}
)

$assetCount = 0
foreach ($asset in $assets) {
    try {
        Write-Host "Testing $($asset.Description)..." -NoNewline
        $response = Invoke-WebRequest -Uri "$FrontendUrl$($asset.Asset)" -Method GET -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host " ✅ OK" -ForegroundColor Green
            $assetCount++
        } else {
            Write-Host " ❌ FAIL" -ForegroundColor Red
        }
    }
    catch {
        Write-Host " ❌ FAIL" -ForegroundColor Red
    }
}

Write-Host "`n📊 Results Summary:" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow
Write-Host "✅ Pages tested: $($pages.Count)" -ForegroundColor Green
Write-Host "✅ Pages working: $successCount" -ForegroundColor Green
Write-Host "✅ Assets tested: $($assets.Count)" -ForegroundColor Green
Write-Host "✅ Assets working: $assetCount" -ForegroundColor Green

$totalTests = $pages.Count + $assets.Count
$totalSuccess = $successCount + $assetCount

if ($totalSuccess -eq $totalTests) {
    Write-Host "`n🎉 Frontend Verification SUCCESSFUL!" -ForegroundColor Green
    Write-Host "Frontend is ready for user testing." -ForegroundColor Green
} else {
    Write-Host "`n⚠️ Frontend Verification COMPLETED WITH WARNINGS" -ForegroundColor Yellow
    Write-Host "Some components may need attention." -ForegroundColor Yellow
}

Write-Host "`nFrontend URLs:" -ForegroundColor Cyan
Write-Host "- Main App: $FrontendUrl" -ForegroundColor White
Write-Host "- Reports: $FrontendUrl/reports" -ForegroundColor White
Write-Host "- Calendar: $FrontendUrl/scheduled-reports" -ForegroundColor White
Write-Host "- Templates: $FrontendUrl/templates" -ForegroundColor White

Write-Host "`n🔧 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Test user authentication with different roles" -ForegroundColor White
Write-Host "2. Verify dashboard functionality per role" -ForegroundColor White
Write-Host "3. Test report generation and download" -ForegroundColor White
Write-Host "4. Validate calendar and scheduling features" -ForegroundColor White

Write-Host "`nPress any key to stop frontend or Ctrl+C to keep it running..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Cleanup
Stop-Job $frontendJob -ErrorAction SilentlyContinue
Remove-Job $frontendJob -ErrorAction SilentlyContinue
Write-Host "`n👋 Frontend verification completed." -ForegroundColor Blue
