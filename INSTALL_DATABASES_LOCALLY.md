# Install Databases Locally (Without Docker)

Since Docker Desktop is having issues, here's how to install MongoDB and InfluxDB directly on Windows.

## Option 1: MongoDB Only (Simplest - Start Here)

The API will work with just MongoDB for now. InfluxDB can be added later.

### Install MongoDB Community Edition

1. **Download MongoDB:**
   - Go to: https://www.mongodb.com/try/download/community
   - Select: Windows, MSI package, Latest version
   - Click "Download"

2. **Run the Installer:**
   - Double-click the downloaded `.msi` file
   - Choose "Complete" installation
   - **Important:** Check "Install MongoDB as a Service"
   - Use default settings for everything else
   - Click "Install"

3. **Verify MongoDB is Running:**
   ```powershell
   # Check if service is running
   Get-Service MongoDB
   
   # Should show: Status = Running
   ```

4. **Test Connection:**
   ```powershell
   # If mongosh is installed
   mongosh --eval "db.version()"
   
   # Should show MongoDB version
   ```

### Update API Configuration

The API is already configured for `mongodb://localhost:27017` - no changes needed!

### Start the API

```powershell
dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj
```

Now test at: **http://localhost:5000/swagger**

The trigger events endpoint should now return an empty array `[]` instead of an error!

## Option 2: Add InfluxDB (For Full Functionality)

### Install InfluxDB

1. **Download InfluxDB:**
   - Go to: https://portal.influxdata.com/downloads/
   - Select: Windows
   - Download the ZIP file

2. **Extract and Setup:**
   ```powershell
   # Extract to C:\influxdb
   # Open PowerShell as Administrator
   cd C:\influxdb
   
   # Run InfluxDB
   .\influxd.exe
   ```

3. **Setup InfluxDB (First Time):**
   - Open browser to: http://localhost:8086
   - Click "Get Started"
   - Create account:
     - Username: admin
     - Password: adminpassword
     - Organization: signal-processing
     - Bucket: signals
   - Copy the generated token
   - Update `src/SignalProcessing.Api/appsettings.json`:
     ```json
     "InfluxDB": {
       "Token": "your-token-here",
       "Organization": "signal-processing",
       "Bucket": "signals"
     }
     ```

4. **Run InfluxDB as Background Service:**
   ```powershell
   # Create a scheduled task or use NSSM (Non-Sucking Service Manager)
   # Or just run in a separate PowerShell window:
   cd C:\influxdb
   .\influxd.exe
   ```

## Quick Test

### Test MongoDB Only:

```powershell
# Start API
dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj

# In another terminal, test trigger endpoint:
curl http://localhost:5000/api/triggers/events

# Should return: []
```

### Test Full System (MongoDB + InfluxDB):

```powershell
# Generate a signal
curl -X POST http://localhost:5000/api/signals/generate `
  -H "Content-Type: application/json" `
  -d '{
    "type": "Sine",
    "frequency": 1000,
    "amplitude": 1.0,
    "phase": 0,
    "duration": 1.0,
    "sampleRate": 44100
  }'

# Should return signal data with ID
```

## Troubleshooting

### MongoDB won't start

```powershell
# Check if port 27017 is in use
netstat -an | findstr "27017"

# Start service manually
net start MongoDB

# Check logs
# C:\Program Files\MongoDB\Server\7.0\log\mongod.log
```

### InfluxDB connection errors

- Make sure InfluxDB is running (`.\influxd.exe`)
- Check http://localhost:8086 is accessible
- Verify token in appsettings.json matches InfluxDB

## What Works Without InfluxDB?

With just MongoDB running:
- ✅ Trigger configuration and events
- ✅ Signal metadata storage
- ❌ Signal sample data (needs InfluxDB)
- ❌ Full signal generation (needs InfluxDB)

## Recommended: Start with MongoDB

1. Install MongoDB (5 minutes)
2. Test trigger endpoints
3. Install InfluxDB later when needed for signal generation

## Alternative: Fix Docker Desktop

If you want to use Docker instead:

1. **Completely restart Docker Desktop:**
   - Right-click Docker icon in system tray
   - Select "Quit Docker Desktop"
   - Wait 30 seconds
   - Start Docker Desktop again
   - Wait for it to fully start (whale icon stops animating)

2. **Then try:**
   ```powershell
   docker-compose up -d
   ```

3. **If still failing:**
   - Open Docker Desktop settings
   - Go to "Resources" → "Advanced"
   - Increase memory to 4GB
   - Click "Apply & Restart"
