# Signal Processing Visualization Application

A signal processing visualization application built with .NET 10 and React that enables users to generate, process, and visualize signal waveforms in real-time.

## Project Structure

The application follows **Onion Architecture** with clear separation of concerns:

```
signal-processing-viz/
├── src/
│   ├── SignalProcessing.Core/              # Core/Domain Layer
│   │   ├── Entities/                       # Domain entities
│   │   │   ├── Signal.cs
│   │   │   ├── ProcessedSignal.cs
│   │   │   ├── TriggerEvent.cs
│   │   │   └── AppConfig.cs
│   │   ├── Interfaces/                     # Core interfaces
│   │   │   ├── ISignalGenerator.cs
│   │   │   ├── ISignalProcessor.cs
│   │   │   ├── ITriggerComponent.cs
│   │   │   ├── ITimeSeriesDatabase.cs
│   │   │   ├── IMetadataDatabase.cs
│   │   │   └── ISignalDatabase.cs
│   │   └── ValueObjects/                   # Value objects and enums
│   │       ├── SignalType.cs
│   │       ├── OperationType.cs
│   │       ├── SignalGeneratorParams.cs
│   │       ├── ProcessingParams.cs
│   │       ├── ValidationResult.cs
│   │       ├── TriggerConfig.cs
│   │       └── UiPreferences.cs
│   ├── SignalProcessing.Application/       # Application Layer
│   │   ├── Services/                       # Application services (to be implemented)
│   │   └── DTOs/                           # Data transfer objects (to be implemented)
│   ├── SignalProcessing.Infrastructure/    # Infrastructure Layer
│   │   ├── Data/                           # Database implementations (to be implemented)
│   │   └── Implementations/                # Core interface implementations (to be implemented)
│   └── SignalProcessing.Api/               # Presentation Layer
│       ├── Controllers/                    # API controllers
│       │   └── HealthController.cs
│       └── Program.cs                      # Application entry point with DI configuration
├── client/                                  # React Frontend
│   ├── src/
│   │   ├── components/                     # React components (to be implemented)
│   │   ├── services/                       # API service layer (to be implemented)
│   │   ├── App.tsx                         # Main application component
│   │   └── main.tsx                        # Application entry point
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
└── SignalProcessing.sln                    # Solution file

```

## Technology Stack

### Backend
- **.NET 10** - Web API framework
- **ASP.NET Core** - REST API
- **Swashbuckle** - API documentation (Swagger/OpenAPI)
- **MongoDB** - Metadata storage (to be configured)
- **InfluxDB** - Time-series data storage (to be configured)

### Frontend
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **Recharts** - Signal visualization (to be installed)
- **Axios** - HTTP client (to be installed)

## Architecture Principles

### Onion Architecture Layers

1. **Core Layer** (Domain)
   - Contains domain entities, value objects, and interfaces
   - No dependencies on other layers
   - Pure business logic

2. **Application Layer**
   - Contains application services and use cases
   - Depends only on Core layer
   - Orchestrates domain objects

3. **Infrastructure Layer**
   - Contains database implementations and external services
   - Implements interfaces defined in Core layer
   - Can depend on Core layer

4. **Presentation Layer** (API + UI)
   - ASP.NET Core Web API controllers
   - React UI components
   - Depends on Application layer through dependency injection

### Dependency Flow
```
Presentation → Application → Core ← Infrastructure
```

## Features

- **Signal Generation**: Create sine, square, sawtooth, and white noise signals with configurable parameters
- **Signal Processing**: Apply low-pass, high-pass, band-pass filters and gain adjustments
- **Real-time Visualization**: Interactive charts with zoom and pan capabilities
- **Trigger System**: Monitor values and emit events when thresholds are exceeded
- **Persistent Storage**: Save signals and configurations using MongoDB and InfluxDB
- **Property-Based Testing**: Comprehensive test coverage using FsCheck

## Prerequisites

### Required
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **MongoDB Community Edition** - [Download](https://www.mongodb.com/try/download/community)

### Optional (for development)
- **Node.js 18+** - [Download](https://nodejs.org/) (for frontend development)
- **Visual Studio 2022** or **VS Code** - For development

## Quick Start (Windows)

### Option 1: Using PowerShell (Recommended)

1. **Clone the repository**
   ```powershell
   git clone <repository-url>
   cd signal-processing-viz
   ```

2. **Install MongoDB** (if not already installed)
   - Download MongoDB Community Edition from the link above
   - Install as a Windows Service
   - The startup script will check and start MongoDB automatically

3. **Run the application**
   ```powershell
   .\start.ps1
   ```

   The script will:
   - Check for .NET 10 SDK
   - Check and start MongoDB service
   - Start the backend server
   - Wait for the server to be ready
   - Open your browser to http://localhost:5000

### Option 2: Using Command Prompt

```cmd
start.bat
```

### Option 3: Manual Start

1. **Start MongoDB** (if not running as a service)
   ```powershell
   net start MongoDB
   ```

2. **Run the backend**
   ```powershell
   dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj
   ```

3. **Open browser**
   Navigate to http://localhost:5000

## Production Build

### Build for Production

**PowerShell:**
```powershell
.\build-production.ps1
```

**Command Prompt:**
```cmd
build-production.bat
```

This will:
1. Build the React frontend for production
2. Output frontend files to `src/SignalProcessing.Api/wwwroot`
3. Build the .NET backend in Release mode
4. Create MongoDB data directory in `%LOCALAPPDATA%\SignalProcessingViz\mongodb`

### Deploy Production Build

After building, you can run the production version:

```powershell
dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj --configuration Release
```

Or use the startup scripts which work with both Debug and Release builds.

## Development

### Backend Development

1. **Build the solution**
   ```bash
   dotnet build SignalProcessing.sln
   ```

2. **Run tests**
   ```bash
   dotnet test src/SignalProcessing.Tests/SignalProcessing.Tests.csproj
   ```

3. **Run the API**
   ```bash
   dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj
   ```

### Frontend Development

1. **Install dependencies**
   ```bash
   cd client
   npm install
   ```

2. **Run development server**
   ```bash
   npm run dev
   ```
   Frontend will be available at http://localhost:3000

3. **Run tests**
   ```bash
   npm test
   ```

4. **Build for production**
   ```bash
   npm run build:prod
   ```

### API Documentation

Once the API is running, access Swagger UI at:
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/api/health

### Available Endpoints

- `GET /api/health` - Health check
- `POST /api/signals/generate` - Generate a new signal
- `GET /api/signals/{id}` - Get signal by ID
- `GET /api/signals` - Get recent signals (paginated)
- `POST /api/processing/process` - Process a signal
- `GET /api/processing/{id}` - Get processed signal by ID
- `POST /api/triggers/configure` - Configure trigger threshold
- `POST /api/triggers/check` - Check value against threshold
- `GET /api/triggers/events` - Get trigger events

## Database Configuration

### MongoDB

The application uses MongoDB for storing:
- Signal metadata (type, frequency, amplitude, etc.)
- Processed signal metadata
- Trigger events
- Application configuration

**Default Connection**: `mongodb://localhost:27017`

**Database Name**: `signal_processing`

**Data Directory**: `%LOCALAPPDATA%\SignalProcessingViz\mongodb`

### InfluxDB

InfluxDB runs in embedded mode within the .NET application for storing:
- Signal sample data (time-series)
- Processed signal samples

No separate installation required.

## Troubleshooting

### MongoDB Service Not Starting

If MongoDB fails to start:

1. **Check if MongoDB is installed**
   ```powershell
   sc query MongoDB
   ```

2. **Start manually**
   ```powershell
   net start MongoDB
   ```

3. **Check MongoDB logs**
   - Default location: `C:\Program Files\MongoDB\Server\<version>\log\mongod.log`

### Port Already in Use

If port 5000 is already in use:

1. **Find the process using port 5000**
   ```powershell
   netstat -ano | findstr :5000
   ```

2. **Kill the process** (replace PID with actual process ID)
   ```powershell
   taskkill /PID <PID> /F
   ```

### .NET SDK Not Found

Ensure .NET 10 SDK is installed:
```powershell
dotnet --version
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/10.0

## Testing

### Backend Tests

The project includes comprehensive property-based tests using FsCheck:

```bash
dotnet test src/SignalProcessing.Tests/SignalProcessing.Tests.csproj
```

**Test Coverage:**
- Signal generation with various parameters
- Signal processing operations
- Database persistence and retrieval
- Trigger threshold detection
- Referential integrity

### Frontend Tests

```bash
cd client
npm test
```

**Test Coverage:**
- Component rendering
- Parameter validation
- Event handling
- Chart visualization

## Project Status

✅ **Completed:**
- Complete backend implementation with signal generation and processing
- MongoDB and InfluxDB integration
- REST API with Swagger documentation
- React frontend with interactive charts
- Trigger system with event tracking
- Property-based testing suite
- Windows deployment scripts

## Architecture

The application follows **Onion Architecture** principles:

- **Core Layer**: Domain entities, interfaces, value objects (no dependencies)
- **Application Layer**: Services and DTOs (depends on Core only)
- **Infrastructure Layer**: Database implementations (implements Core interfaces)
- **Presentation Layer**: API controllers and React UI (depends on Application)

**Dependency Flow**: Presentation → Application → Core ← Infrastructure

## Contributing

1. Follow the coding conventions in `.kiro/steering/dotnet-conventions.md`
2. Write tests for new features
3. Update documentation as needed
4. Ensure all tests pass before committing

## License

[Your License Here]

## Support

For issues and questions, please refer to:
- **Specification**: `.kiro/specs/signal-processing-viz/`
- **Design Document**: `.kiro/specs/signal-processing-viz/design.md`
- **Implementation Tasks**: `.kiro/specs/signal-processing-viz/tasks.md`
