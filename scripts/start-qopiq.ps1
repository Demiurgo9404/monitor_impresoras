# QOPIQ Simple Start Script
# Starts backend and frontend with basic verification

param(
    [switch]$SkipBackend,
    [switch]$SkipFrontend,
    [switch]$CreateUsers
)

Write-Host "QOPIQ Simple Start Script" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

# Paths
$BackendPath = "c:\Users\Advance\Documents\GitHub\monitor_impresoras\MonitorImpresoras\MonitorImpresoras.API"
$FrontendPath = "c:\Users\Advance\Documents\GitHub\monitor_impresoras\MonitorImpresoras\MonitorImpresoras.Web"

# Function to test if service is running
function Test-ServiceRunning {
    param($Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 5 -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

if (-not $SkipBackend) {
    Write-Host "`nStarting Backend..." -ForegroundColor Yellow
    
    if (Test-Path $BackendPath) {
        Set-Location $BackendPath
        Write-Host "Building backend..." -ForegroundColor Cyan
        
        # Start backend in background
        $backendJob = Start-Job -ScriptBlock {
            param($Path)
            Set-Location $Path
            dotnet run --urls="http://localhost:5278"
        } -ArgumentList $BackendPath
        
        # Wait for backend to start
        Write-Host "Waiting for backend to start..." -NoNewline
        $attempts = 0
        do {
            Start-Sleep -Seconds 2
            $attempts++
            Write-Host "." -NoNewline
        } while (-not (Test-ServiceRunning "http://localhost:5278/health") -and $attempts -lt 15)
        
        if (Test-ServiceRunning "http://localhost:5278/health") {
            Write-Host " Backend running at http://localhost:5278" -ForegroundColor Green
        } else {
            Write-Host " Backend failed to start" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Backend path not found: $BackendPath" -ForegroundColor Red
    }
}

if (-not $SkipFrontend) {
    Write-Host "`n🎨 Starting Frontend..." -ForegroundColor Yellow
    
    if (Test-Path $FrontendPath) {
        # Start frontend in background
        $frontendJob = Start-Job -ScriptBlock {
            param($Path)
            Set-Location $Path
            dotnet run --urls="http://localhost:5000"
        } -ArgumentList $FrontendPath
        
        # Wait for frontend to start
        Write-Host "Waiting for frontend to start..." -NoNewline
        $attempts = 0
        do {
            Start-Sleep -Seconds 2
            $attempts++
            Write-Host "." -NoNewline
        } while (-not (Test-ServiceRunning "http://localhost:5000") -and $attempts -lt 15)
        
        if (Test-ServiceRunning "http://localhost:5000") {
            Write-Host " ✅ Frontend running at http://localhost:5000" -ForegroundColor Green
        } else {
            Write-Host " ❌ Frontend failed to start" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Frontend path not found: $FrontendPath" -ForegroundColor Red
    }
}

if ($CreateUsers) {
    Write-Host "`n👥 Creating Test Users..." -ForegroundColor Yellow
    
    # Test users data
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
            $response = Invoke-WebRequest -Uri "http://localhost:5278/api/auth/register" -Method POST -Body $body -Headers $headers -TimeoutSec 10
            Write-Host " ✅ Created" -ForegroundColor Green
        }
        catch {
            if ($_.Exception.Message -like "*409*") {
                Write-Host " ⚠️ Already exists" -ForegroundColor Yellow
            } else {
                Write-Host " ❌ Failed" -ForegroundColor Red
            }
        }
    }
}

Write-Host "`n🎉 QOPIQ System Started!" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host "🌐 Frontend: http://localhost:5000" -ForegroundColor Cyan
Write-Host "🔧 Backend API: http://localhost:5278" -ForegroundColor Cyan
Write-Host "📚 Swagger Docs: http://localhost:5278/swagger" -ForegroundColor Cyan

Write-Host "`n👥 Test Users:" -ForegroundColor Yellow
Write-Host "- SuperAdmin: admin@qopiq.com / Admin123!" -ForegroundColor White
Write-Host "- CompanyAdmin: companyadmin@qopiq.com / CompanyAdmin123!" -ForegroundColor White
Write-Host "- ProjectManager: pm@qopiq.com / ProjectManager123!" -ForegroundColor White
Write-Host "- Viewer: viewer@qopiq.com / Viewer123!" -ForegroundColor White

Write-Host "`nPress Ctrl+C to stop all services..." -ForegroundColor Yellow
try {
    while ($true) {
        Start-Sleep -Seconds 1
    }
}
finally {
    Write-Host "`n🛑 Stopping services..." -ForegroundColor Red
    if ($backendJob) { Stop-Job $backendJob -ErrorAction SilentlyContinue; Remove-Job $backendJob -ErrorAction SilentlyContinue }
    if ($frontendJob) { Stop-Job $frontendJob -ErrorAction SilentlyContinue; Remove-Job $frontendJob -ErrorAction SilentlyContinue }
    Write-Host "✅ Services stopped." -ForegroundColor Green
}
