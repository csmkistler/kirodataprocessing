# Database Setup Guide for Signal Processing API

The Signal Processing API requires two databases to be running:
1. **MongoDB** - For metadata, events, and configuration
2. **InfluxDB** - For time-series signal data

## Quick Start Options

### Option 1: Docker (Recommended - Easiest)

If you have Docker Desktop installed, this is the fastest way:

```powershell
# Start MongoDB
docker run -d --name mongodb -p 27017:27017 mongo:latest

# Start InfluxDB
docker run -d --name influxdb -p 8086:8086 influxdata/influxdb:latest

# Verify they're running
docker ps
```

To stop them later:
```powershell
docker stop mongodb influxdb
```

To start them again:
```powershell
docker start mongodb influxdb
```

### Option 2: Install MongoDB and InfluxDB Locally

#### Install MongoDB Community Edition

1. **Download MongoDB:**
   - Visit: https://www.mongodb.com/try/download/community
   - Select: Windows, MSI package
   - Download and run the installer

2. **During Installation:**
   - Choose "Complete" installation
   - Install MongoDB as a Service (check the box)
   - Use default data directory: `C:\Program Files\MongoDB\Server\7.0\data`

3. **Start MongoDB Service:**
   ```powershell
   # Start the service
   net start MongoDB
   
   # Check if it's running
   Get-Service MongoDB
   ```

4. **Verify MongoDB is working:**
   ```powershell
   # This should connect without errors
   mongosh --eval "db.version()"
   ```

#### Install InfluxDB

1. **Download InfluxDB:**
   - Visit: https://portal.influxdata.com/downloads/
   - Select: Windows
   - Download the ZIP file

2. **Extract and Run:**
   ```powershell
   # Extract to a folder like C:\influxdb
   # Navigate to the folder
   cd C:\influxdb
   
   # Run InfluxDB
   .\influxd.exe
   ```

3. **Or run as a background process:**
   ```powershell
   Start-Process -FilePath "C:\influxdb\influxd.exe" -WindowStyle Hidden
   ```

### Option 3: Use Embedded/In-Memory (Development Only)

For quick testing without installing databases, you could modify the code to use:
- **MongoDB**: Use an embedded MongoDB library (not recommended for this project)
- **InfluxDB**: The current implementation expects a running instance

## Verify Databases Are Running

### Check MongoDB:
```powershell
# Test connection
Test-NetConnection -ComputerName localhost -Port 27017
```

### Check InfluxDB:
```powershell
# Test connection
Test-NetConnection -ComputerName localhost -Port 8086

# Or visit in browser
# http://localhost:8086
```

## Configuration

The API uses these default connection strings (configured in `appsettings.json`):

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017",
    "InfluxDB": "http://localhost:8086"
  },
  "MongoDB": {
    "DatabaseName": "signal_processing"
  },
  "InfluxDB": {
    "Organization": "signal-processing",
    "Bucket": "signals",
    "Token": ""
  }
}
```

## Troubleshooting

### MongoDB Connection Issues

**Error:** "No connection could be made because the target machine actively refused it"
- **Solution:** MongoDB is not running. Start the service or Docker container.

**Error:** "A timeout occurred after 30000ms"
- **Solution:** MongoDB is not accessible. Check firewall settings.

### InfluxDB Connection Issues

**Error:** "Connection refused"
- **Solution:** InfluxDB is not running. Start the process or Docker container.

## Testing the Setup

Once both databases are running, test the API:

1. **Start the API:**
   ```powershell
   dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj
   ```

2. **Test endpoints in Swagger:**
   - Visit: http://localhost:5000/swagger
   - Try GET /api/triggers/events (should return empty array, not an error)
   - Try POST /api/signals/generate to create a signal

3. **Check database contents:**
   ```powershell
   # MongoDB
   mongosh
   use signal_processing
   db.signal_metadata.find()
   
   # InfluxDB (visit web UI)
   # http://localhost:8086
   ```

## Recommended: Docker Compose (Future Enhancement)

For easier management, consider creating a `docker-compose.yml`:

```yaml
version: '3.8'
services:
  mongodb:
    image: mongo:latest
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
  
  influxdb:
    image: influxdata/influxdb:latest
    ports:
      - "8086:8086"
    volumes:
      - influxdb_data:/var/lib/influxdb2

volumes:
  mongodb_data:
  influxdb_data:
```

Then simply run:
```powershell
docker-compose up -d
```

## Next Steps

After setting up the databases:
1. Restart the API
2. Test all endpoints in Swagger
3. Verify data is being stored correctly
4. Continue with frontend development (Tasks 9-12)
