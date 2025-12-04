@echo off
REM Signal Processing Visualization Application Startup Script
REM This script checks dependencies, starts required services, and launches the application

echo Signal Processing Visualization - Startup Script
echo =================================================
echo.

REM Check for .NET 10 SDK installation
echo Checking for .NET 10 SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 10 SDK is not installed
    echo Please install .NET 10 SDK from: https://dotnet.microsoft.com/download/dotnet/10.0
    exit /b 1
)

for /f "tokens=1 delims=." %%a in ('dotnet --version') do set DOTNET_MAJOR=%%a
if %DOTNET_MAJOR% LSS 10 (
    echo ERROR: .NET 10 SDK is required but an older version is installed
    echo Please install .NET 10 SDK from: https://dotnet.microsoft.com/download/dotnet/10.0
    exit /b 1
)

for /f %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK version %DOTNET_VERSION% found
echo.

REM Check for MongoDB service
echo Checking for MongoDB service...
sc query MongoDB >nul 2>&1
if errorlevel 1 (
    echo WARNING: MongoDB service is not installed
    echo The application requires MongoDB to store signal metadata and events.
    echo Please install MongoDB Community Edition from: https://www.mongodb.com/try/download/community
    echo.
    set /p CONTINUE="Do you want to continue without MongoDB? (y/N): "
    if /i not "%CONTINUE%"=="y" exit /b 1
) else (
    REM Check if MongoDB is running
    sc query MongoDB | find "RUNNING" >nul
    if errorlevel 1 (
        echo Starting MongoDB service...
        net start MongoDB >nul 2>&1
        if errorlevel 1 (
            echo ERROR: Failed to start MongoDB service
            echo Please start MongoDB manually or check service permissions
            exit /b 1
        )
        timeout /t 2 /nobreak >nul
        echo [OK] MongoDB service started successfully
    ) else (
        echo [OK] MongoDB service is already running
    )
)

echo.
echo Note: InfluxDB will run in embedded mode within the application
echo.

REM Start backend server
echo Starting backend server...
echo This may take a moment on first run...

start /B dotnet run --project src\SignalProcessing.Api\SignalProcessing.Api.csproj > server-output.log 2> server-error.log
if errorlevel 1 (
    echo ERROR: Failed to start backend server
    exit /b 1
)

echo [OK] Backend server process started
echo.

REM Wait for server to be ready
echo Waiting for server to be ready...
set ATTEMPTS=0
set MAX_ATTEMPTS=30

:WAIT_LOOP
if %ATTEMPTS% GEQ %MAX_ATTEMPTS% goto SERVER_TIMEOUT

timeout /t 1 /nobreak >nul
set /a ATTEMPTS+=1

REM Try to connect to health endpoint
curl -s -o nul -w "%%{http_code}" http://localhost:5000/api/health 2>nul | findstr "200" >nul
if errorlevel 1 (
    echo|set /p="."
    goto WAIT_LOOP
)

echo.
echo [OK] Server is ready and responding
goto SERVER_READY

:SERVER_TIMEOUT
echo.
echo ERROR: Server failed to start within 30 seconds
echo Check server-output.log and server-error.log for details
REM Try to kill any dotnet processes we started
taskkill /F /FI "IMAGENAME eq dotnet.exe" /FI "WINDOWTITLE eq *SignalProcessing.Api*" >nul 2>&1
exit /b 1

:SERVER_READY
REM Open browser
echo.
echo Opening browser to http://localhost:5000...
timeout /t 1 /nobreak >nul
start http://localhost:5000

echo.
echo =================================================
echo Application is running!
echo URL: http://localhost:5000
echo Press Ctrl+C to stop the server
echo =================================================
echo.

REM Wait for user to close (keep window open)
pause

REM Cleanup - try to stop the server
echo.
echo Stopping server...
taskkill /F /FI "IMAGENAME eq dotnet.exe" /FI "WINDOWTITLE eq *SignalProcessing.Api*" >nul 2>&1
echo Server stopped. Goodbye!
