# QOPIQ Test Users Setup Script
# Crea usuarios de prueba para cada rol del sistema

Write-Host "👥 QOPIQ Test Users Setup Script" -ForegroundColor Magenta
Write-Host "==================================" -ForegroundColor Magenta

# Variables
$BackendUrl = "http://localhost:5278"

# Usuarios de prueba
$testUsers = @(
    @{
        Email = "admin@qopiq.com"
        Password = "Admin123!"
        FullName = "Super Administrator"
        Role = "SuperAdmin"
        CompanyName = "QOPIQ Platform"
    },
    @{
        Email = "companyadmin@qopiq.com"
        Password = "CompanyAdmin123!"
        FullName = "Company Administrator"
        Role = "CompanyAdmin"
        CompanyName = "Demo Company Ltd"
    },
    @{
        Email = "pm@qopiq.com"
        Password = "ProjectManager123!"
        FullName = "Project Manager"
        Role = "ProjectManager"
        CompanyName = "Demo Company Ltd"
    },
    @{
        Email = "viewer@qopiq.com"
        Password = "Viewer123!"
        FullName = "Report Viewer"
        Role = "Viewer"
        CompanyName = "Demo Company Ltd"
    }
)

# Función para verificar si el backend está corriendo
function Test-BackendRunning {
    try {
        $response = Invoke-WebRequest -Uri "$BackendUrl/health" -Method GET -TimeoutSec 5
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

# Función para crear usuario
function Create-TestUser {
    param($User)
    
    $body = @{
        email = $User.Email
        password = $User.Password
        fullName = $User.FullName
        role = $User.Role
        companyName = $User.CompanyName
    } | ConvertTo-Json
    
    try {
        Write-Host "Creating user: $($User.FullName) ($($User.Role))..." -NoNewline
        
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-WebRequest -Uri "$BackendUrl/api/auth/register" -Method POST -Body $body -Headers $headers -TimeoutSec 10
        
        if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 201) {
            Write-Host " ✅ Created" -ForegroundColor Green
            return $true
        } else {
            Write-Host " ❌ Failed (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    }
    catch {
        if ($_.Exception.Message -like "*409*" -or $_.Exception.Message -like "*Conflict*") {
            Write-Host " ⚠️ Already exists" -ForegroundColor Yellow
            return $true
        } else {
            Write-Host " ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
}

# Función para probar login
function Test-UserLogin {
    param($User)
    
    $body = @{
        email = $User.Email
        password = $User.Password
    } | ConvertTo-Json
    
    try {
        Write-Host "Testing login for $($User.Email)..." -NoNewline
        
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-WebRequest -Uri "$BackendUrl/api/auth/login" -Method POST -Body $body -Headers $headers -TimeoutSec 10
        
        if ($response.StatusCode -eq 200) {
            $responseData = $response.Content | ConvertFrom-Json
            if ($responseData.token) {
                Write-Host " ✅ Login successful" -ForegroundColor Green
                return @{Success = $true; Token = $responseData.token}
            } else {
                Write-Host " ❌ No token received" -ForegroundColor Red
                return @{Success = $false; Token = $null}
            }
        } else {
            Write-Host " ❌ Failed (Status: $($response.StatusCode))" -ForegroundColor Red
            return @{Success = $false; Token = $null}
        }
    }
    catch {
        Write-Host " ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        return @{Success = $false; Token = $null}
    }
}

Write-Host "`n🔍 Step 1: Checking Backend Availability..." -ForegroundColor Yellow
if (-not (Test-BackendRunning)) {
    Write-Host "❌ Backend is not running at $BackendUrl" -ForegroundColor Red
    Write-Host "Please start the backend first using verify-backend.ps1" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Backend is accessible" -ForegroundColor Green

Write-Host "`n👤 Step 2: Creating Test Users..." -ForegroundColor Yellow
$createdCount = 0
foreach ($user in $testUsers) {
    if (Create-TestUser $user) {
        $createdCount++
    }
}

Write-Host "`n🔐 Step 3: Testing User Authentication..." -ForegroundColor Yellow
$loginCount = 0
$userTokens = @{}

foreach ($user in $testUsers) {
    $loginResult = Test-UserLogin $user
    if ($loginResult.Success) {
        $loginCount++
        $userTokens[$user.Role] = $loginResult.Token
    }
}

Write-Host "`n🏢 Step 4: Creating Sample Company Data..." -ForegroundColor Yellow
if ($userTokens.ContainsKey("SuperAdmin")) {
    try {
        Write-Host "Creating sample company..." -NoNewline
        
        $companyBody = @{
            name = "Demo Company Ltd"
            domain = "demo.qopiq.com"
            contactEmail = "contact@demo.qopiq.com"
            isActive = $true
        } | ConvertTo-Json
        
        $headers = @{
            "Content-Type" = "application/json"
            "Authorization" = "Bearer $($userTokens.SuperAdmin)"
        }
        
        $response = Invoke-WebRequest -Uri "$BackendUrl/api/admin/companies" -Method POST -Body $companyBody -Headers $headers -TimeoutSec 10
        Write-Host " ✅ Created" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Message -like "*409*") {
            Write-Host " ⚠️ Already exists" -ForegroundColor Yellow
        } else {
            Write-Host " ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`n📊 Results Summary:" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow
Write-Host "✅ Users to create: $($testUsers.Count)" -ForegroundColor Green
Write-Host "✅ Users created/verified: $createdCount" -ForegroundColor Green
Write-Host "✅ Successful logins: $loginCount" -ForegroundColor Green

if ($loginCount -eq $testUsers.Count) {
    Write-Host "`n🎉 Test Users Setup SUCCESSFUL!" -ForegroundColor Green
    Write-Host "All test users are ready for authentication testing." -ForegroundColor Green
} else {
    Write-Host "`n⚠️ Test Users Setup COMPLETED WITH WARNINGS" -ForegroundColor Yellow
    Write-Host "Some users may need manual verification." -ForegroundColor Yellow
}

Write-Host "`n👥 Test User Credentials:" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
foreach ($user in $testUsers) {
    Write-Host "🔹 $($user.Role):" -ForegroundColor White
    Write-Host "   Email: $($user.Email)" -ForegroundColor Gray
    Write-Host "   Password: $($user.Password)" -ForegroundColor Gray
    Write-Host "   Company: $($user.CompanyName)" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "🔧 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Test login with each user in the frontend" -ForegroundColor White
Write-Host "2. Verify role-based dashboard access" -ForegroundColor White
Write-Host "3. Test permissions for each role" -ForegroundColor White
Write-Host "4. Validate multi-tenant functionality" -ForegroundColor White

Write-Host "`n✨ Users are ready for testing!" -ForegroundColor Green
