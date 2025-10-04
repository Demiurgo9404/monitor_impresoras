@echo off
echo.
echo ========================================
echo   QOPIQ Quick Start - Batch Version
echo ========================================
echo.

REM Set colors for better visibility
color 0A

echo [INFO] Starting QOPIQ system verification...
echo.

REM Check if we're in the right directory
if not exist "verify-backend.ps1" (
    echo [ERROR] Scripts not found in current directory
    echo Please run this from the scripts folder
    pause
    exit /b 1
)

echo [STEP 1] Setting PowerShell execution policy...
powershell -Command "Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to set execution policy
    pause
    exit /b 1
)
echo [OK] Execution policy set successfully
echo.

echo [STEP 2] Starting backend verification...
echo ==========================================
powershell -ExecutionPolicy Bypass -File "verify-backend.ps1"
if %errorlevel% neq 0 (
    echo [WARNING] Backend verification had issues
    echo.
)

echo.
echo [STEP 3] Do you want to continue with frontend verification? (Y/N)
set /p continue="Enter your choice: "
if /i "%continue%" neq "Y" goto :skip_frontend

echo [STEP 3] Starting frontend verification...
echo ==========================================
powershell -ExecutionPolicy Bypass -File "verify-frontend.ps1"
if %errorlevel% neq 0 (
    echo [WARNING] Frontend verification had issues
    echo.
)

:skip_frontend
echo.
echo [STEP 4] Do you want to setup test users? (Y/N)
set /p setup_users="Enter your choice: "
if /i "%setup_users%" neq "Y" goto :skip_users

echo [STEP 4] Setting up test users...
echo ==================================
powershell -ExecutionPolicy Bypass -File "setup-test-users.ps1"
if %errorlevel% neq 0 (
    echo [WARNING] Test users setup had issues
    echo.
)

:skip_users
echo.
echo ========================================
echo   QOPIQ Quick Start Completed
echo ========================================
echo.
echo Next steps:
echo 1. Open browser to http://localhost:5000
echo 2. Test login with these users:
echo    - SuperAdmin: admin@qopiq.com / Admin123!
echo    - CompanyAdmin: companyadmin@qopiq.com / CompanyAdmin123!
echo    - ProjectManager: pm@qopiq.com / ProjectManager123!
echo    - Viewer: viewer@qopiq.com / Viewer123!
echo.
echo 3. Backend API available at: http://localhost:5278
echo 4. Swagger docs at: http://localhost:5278/swagger
echo.
pause
