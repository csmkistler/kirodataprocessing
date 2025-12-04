# Start Databases for Signal Processing API
Write-Host "🚀 Starting Signal Processing Databases..." -ForegroundColor Cyan
Write-Host ""

# Start the databases
Write-Host "📦 Starting MongoDB and InfluxDB containers..." -ForegroundColor Green
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Databases started successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Database Information:" -ForegroundColor Cyan
    Write-Host "  MongoDB:  mongodb://localhost:27017" -ForegroundColor White
    Write-Host "  InfluxDB: http://localhost:8086" -ForegroundColor White
    Write-Host ""
    Write-Host "🔐 InfluxDB Credentials:" -ForegroundColor Cyan
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: adminpassword" -ForegroundColor White
    Write-Host "  Token:    my-super-secret-auth-token" -ForegroundColor White
    Write-Host ""
    Write-Host "⏳ Waiting for databases to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    Write-Host ""
    Write-Host "🏥 Health Check:" -ForegroundColor Cyan
    docker-compose ps
    
    Write-Host ""
    Write-Host "✨ Ready! You can now start the API:" -ForegroundColor Green
    Write-Host "   dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj" -ForegroundColor White
    Write-Host ""
    Write-Host "📚 Swagger UI: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🛑 To stop: docker-compose down" -ForegroundColor Yellow
}
