# Signal Processing Visualization Application Startup Script
# This script checks dependencies, starts required services, and launches the application

Write-Host "Signal Processing Visualization - Startup Script" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Check for .NET 10 SDK installation
Write-Host "Checking for .NET 10 SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command not found"
    }
    
    $majorVersion = [int]($dotnetVersion.Split('.')[0])
    if ($majorVersion -lt 10) {
        Write-Host "ERROR: .NET 10 SDK is required but version $dotnetVersion is installed" -ForegroundColor Red
        Write-Host "Please install .NET 10 SDK from: https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "✓ .NET SDK version $dotnetVersion found" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET 10 SDK is not installed" -ForegroundColor Red
    Write-Host "Please install .NET 10 SDK from: https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Check for MongoDB service
Write-Host "Checking for MongoDB service..." -ForegroundColor Yellow
$mongoService = Get-Service -Name "MongoDB" -ErrorAction SilentlyContinue

if ($null -eq $mongoService) {
    Write-Host "WARNING: MongoDB service is not installed" -ForegroundColor Red
    Write-Host "The application requires MongoDB to store signal metadata and events." -ForegroundColor Yellow
    Write-Host "Please install MongoDB Community Edition from: https://www.mongodb.com/try/download/community" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Do you want to continue without MongoDB? (y/N)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
} else {
    # Start MongoDB service if not running
    if ($mongoService.Status -ne "Running") {
        Write-Host "Starting MongoDB service..." -ForegroundColor Yellow
        try {
            Start-Service -Name "MongoDB" -ErrorAction Stop
            Start-Sleep -Seconds 2
            Write-Host "✓ MongoDB service started successfully" -ForegroundColor Green
        } catch {
            Write-Host "ERROR: Failed to start MongoDB service" -ForegroundColor Red
            Write-Host "Please start MongoDB manually or check service permissions" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "✓ MongoDB service is already running" -ForegroundColor Green
    }
}

Write-Host ""

# Check if InfluxDB is available (optional, as it can run embedded)
Write-Host "Note: InfluxDB will run in embedded mode within the application" -ForegroundColor Cyan

Write-Host ""

# Start backend server
Write-Host "Starting backend server..." -ForegroundColor Yellow
Write-Host "This may take a moment on first run..." -ForegroundColor Gray

$serverProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj" `
    -PassThru `
    -NoNewWindow `
    -RedirectStandardOutput "server-output.log" `
    -RedirectStandardError "server-error.log"

if ($null -eq $serverProcess) {
    Write-Host "ERROR: Failed to start backend server" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Backend server process started (PID: $($serverProcess.Id))" -ForegroundColor Green

# Wait for server to be ready
Write-Host ""
Write-Host "Waiting for server to be ready..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$serverReady = $false

while ($attempt -lt $maxAttempts) {
    Start-Sleep -Seconds 1
    $attempt++
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/health" -Method GET -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $serverReady = $true
            break
        }
    } catch {
        # Server not ready yet, continue waiting
        Write-Host "." -NoNewline -ForegroundColor Gray
    }
}

Write-Host ""

if (-not $serverReady) {
    Write-Host "ERROR: Server failed to start within 30 seconds" -ForegroundColor Red
    Write-Host "Check server-output.log and server-error.log for details" -ForegroundColor Yellow
    Stop-Process -Id $serverProcess.Id -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host "✓ Server is ready and responding" -ForegroundColor Green

# Open browser
Write-Host ""
Write-Host "Opening browser to http://localhost:5000..." -ForegroundColor Yellow
Start-Sleep -Seconds 1
Start-Process "http://localhost:5000"

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Application is running!" -ForegroundColor Green
Write-Host "URL: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Wait for user to close
try {
    Wait-Process -Id $serverProcess.Id
} catch {
    # Process was terminated
}

Write-Host ""
Write-Host "Server stopped. Goodbye!" -ForegroundColor Cyan
