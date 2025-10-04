# QOPIQ Simple Start Script - Clean Version
Write-Host "QOPIQ System Startup" -ForegroundColor Green
Write-Host "====================" -ForegroundColor Green

# Paths
$BackendPath = "c:\Users\Advance\Documents\GitHub\monitor_impresoras\MonitorImpresoras\MonitorImpresoras.API"
$FrontendPath = "c:\Users\Advance\Documents\GitHub\monitor_impresoras\MonitorImpresoras\MonitorImpresoras.Web"

Write-Host "`nStep 1: Starting Backend..." -ForegroundColor Yellow

if (Test-Path $BackendPath) {
    Set-Location $BackendPath
    Write-Host "Backend path found. Starting service..." -ForegroundColor Cyan
    
    # Start backend
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --urls=http://localhost:5278" -WindowStyle Normal
    
    Write-Host "Backend starting at http://localhost:5278" -ForegroundColor Green
    Write-Host "Waiting 10 seconds for backend to initialize..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
} else {
    Write-Host "ERROR: Backend path not found: $BackendPath" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 2: Starting Frontend..." -ForegroundColor Yellow

if (Test-Path $FrontendPath) {
    Set-Location $FrontendPath
    Write-Host "Frontend path found. Starting service..." -ForegroundColor Cyan
    
    # Start frontend
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --urls=http://localhost:5000" -WindowStyle Normal
    
    Write-Host "Frontend starting at http://localhost:5000" -ForegroundColor Green
    Write-Host "Waiting 10 seconds for frontend to initialize..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
} else {
    Write-Host "ERROR: Frontend path not found: $FrontendPath" -ForegroundColor Red
}

Write-Host "`nStep 3: Creating Test Users..." -ForegroundColor Yellow

# Wait a bit more for backend to be ready
Start-Sleep -Seconds 5

# Test users
$users = @(
    @{Email="admin@qopiq.com"; Password="Admin123!"; FullName="Super Administrator"; Role="SuperAdmin"},
    @{Email="companyadmin@qopiq.com"; Password="CompanyAdmin123!"; FullName="Company Administrator"; Role="CompanyAdmin"},
    @{Email="pm@qopiq.com"; Password="ProjectManager123!"; FullName="Project Manager"; Role="ProjectManager"},
    @{Email="viewer@qopiq.com"; Password="Viewer123!"; FullName="Report Viewer"; Role="Viewer"}
)

foreach ($user in $users) {
    try {
        $body = @{
            email = $user.Email
            password = $user.Password
            fullName = $user.FullName
            role = $user.Role
        } | ConvertTo-Json
        
        $headers = @{"Content-Type" = "application/json"}
        
        Write-Host "Creating user: $($user.FullName)..." -NoNewline
        Invoke-WebRequest -Uri "http://localhost:5278/api/auth/register" -Method POST -Body $body -Headers $headers -TimeoutSec 10 | Out-Null
        Write-Host " SUCCESS" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Message -like "*409*") {
            Write-Host " ALREADY EXISTS" -ForegroundColor Yellow
        } else {
            Write-Host " FAILED" -ForegroundColor Red
        }
    }
}

Write-Host "`n" -NoNewline
Write-Host "QOPIQ SYSTEM READY!" -ForegroundColor Green -BackgroundColor Black
Write-Host "===================" -ForegroundColor Green

Write-Host "`nAccess URLs:" -ForegroundColor Cyan
Write-Host "- Frontend Application: http://localhost:5000" -ForegroundColor White
Write-Host "- Backend API: http://localhost:5278" -ForegroundColor White
Write-Host "- API Documentation: http://localhost:5278/swagger" -ForegroundColor White

Write-Host "`nTest User Credentials:" -ForegroundColor Yellow
Write-Host "- SuperAdmin: admin@qopiq.com / Admin123!" -ForegroundColor White
Write-Host "- CompanyAdmin: companyadmin@qopiq.com / CompanyAdmin123!" -ForegroundColor White
Write-Host "- ProjectManager: pm@qopiq.com / ProjectManager123!" -ForegroundColor White
Write-Host "- Viewer: viewer@qopiq.com / Viewer123!" -ForegroundColor White

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. Open browser to http://localhost:5000" -ForegroundColor White
Write-Host "2. Login with any of the test users above" -ForegroundColor White
Write-Host "3. Explore the different dashboards per role" -ForegroundColor White
Write-Host "4. Test report generation and download" -ForegroundColor White

Write-Host "`nNote: Backend and Frontend are running in separate PowerShell windows." -ForegroundColor Yellow
Write-Host "Close those windows to stop the services." -ForegroundColor Yellow

Write-Host "`nPress any key to exit this setup script..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
