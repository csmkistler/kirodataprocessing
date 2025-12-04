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

## Prerequisites

- **.NET 10 SDK** (RC or later)
- **Node.js** (for frontend development - to be installed)
- **MongoDB** (local instance - to be configured)
- **InfluxDB** (embedded mode - to be configured)

## Getting Started

### Build the Solution

```bash
dotnet build SignalProcessing.sln
```

### Run the API

```bash
dotnet run --project src/SignalProcessing.Api/SignalProcessing.Api.csproj
```

The API will be available at `http://localhost:5000`

### API Documentation

Once the API is running, access Swagger UI at:
- `http://localhost:5000/swagger`

### Health Check

Test the API is running:
```bash
curl http://localhost:5000/api/health
```

## Development Status

✅ **Completed:**
- Project structure and solution setup
- Core domain entities and interfaces
- Onion architecture implementation
- Basic API configuration with Swagger
- React frontend project structure

🚧 **To Be Implemented:**
- Signal generation algorithms
- Signal processing algorithms
- Database implementations (MongoDB, InfluxDB)
- Application services
- API controllers
- React UI components
- Frontend-backend integration

## Next Steps

Refer to `.kiro/specs/signal-processing-viz/tasks.md` for the detailed implementation plan.

Data processing project
