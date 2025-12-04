# Implementation Plan

- [x] 1. Set up project structure and core interfaces
  - Create .NET solution with Core, Application, Infrastructure, and Api projects
  - Set up React frontend project with Vite and TypeScript
  - Define core domain entities (Signal, ProcessedSignal, TriggerEvent, AppConfig)
  - Define core interfaces (ISignalGenerator, ISignalProcessor, ITriggerComponent, IDatabase)
  - Configure dependency injection in Program.cs
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [x] 2. Implement Signal Generator component
  - [x] 2.1 Create SignalGeneratorParams value object and validation logic
    - Implement SignalGeneratorParams record with all required fields
    - Add validation for frequency, amplitude, phase, duration, and sample rate
    - Implement ValidationResult class for error reporting
    - _Requirements: 1.1, 1.4, 1.5_

  - [x] 2.2 Write property test for parameter validation
    - **Property 2: Invalid parameter rejection**
    - **Validates: Requirements 1.4**

  - [x] 2.3 Implement signal generation algorithms for all signal types
    - Create SignalGenerator class implementing ISignalGenerator
    - Implement sine wave generation using Math.Sin
    - Implement square wave generation
    - Implement sawtooth wave generation
    - Implement white noise generation using random number generator
    - Generate samples and timestamps arrays based on duration and sample rate
    - _Requirements: 1.1, 1.2, 1.5_

  - [x] 2.4 Write property test for signal generation
    - **Property 1: Signal generation with parameters**
    - **Validates: Requirements 1.1, 1.3, 1.5**

- [x] 3. Implement database layer
  - [x] 3.1 Set up MongoDB connection and context
    - Create MongoDbContext class with connection management
    - Configure MongoDB connection string in appsettings.json
    - Implement IMetadataDatabase interface with MongoDB driver
    - Create collections for signal_metadata, processed_signal_metadata, trigger_events, app_config
    - _Requirements: 6.1, 6.2, 6.5_

  - [x] 3.2 Set up InfluxDB connection and context
    - Create InfluxDbContext class with InfluxDB client
    - Configure InfluxDB connection settings
    - Implement ITimeSeriesDatabase interface
    - Define measurements for signal_samples and processed_signal_samples
    - _Requirements: 6.1, 6.2_

  - [x] 3.3 Implement unified database interface
    - Create SignalDatabase class implementing ISignalDatabase
    - Implement SaveCompleteSignal method coordinating both databases
    - Implement GetCompleteSignal method retrieving from both databases
    - Add batch write operations for efficient sample storage
    - _Requirements: 6.1, 6.2, 6.3_

  - [x] 3.4 Write property test for signal persistence
    - **Property 13: Signal persistence with metadata**
    - **Validates: Requirements 6.1, 6.2**

  - [x] 3.5 Write property test for referential integrity
    - **Property 15: Referential integrity**
    - **Validates: Requirements 6.4**

- [x] 4. Implement Signal Processor component
  - [x] 4.1 Create ProcessingParams value object and validation
    - Implement ProcessingParams record with operation type and parameters
    - Add validation for cutoff frequencies, gain, and filter order
    - Validate against Nyquist frequency constraints
    - _Requirements: 2.1, 2.5_

  - [x] 4.2 Write property test for processing parameter validation
    - **Property 4: Invalid processing parameter rejection**
    - **Validates: Requirements 2.5**

  - [x] 4.3 Implement signal processing algorithms
    - Create SignalProcessor class implementing ISignalProcessor
    - Implement low-pass Butterworth filter
    - Implement high-pass Butterworth filter
    - Implement band-pass filter
    - Implement gain adjustment operation
    - Retrieve original signal from database before processing
    - _Requirements: 2.1, 2.2_

  - [x] 4.4 Write property test for signal processing
    - **Property 3: Signal processing preserves original**
    - **Validates: Requirements 2.1, 2.3, 2.4**

- [ ] 5. Implement Trigger Component
  - [ ] 5.1 Create TriggerComponent class with threshold checking
    - Implement ITriggerComponent interface
    - Add Configure method for setting threshold
    - Implement CheckValue method for threshold comparison
    - Create TriggerEvent when threshold is exceeded
    - Store events in MongoDB via IMetadataDatabase
    - _Requirements: 5.1, 5.2, 5.4_

  - [ ] 5.2 Write property test for threshold comparison
    - **Property 9: Threshold comparison**
    - **Validates: Requirements 5.1**

  - [ ] 5.3 Write property test for event emission
    - **Property 10: Event emission on threshold exceeded**
    - **Validates: Requirements 5.2**

- [ ] 6. Implement Application Services layer
  - [ ] 6.1 Create SignalGenerationService
    - Implement service orchestrating signal generation and storage
    - Add GenerateSignalAsync method calling ISignalGenerator and ISignalDatabase
    - Add GetSignalAsync and GetRecentSignalsAsync methods
    - Map domain entities to DTOs
    - _Requirements: 1.1, 1.3, 6.1_

  - [ ] 6.2 Create SignalProcessingService
    - Implement service orchestrating signal processing and storage
    - Add ProcessSignalAsync method calling ISignalProcessor and ISignalDatabase
    - Ensure original signal is preserved during processing
    - Map processed signals to DTOs
    - _Requirements: 2.1, 2.3, 2.4_

  - [ ] 6.3 Create TriggerService
    - Implement service wrapping ITriggerComponent
    - Add methods for configuring threshold and checking values
    - Add GetEventsAsync method retrieving events from database
    - Map trigger events to DTOs
    - _Requirements: 5.1, 5.2, 5.3, 5.5_

- [ ] 7. Implement API Controllers
  - [ ] 7.1 Create SignalsController
    - Add POST /api/signals/generate endpoint accepting SignalGenerationRequest
    - Add GET /api/signals/{id} endpoint returning SignalDto
    - Add GET /api/signals endpoint with pagination for recent signals
    - Implement proper error handling and status codes
    - Add XML documentation for Swagger
    - _Requirements: 1.1, 1.4, 6.3_

  - [ ] 7.2 Create ProcessingController
    - Add POST /api/processing/process endpoint accepting ProcessingRequest
    - Add GET /api/processing/{id} endpoint returning ProcessedSignalDto
    - Validate processing parameters and return 400 for invalid input
    - _Requirements: 2.1, 2.5_

  - [ ] 7.3 Create TriggersController
    - Add POST /api/triggers/configure endpoint for setting threshold
    - Add POST /api/triggers/check endpoint accepting input value
    - Add GET /api/triggers/events endpoint returning event list
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [ ] 7.4 Configure global exception handling and CORS
    - Implement GlobalExceptionHandler for consistent error responses
    - Configure CORS for local development
    - Add Swagger/OpenAPI documentation
    - _Requirements: 1.4, 2.5_

- [ ] 8. Checkpoint - Ensure backend tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 9. Implement React frontend structure
  - [ ] 9.1 Set up React project with TypeScript and Vite
    - Initialize Vite project with React and TypeScript template
    - Install dependencies (recharts, axios, react-hook-form)
    - Configure Vite proxy for API calls to backend
    - Set up basic App component structure
    - _Requirements: 3.1, 7.1, 7.4_

  - [ ] 9.2 Create API service layer
    - Implement api.ts with axios client
    - Add methods for signal generation, processing, and trigger operations
    - Add TypeScript interfaces for request/response types
    - Implement error handling and response parsing
    - _Requirements: 1.1, 2.1, 5.1_

- [ ] 10. Implement Visualization Component
  - [ ] 10.1 Create SignalChart component
    - Implement chart using Recharts LineChart
    - Display time on x-axis and amplitude on y-axis
    - Support rendering both original and processed signals
    - Add zoom and pan capabilities
    - Implement downsampling for large datasets (>10,000 points)
    - _Requirements: 3.1, 3.2, 3.3, 3.5_

  - [ ] 10.2 Write property test for chart rendering
    - **Property 5: Chart renders signal data**
    - **Validates: Requirements 3.1**

  - [ ] 10.3 Create ParameterPanel component
    - Add form inputs for signal type, frequency, amplitude, phase, duration, sample rate
    - Implement real-time validation with error messages
    - Add "Generate Signal" button triggering API call
    - Display loading state during generation
    - _Requirements: 1.1, 1.4, 1.5, 4.1, 4.2_

  - [ ] 10.4 Write property test for parameter validation feedback
    - **Property 6: Parameter validation feedback**
    - **Validates: Requirements 4.2**

  - [ ] 10.5 Write property test for invalid parameter prevention
    - **Property 8: Invalid parameter prevention**
    - **Validates: Requirements 4.5**

  - [ ] 10.6 Create ProcessingPanel component
    - Add dropdown for selecting processing operation type
    - Add conditional inputs based on operation (cutoff frequency, gain, etc.)
    - Implement validation for processing parameters
    - Add "Apply Processing" button triggering API call
    - _Requirements: 2.1, 2.5, 4.1, 4.3, 4.4_

  - [ ] 10.7 Write property test for parameter application
    - **Property 7: Parameter application triggers processing**
    - **Validates: Requirements 4.3**

  - [ ] 10.8 Create SignalSelector component
    - Add dropdown listing recent signals from database
    - Implement signal selection triggering chart update
    - Display signal metadata (type, frequency, creation time)
    - _Requirements: 6.3_

- [ ] 11. Implement Trigger UI components
  - [ ] 11.1 Create TriggerInput component
    - Add numeric input field for entering values
    - Add input field for configuring threshold
    - Add "Check Value" button calling trigger API
    - Display feedback when threshold is exceeded
    - _Requirements: 5.1, 5.2, 5.4_

  - [ ] 11.2 Create EventList component
    - Fetch and display trigger events from API
    - Display events in reverse chronological order
    - Show event value, threshold, and timestamp for each event
    - Implement auto-refresh when new events are triggered
    - _Requirements: 5.3, 5.5_

  - [ ] 11.3 Write property test for event display
    - **Property 11: Event display**
    - **Validates: Requirements 5.3**

  - [ ] 11.4 Write property test for event ordering
    - **Property 12: Event ordering**
    - **Validates: Requirements 5.5**

- [ ] 12. Implement main App component integration
  - [ ] 12.1 Wire up all components in App.tsx
    - Create state management for signals and processing
    - Connect ParameterPanel to signal generation
    - Connect ProcessingPanel to signal processing
    - Connect SignalChart to display both signals
    - Connect TriggerInput and EventList
    - Implement chart update within 100ms of data availability
    - _Requirements: 3.1, 3.2, 3.4_

  - [ ] 12.2 Write property test for query retrieval
    - **Property 14: Query retrieval**
    - **Validates: Requirements 6.3**

- [ ] 13. Implement Windows deployment scripts
  - [ ] 13.1 Create PowerShell startup script
    - Check for .NET 10 SDK installation
    - Check for MongoDB service and start if needed
    - Start backend server with dotnet run
    - Wait for server to be ready
    - Open browser to http://localhost:5000
    - _Requirements: 7.1, 7.2, 7.3, 7.4_

  - [ ] 13.2 Create batch file startup script
    - Implement same functionality as PowerShell script for cmd.exe
    - Add error handling for missing dependencies
    - _Requirements: 7.1, 7.2, 7.3_

  - [ ] 13.3 Configure production build
    - Set up frontend build process outputting to wwwroot
    - Configure backend to serve static files
    - Set up MongoDB data directory in user AppData
    - Create README with installation and usage instructions
    - _Requirements: 7.1, 7.5_

- [ ] 14. Final Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
