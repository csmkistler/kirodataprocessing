# Signal Processing Visualization - Frontend

React-based frontend for the Signal Processing Visualization application.

## Technology Stack

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **Recharts** - Chart visualization library
- **Axios** - HTTP client for API calls
- **React Hook Form** - Form management

## Project Structure

```
client/
├── src/
│   ├── components/       # React components (to be implemented)
│   ├── services/         # API service layer
│   │   └── api.ts       # Axios client and API methods
│   ├── App.tsx          # Main application component
│   ├── main.tsx         # Application entry point
│   └── index.css        # Global styles
├── public/              # Static assets
├── package.json         # Dependencies and scripts
├── vite.config.ts       # Vite configuration
└── tsconfig.json        # TypeScript configuration

```

## API Service Layer

The `src/services/api.ts` file provides a complete API client with:

### Signal Operations
- `signalApi.generateSignal(request)` - Generate new signals
- `signalApi.getSignal(id)` - Retrieve signal by ID
- `signalApi.getRecentSignals(params)` - Get recent signals with pagination

### Processing Operations
- `processingApi.processSignal(request)` - Process signals with filters/gain
- `processingApi.getProcessedSignal(id)` - Retrieve processed signal by ID

### Trigger Operations
- `triggerApi.configure(request)` - Configure trigger threshold
- `triggerApi.checkValue(request)` - Check value against threshold
- `triggerApi.getEvents(params)` - Get trigger events with pagination

### Error Handling

All API methods include comprehensive error handling:
- Network errors
- Server errors (4xx, 5xx)
- Validation errors
- Timeout handling

Errors are returned as `ApiError` objects with:
```typescript
{
  message: string;
  statusCode?: number;
  validationErrors?: string[];
}
```

## Development

### Prerequisites
- Node.js 18+ and npm
- Backend API running on http://localhost:5000

### Install Dependencies
```bash
npm install
```

### Run Development Server
```bash
npm run dev
```

The application will be available at http://localhost:3000 with hot module replacement.

### Build for Production
```bash
npm run build
```

### Preview Production Build
```bash
npm run preview
```

## Configuration

### Vite Proxy
The Vite dev server is configured to proxy API requests to the backend:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- API calls to `/api/*` are automatically proxied

### TypeScript
Strict mode is enabled with:
- Type checking
- Unused variable detection
- No implicit any
- Strict null checks

## Next Steps

The following components will be implemented in subsequent tasks:
- SignalChart - Interactive signal visualization
- ParameterPanel - Signal generation controls
- ProcessingPanel - Signal processing controls
- TriggerInput - Threshold monitoring
- EventList - Trigger event display
- SignalSelector - Signal selection dropdown

## Requirements Validation

This implementation satisfies:
- ✅ Requirement 3.1: Visualization Component structure
- ✅ Requirement 7.1: Windows deployment compatibility
- ✅ Requirement 7.4: Local web server integration
- ✅ Requirement 1.1: Signal generation API integration
- ✅ Requirement 2.1: Signal processing API integration
- ✅ Requirement 5.1: Trigger system API integration
