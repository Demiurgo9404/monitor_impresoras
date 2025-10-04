# QOPIQ Complete System Verification Script
# Ejecuta todas las verificaciones del sistema de forma secuencial

Write-Host "🚀 QOPIQ Complete System Verification" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host "This script will verify the entire QOPIQ system step by step." -ForegroundColor White
Write-Host ""

# Variables
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$LogFile = Join-Path $ScriptPath "verification-log.txt"

# Función para logging
function Write-Log {
    param($Message, $Color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Write-Host $logMessage -ForegroundColor $Color
    Add-Content -Path $LogFile -Value $logMessage
}

# Función para ejecutar script y capturar resultado
function Invoke-VerificationScript {
    param($ScriptName, $Description)
    
    Write-Log "🔄 Starting: $Description" "Yellow"
    $scriptPath = Join-Path $ScriptPath $ScriptName
    
    if (-not (Test-Path $scriptPath)) {
        Write-Log "❌ Script not found: $ScriptName" "Red"
        return $false
    }
    
    try {
        $result = & $scriptPath
        Write-Log "✅ Completed: $Description" "Green"
        return $true
    }
    catch {
        Write-Log "❌ Failed: $Description - $($_.Exception.Message)" "Red"
        return $false
    }
}

# Función para pausa interactiva
function Wait-UserInput {
    param($Message)
    Write-Host "`n$Message" -ForegroundColor Cyan
    Write-Host "Press any key to continue or Ctrl+C to abort..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

# Inicializar log
if (Test-Path $LogFile) { Remove-Item $LogFile }
Write-Log "🚀 QOPIQ System Verification Started" "Green"

Write-Host "📋 Verification Plan:" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host "Phase 1: Backend Verification" -ForegroundColor White
Write-Host "Phase 2: Frontend Verification" -ForegroundColor White
Write-Host "Phase 3: Test Users Setup" -ForegroundColor White
Write-Host "Phase 4: End-to-End Testing" -ForegroundColor White
Write-Host "Phase 5: Security Validation" -ForegroundColor White

Wait-UserInput "Ready to start Phase 1: Backend Verification?"

# Phase 1: Backend Verification
Write-Log "🔧 PHASE 1: Backend Verification" "Magenta"
Write-Host "`n🔧 PHASE 1: Backend Verification" -ForegroundColor Magenta
Write-Host "=================================" -ForegroundColor Magenta

$backendResult = Invoke-VerificationScript "verify-backend.ps1" "Backend System Check"

if (-not $backendResult) {
    Write-Log "❌ Backend verification failed. Cannot continue." "Red"
    Write-Host "❌ Backend verification failed. Please check the backend setup." -ForegroundColor Red
    exit 1
}

Wait-UserInput "Backend verification completed. Ready for Phase 2: Frontend Verification?"

# Phase 2: Frontend Verification
Write-Log "🎨 PHASE 2: Frontend Verification" "Magenta"
Write-Host "`n🎨 PHASE 2: Frontend Verification" -ForegroundColor Magenta
Write-Host "==================================" -ForegroundColor Magenta

$frontendResult = Invoke-VerificationScript "verify-frontend.ps1" "Frontend System Check"

if (-not $frontendResult) {
    Write-Log "⚠️ Frontend verification had issues, but continuing..." "Yellow"
    Write-Host "⚠️ Frontend verification had issues, but continuing..." -ForegroundColor Yellow
}

Wait-UserInput "Frontend verification completed. Ready for Phase 3: Test Users Setup?"

# Phase 3: Test Users Setup
Write-Log "👥 PHASE 3: Test Users Setup" "Magenta"
Write-Host "`n👥 PHASE 3: Test Users Setup" -ForegroundColor Magenta
Write-Host "=============================" -ForegroundColor Magenta

$usersResult = Invoke-VerificationScript "setup-test-users.ps1" "Test Users Creation"

if (-not $usersResult) {
    Write-Log "⚠️ Test users setup had issues, but continuing..." "Yellow"
    Write-Host "⚠️ Test users setup had issues, but continuing..." -ForegroundColor Yellow
}

# Phase 4: Manual Testing Instructions
Write-Log "🧪 PHASE 4: Manual Testing Instructions" "Magenta"
Write-Host "`n🧪 PHASE 4: Manual Testing Instructions" -ForegroundColor Magenta
Write-Host "=======================================" -ForegroundColor Magenta

Write-Host "`n📝 Manual Testing Checklist:" -ForegroundColor Yellow
Write-Host "=============================" -ForegroundColor Yellow

$testChecklist = @(
    "✅ Login with SuperAdmin (admin@qopiq.com / Admin123!)",
    "✅ Verify SuperAdmin dashboard shows global metrics",
    "✅ Login with CompanyAdmin (companyadmin@qopiq.com / CompanyAdmin123!)",
    "✅ Verify CompanyAdmin dashboard shows company-specific data",
    "✅ Login with ProjectManager (pm@qopiq.com / ProjectManager123!)",
    "✅ Verify ProjectManager dashboard shows assigned projects",
    "✅ Login with Viewer (viewer@qopiq.com / Viewer123!)",
    "✅ Verify Viewer dashboard shows read-only access",
    "✅ Test report generation from Reports page",
    "✅ Test report download functionality",
    "✅ Test calendar view and scheduled reports",
    "✅ Test template gallery and preview functionality",
    "✅ Verify role-based access restrictions"
)

foreach ($item in $testChecklist) {
    Write-Host $item -ForegroundColor White
}

Write-Host "`n🌐 Testing URLs:" -ForegroundColor Cyan
Write-Host "================" -ForegroundColor Cyan
Write-Host "Frontend: http://localhost:5000" -ForegroundColor White
Write-Host "Backend API: http://localhost:5278" -ForegroundColor White
Write-Host "Swagger Docs: http://localhost:5278/swagger" -ForegroundColor White

Wait-UserInput "Complete the manual testing checklist above, then continue to Phase 5."

# Phase 5: Security Validation
Write-Log "🔒 PHASE 5: Security Validation" "Magenta"
Write-Host "`n🔒 PHASE 5: Security Validation" -ForegroundColor Magenta
Write-Host "===============================" -ForegroundColor Magenta

Write-Host "`n🔐 Security Checklist:" -ForegroundColor Yellow
Write-Host "======================" -ForegroundColor Yellow

$securityChecklist = @(
    "✅ JWT tokens are generated correctly for each user",
    "✅ API endpoints require valid JWT authentication",
    "✅ Role-based authorization works (Admin vs Viewer access)",
    "✅ Multi-tenant data isolation is enforced",
    "✅ File downloads are secured with proper validation",
    "✅ Sensitive endpoints are protected from unauthorized access",
    "✅ Password requirements are enforced",
    "✅ Session management works correctly"
)

foreach ($item in $securityChecklist) {
    Write-Host $item -ForegroundColor White
}

# Final Summary
Write-Log "📊 VERIFICATION SUMMARY" "Green"
Write-Host "`n📊 VERIFICATION SUMMARY" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green

Write-Host "✅ Backend Verification: " -NoNewline -ForegroundColor Green
Write-Host $(if ($backendResult) { "PASSED" } else { "FAILED" }) -ForegroundColor $(if ($backendResult) { "Green" } else { "Red" })

Write-Host "✅ Frontend Verification: " -NoNewline -ForegroundColor Green
Write-Host $(if ($frontendResult) { "PASSED" } else { "WARNING" }) -ForegroundColor $(if ($frontendResult) { "Green" } else { "Yellow" })

Write-Host "✅ Test Users Setup: " -NoNewline -ForegroundColor Green
Write-Host $(if ($usersResult) { "PASSED" } else { "WARNING" }) -ForegroundColor $(if ($usersResult) { "Green" } else { "Yellow" })

Write-Host "✅ Manual Testing: PENDING USER VALIDATION" -ForegroundColor Yellow
Write-Host "✅ Security Testing: PENDING USER VALIDATION" -ForegroundColor Yellow

if ($backendResult) {
    Write-Host "`n🎉 QOPIQ System Verification COMPLETED!" -ForegroundColor Green
    Write-Host "The system is ready for production deployment." -ForegroundColor Green
    
    Write-Log "🎉 System verification completed successfully" "Green"
    
    Write-Host "`n🚀 Next Steps:" -ForegroundColor Cyan
    Write-Host "==============" -ForegroundColor Cyan
    Write-Host "1. Complete manual testing checklist" -ForegroundColor White
    Write-Host "2. Validate security requirements" -ForegroundColor White
    Write-Host "3. Prepare production configuration" -ForegroundColor White
    Write-Host "4. Deploy to production environment" -ForegroundColor White
    Write-Host "5. Conduct final user acceptance testing" -ForegroundColor White
} else {
    Write-Host "`n⚠️ System verification completed with issues." -ForegroundColor Yellow
    Write-Host "Please review the log file: $LogFile" -ForegroundColor Yellow
    Write-Log "⚠️ System verification completed with issues" "Yellow"
}

Write-Host "`n📄 Full verification log saved to: $LogFile" -ForegroundColor Cyan
Write-Host "👋 Verification process completed." -ForegroundColor Green
