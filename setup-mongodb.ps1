# MongoDB Setup Script for Signal Processing Visualization
# This script configures MongoDB data directory in user's AppData

Write-Host "MongoDB Setup for Signal Processing Visualization" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Get AppData path
$appDataPath = [Environment]::GetFolderPath('LocalApplicationData')
$appBasePath = Join-Path $appDataPath "SignalProcessingViz"
$mongoDataPath = Join-Path $appBasePath "mongodb"
$mongoLogPath = Join-Path $appBasePath "logs"

Write-Host "Setting up MongoDB directories..." -ForegroundColor Yellow
Write-Host "Base path: $appBasePath" -ForegroundColor Gray

# Create directories
if (-not (Test-Path $mongoDataPath)) {
    New-Item -ItemType Directory -Path $mongoDataPath -Force | Out-Null
    Write-Host "✓ Created MongoDB data directory: $mongoDataPath" -ForegroundColor Green
} else {
    Write-Host "✓ MongoDB data directory already exists" -ForegroundColor Green
}

if (-not (Test-Path $mongoLogPath)) {
    New-Item -ItemType Directory -Path $mongoLogPath -Force | Out-Null
    Write-Host "✓ Created logs directory: $mongoLogPath" -ForegroundColor Green
} else {
    Write-Host "✓ Logs directory already exists" -ForegroundColor Green
}

Write-Host ""
Write-Host "MongoDB Configuration:" -ForegroundColor Cyan
Write-Host "  Data Directory: $mongoDataPath" -ForegroundColor Gray
Write-Host "  Log Directory: $mongoLogPath" -ForegroundColor Gray
Write-Host "  Connection String: mongodb://localhost:27017" -ForegroundColor Gray
Write-Host "  Database Name: signal_processing" -ForegroundColor Gray

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "MongoDB setup completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Note: The application will use the default MongoDB" -ForegroundColor Yellow
Write-Host "installation. Ensure MongoDB is installed as a" -ForegroundColor Yellow
Write-Host "Windows service or running manually." -ForegroundColor Yellow
Write-Host "=================================================" -ForegroundColor Cyan
