# Installation Guide

This guide provides step-by-step instructions for installing and running the Signal Processing Visualization application on Windows.

## System Requirements

- **Operating System**: Windows 10 or later
- **RAM**: 4 GB minimum, 8 GB recommended
- **Disk Space**: 2 GB for application and dependencies
- **Internet Connection**: Required for initial setup

## Step 1: Install .NET 10 SDK

1. Visit the .NET download page: https://dotnet.microsoft.com/download/dotnet/10.0
2. Download the .NET 10 SDK installer for Windows
3. Run the installer and follow the prompts
4. Verify installation by opening PowerShell or Command Prompt and running:
   ```powershell
   dotnet --version
   ```
   You should see version 10.x.x or higher

## Step 2: Install MongoDB Community Edition

1. Visit the MongoDB download page: https://www.mongodb.com/try/download/community
2. Select:
   - **Version**: Latest (7.0 or higher)
   - **Platform**: Windows
   - **Package**: MSI
3. Download and run the installer
4. During installation:
   - Choose "Complete" installation
   - Select "Install MongoDB as a Service"
   - Use default service name: "MongoDB"
   - Use default data directory or choose custom location
5. Verify installation by opening PowerShell and running:
   ```powershell
   sc query MongoDB
   ```
   The service should show as "RUNNING"

### Alternative: MongoDB Manual Installation

If you prefer not to install MongoDB as a service:

1. Download MongoDB ZIP archive
2. Extract to a location (e.g., `C:\mongodb`)
3. Create data directory: `C:\mongodb\data`
4. Start MongoDB manually:
   ```powershell
   C:\mongodb\bin\mongod.exe --dbpath C:\mongodb\data
   ```

## Step 3: Download the Application

### Option A: Clone from Git (if available)
```powershell
git clone <repository-url>
cd signal-processing-viz
```

### Option B: Download ZIP
1. Download the application ZIP file
2. Extract to a location (e.g., `C:\signal-processing-viz`)
3. Open PowerShell and navigate to the directory:
   ```powershell
   cd C:\signal-processing-viz
   ```

## Step 4: Build the Application (Optional)

If you want to build from source:

### PowerShell:
```powershell
.\build-production.ps1
```

### Command Prompt:
```cmd
build-production.bat
```

This step is optional if you received pre-built binaries.

## Step 5: Run the Application

### PowerShell (Recommended):
```powershell
.\start.ps1
```

### Command Prompt:
```cmd
start.bat
```

The startup script will:
1. ✓ Check for .NET 10 SDK
2. ✓ Check for MongoDB service
3. ✓ Start MongoDB if not running
4. ✓ Start the backend server
5. ✓ Wait for server to be ready
6. ✓ Open your browser to http://localhost:5000

## Step 6: Verify Installation

Once the application starts, you should see:

1. **Console Output**:
   ```
   🚀 Signal Processing API is running!
   📍 API: http://localhost:5000
   📚 Swagger: http://localhost:5000/swagger
   💚 Health: http://localhost:5000/api/health
   🌐 Frontend: http://localhost:5000
   ```

2. **Browser**: Automatically opens to http://localhost:5000 showing the application UI

3. **Test the API**: Visit http://localhost:5000/api/health
   - Should return: `{"status":"healthy"}`

## Troubleshooting

### Issue: ".NET 10 SDK is not installed"

**Solution**: Install .NET 10 SDK from Step 1 above

### Issue: "MongoDB service is not installed"

**Solution**: 
1. Install MongoDB from Step 2 above
2. Or choose to continue without MongoDB (limited functionality)

### Issue: "Failed to start MongoDB service"

**Solution**:
1. Check if MongoDB is already running:
   ```powershell
   sc query MongoDB
   ```
2. Try starting manually:
   ```powershell
   net start MongoDB
   ```
3. Check MongoDB logs:
   - Default location: `C:\Program Files\MongoDB\Server\<version>\log\mongod.log`

### Issue: "Port 5000 is already in use"

**Solution**:
1. Find what's using port 5000:
   ```powershell
   netstat -ano | findstr :5000
   ```
2. Kill the process (replace PID with actual process ID):
   ```powershell
   taskkill /PID <PID> /F
   ```
3. Or configure the application to use a different port in `src/SignalProcessing.Api/Program.cs`

### Issue: "Server failed to start within 30 seconds"

**Solution**:
1. Check server logs:
   - `server-output.log`
   - `server-error.log`
2. Ensure MongoDB is running
3. Check for port conflicts
4. Try running manually:
   ```powershell
   dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj
   ```

### Issue: Browser doesn't open automatically

**Solution**:
Manually open your browser and navigate to: http://localhost:5000

## Uninstallation

To remove the application:

1. **Stop the application**: Press Ctrl+C in the console window

2. **Delete application files**: Remove the application directory

3. **Remove MongoDB data** (optional):
   ```powershell
   Remove-Item -Recurse -Force "$env:LOCALAPPDATA\SignalProcessingViz"
   ```

4. **Uninstall MongoDB** (optional):
   - Use Windows "Add or Remove Programs"
   - Search for "MongoDB"
   - Click "Uninstall"

5. **Uninstall .NET SDK** (optional):
   - Use Windows "Add or Remove Programs"
   - Search for ".NET SDK"
   - Click "Uninstall"

## Data Locations

- **Application Data**: `%LOCALAPPDATA%\SignalProcessingViz`
- **MongoDB Data**: `%LOCALAPPDATA%\SignalProcessingViz\mongodb`
- **Application Logs**: `%LOCALAPPDATA%\SignalProcessingViz\logs`
- **Server Logs**: `server-output.log` and `server-error.log` in application directory

## Next Steps

After successful installation:

1. **Explore the UI**: Generate signals, apply processing, and visualize results
2. **Read the User Guide**: See `USER_GUIDE.md` for feature documentation
3. **API Documentation**: Visit http://localhost:5000/swagger for API reference
4. **Development**: See `README.md` for development setup

## Getting Help

If you encounter issues not covered in this guide:

1. Check the main `README.md` for additional troubleshooting
2. Review the logs in `server-output.log` and `server-error.log`
3. Ensure all prerequisites are properly installed
4. Check that no firewall or antivirus is blocking the application

## System Information

The application uses:
- **Backend**: .NET 10 ASP.NET Core Web API
- **Frontend**: React 18 with TypeScript
- **Databases**: MongoDB (metadata) and InfluxDB (embedded, time-series)
- **Port**: 5000 (HTTP)
- **Binding**: localhost only (not accessible from network)
