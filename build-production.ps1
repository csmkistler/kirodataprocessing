# Production Build Script for Signal Processing Visualization
# This script builds both frontend and backend for production deployment

Write-Host "Signal Processing Visualization - Production Build" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""

# Check for .NET SDK
Write-Host "Checking for .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command not found"
    }
    Write-Host "✓ .NET SDK version $dotnetVersion found" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK is not installed" -ForegroundColor Red
    exit 1
}

# Check for Node.js
Write-Host "Checking for Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "node command not found"
    }
    Write-Host "✓ Node.js version $nodeVersion found" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Node.js is not installed" -ForegroundColor Red
    Write-Host "Please install Node.js from: https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Build frontend
Write-Host "Building frontend..." -ForegroundColor Yellow
Push-Location client

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install frontend dependencies" -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

# Build frontend
Write-Host "Compiling frontend for production..." -ForegroundColor Yellow
npm run build:prod
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Frontend build failed" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location
Write-Host "✓ Frontend built successfully" -ForegroundColor Green
Write-Host ""

# Build backend
Write-Host "Building backend..." -ForegroundColor Yellow
dotnet build src/SignalProcessing.Api/SignalProcessing.Api.csproj --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Backend build failed" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Backend built successfully" -ForegroundColor Green
Write-Host ""

# Create MongoDB data directory in AppData
Write-Host "Setting up MongoDB data directory..." -ForegroundColor Yellow
$appDataPath = [Environment]::GetFolderPath('LocalApplicationData')
$mongoDataPath = Join-Path $appDataPath "SignalProcessingViz\mongodb"

if (-not (Test-Path $mongoDataPath)) {
    New-Item -ItemType Directory -Path $mongoDataPath -Force | Out-Null
    Write-Host "✓ Created MongoDB data directory: $mongoDataPath" -ForegroundColor Green
} else {
    Write-Host "✓ MongoDB data directory already exists: $mongoDataPath" -ForegroundColor Green
}

Write-Host ""
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Production build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Frontend output: src/SignalProcessing.Api/wwwroot" -ForegroundColor Cyan
Write-Host "Backend output: src/SignalProcessing.Api/bin/Release" -ForegroundColor Cyan
Write-Host "MongoDB data: $mongoDataPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application, use: .\start.ps1" -ForegroundColor Yellow
Write-Host "===================================================" -ForegroundColor Cyan
