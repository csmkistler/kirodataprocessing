@echo off
REM Production Build Script for Signal Processing Visualization
REM This script builds both frontend and backend for production deployment

echo Signal Processing Visualization - Production Build
echo ===================================================
echo.

REM Check for .NET SDK
echo Checking for .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed
    exit /b 1
)
for /f %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK version %DOTNET_VERSION% found

REM Check for Node.js
echo Checking for Node.js...
node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js is not installed
    echo Please install Node.js from: https://nodejs.org/
    exit /b 1
)
for /f %%i in ('node --version') do set NODE_VERSION=%%i
echo [OK] Node.js version %NODE_VERSION% found
echo.

REM Build frontend
echo Building frontend...
cd client

REM Install dependencies if needed
if not exist "node_modules" (
    echo Installing frontend dependencies...
    call npm install
    if errorlevel 1 (
        echo ERROR: Failed to install frontend dependencies
        cd ..
        exit /b 1
    )
)

REM Build frontend
echo Compiling frontend for production...
call npm run build:prod
if errorlevel 1 (
    echo ERROR: Frontend build failed
    cd ..
    exit /b 1
)

cd ..
echo [OK] Frontend built successfully
echo.

REM Build backend
echo Building backend...
dotnet build src\SignalProcessing.Api\SignalProcessing.Api.csproj --configuration Release
if errorlevel 1 (
    echo ERROR: Backend build failed
    exit /b 1
)

echo [OK] Backend built successfully
echo.

REM Create MongoDB data directory in AppData
echo Setting up MongoDB data directory...
set APPDATA_PATH=%LOCALAPPDATA%\SignalProcessingViz\mongodb
if not exist "%APPDATA_PATH%" (
    mkdir "%APPDATA_PATH%"
    echo [OK] Created MongoDB data directory: %APPDATA_PATH%
) else (
    echo [OK] MongoDB data directory already exists: %APPDATA_PATH%
)

echo.
echo ===================================================
echo Production build completed successfully!
echo.
echo Frontend output: src\SignalProcessing.Api\wwwroot
echo Backend output: src\SignalProcessing.Api\bin\Release
echo MongoDB data: %APPDATA_PATH%
echo.
echo To run the application, use: start.bat
echo ===================================================
