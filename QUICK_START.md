# Quick Start Guide

## Prerequisites
- Docker Desktop installed and running
- .NET 10 SDK installed

## Step 1: Start Databases

Make sure Docker Desktop is running, then:

```powershell
# Start MongoDB and InfluxDB
.\start-databases.ps1
```

Or manually:
```powershell
docker-compose up -d
```

## Step 2: Start the API

```powershell
dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj
```

## Step 3: Test the API

Open your browser to:
- **Swagger UI:** http://localhost:5000/swagger
- **Health Check:** http://localhost:5000/api/health

## Database Access

### MongoDB
- **Connection:** mongodb://localhost:27017
- **Database:** signal_processing
- **Tool:** MongoDB Compass or mongosh

### InfluxDB
- **Web UI:** http://localhost:8086
- **Username:** admin
- **Password:** adminpassword
- **Token:** my-super-secret-auth-token
- **Organization:** signal-processing
- **Bucket:** signals

## Stopping Everything

```powershell
# Stop databases
docker-compose down

# Stop API (Ctrl+C in the terminal where it's running)
```

## Troubleshooting

### Docker not running
```
Error: Cannot connect to the Docker daemon
```
**Solution:** Start Docker Desktop and wait for it to be ready (whale icon in system tray)

### Port already in use
```
Error: port is already allocated
```
**Solution:** Stop any existing containers using those ports:
```powershell
docker-compose down
docker ps -a
docker rm -f <container-id>
```

### Database connection errors in API
```
Error: Database connection error
```
**Solution:** 
1. Check databases are running: `docker-compose ps`
2. Check logs: `docker-compose logs`
3. Restart databases: `docker-compose restart`

## Testing the API

### Generate a Signal
```bash
curl -X POST http://localhost:5000/api/signals/generate \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Sine",
    "frequency": 1000,
    "amplitude": 1.0,
    "phase": 0,
    "duration": 1.0,
    "sampleRate": 44100
  }'
```

### Get Recent Signals
```bash
curl http://localhost:5000/api/signals?limit=10
```

### Configure Trigger
```bash
curl -X POST http://localhost:5000/api/triggers/configure \
  -H "Content-Type: application/json" \
  -d '{
    "threshold": 5.0,
    "enabled": true
  }'
```

### Check Trigger Value
```bash
curl -X POST http://localhost:5000/api/triggers/check \
  -H "Content-Type: application/json" \
  -d '{
    "value": 6.5
  }'
```

### Get Trigger Events
```bash
curl http://localhost:5000/api/triggers/events?limit=100
```

## Next Steps

1. ✅ Databases running
2. ✅ API running
3. ✅ Test endpoints in Swagger
4. 🔜 Build the React frontend (Tasks 9-12)
5. 🔜 Create deployment scripts (Task 13)
