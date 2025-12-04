# Visualization Components

This directory contains the React UI components for the Signal Processing Visualization application.

## Components

### SignalChart
**File:** `SignalChart.tsx`

Interactive chart component for visualizing signal waveforms using Recharts.

**Features:**
- Displays time-domain signal data with time on x-axis and amplitude on y-axis
- Supports rendering both original and processed signals simultaneously
- Zoom and pan capabilities via brush control
- Automatic downsampling for datasets > 10,000 points for performance
- Responsive design with configurable height

**Props:**
- `signalData`: Original signal data (samples, timestamps, metadata)
- `processedData`: Processed signal data (optional)
- `height`: Chart height in pixels (default: 400)

**Requirements:** 3.1, 3.2, 3.3, 3.5

---

### ParameterPanel
**File:** `ParameterPanel.tsx`

Form component for configuring signal generation parameters.

**Features:**
- Input fields for signal type, frequency, amplitude, phase, duration, and sample rate
- Real-time validation with immediate error feedback
- Validates Nyquist criterion (sample rate ≥ 2× frequency)
- Loading state during signal generation
- Prevents submission with invalid parameters

**Props:**
- `onGenerateSignal`: Callback function to generate signal with parameters
- `isLoading`: Loading state flag (optional)

**Validation Rules:**
- Frequency: 0.1 - 20,000 Hz
- Amplitude: 0.01 - 100
- Phase: -2π to 2π (-6.28 to 6.28 radians)
- Duration: 0.1 - 60 seconds
- Sample Rate: ≥ 2× frequency (Nyquist criterion)

**Requirements:** 1.1, 1.4, 1.5, 4.1, 4.2

---

### ProcessingPanel
**File:** `ProcessingPanel.tsx`

Form component for applying signal processing operations.

**Features:**
- Dropdown for selecting operation type (Low-Pass, High-Pass, Band-Pass, Gain)
- Conditional inputs based on selected operation
- Real-time validation for processing parameters
- Validates cutoff frequencies against Nyquist frequency
- Prevents processing without a selected signal

**Props:**
- `currentSignalId`: ID of the signal to process
- `onProcessSignal`: Callback function to apply processing
- `isLoading`: Loading state flag (optional)

**Operations:**
- **Low-Pass Filter**: Cutoff frequency, filter order
- **High-Pass Filter**: Cutoff frequency, filter order
- **Band-Pass Filter**: Low cutoff, high cutoff, filter order
- **Gain Adjustment**: Gain multiplier

**Validation Rules:**
- Cutoff Frequency: > 0 and < Nyquist frequency (22,050 Hz)
- Gain: 0.01 - 100
- Filter Order: 1 - 10 (integer)
- Band-Pass: Low cutoff < High cutoff

**Requirements:** 2.1, 2.5, 4.1, 4.3, 4.4

---

### SignalSelector
**File:** `SignalSelector.tsx`

Dropdown component for selecting signals from history.

**Features:**
- Lists recent signals from database
- Displays signal metadata (type, frequency, creation time)
- Auto-selects first signal when available
- Refresh button to reload signal list
- Detailed metadata view for selected signal

**Props:**
- `signals`: Array of available signals
- `selectedSignalId`: Currently selected signal ID
- `onSelectSignal`: Callback when signal is selected
- `onRefresh`: Callback to refresh signal list
- `isLoading`: Loading state flag (optional)

**Requirements:** 6.3

---

## Property-Based Tests

Each component has comprehensive property-based tests using `fast-check` and `vitest`:

### SignalChart.test.tsx
- **Property 5**: Chart renders signal data with axes for any valid signal
- Tests rendering with various signal sizes
- Tests dual-signal rendering (original + processed)
- Tests downsampling indication for large datasets

### ParameterPanel.test.tsx
- **Property 6**: Parameter validation feedback for all input types
- **Property 8**: Invalid parameter prevention
- Tests validation for frequency, amplitude, phase, duration, sample rate
- Tests Nyquist criterion validation
- Tests form submission prevention with invalid data

### ProcessingPanel.test.tsx
- **Property 7**: Parameter application triggers processing with exact parameters
- Tests all operation types (Low-Pass, High-Pass, Band-Pass, Gain)
- Tests parameter validation for each operation
- Tests prevention of processing without signal

---

## Testing

To run the property-based tests:

```bash
# Install dependencies first
npm install

# Run tests
npm test
```

Each property test runs 100 iterations (50 for async tests) as specified in the design document.

---

## Usage Example

```tsx
import { 
  SignalChart, 
  ParameterPanel, 
  ProcessingPanel, 
  SignalSelector 
} from './components';

function App() {
  const [signalData, setSignalData] = useState(null);
  const [processedData, setProcessedData] = useState(null);
  const [signals, setSignals] = useState([]);
  const [selectedId, setSelectedId] = useState(null);

  const handleGenerate = async (params) => {
    const signal = await api.signals.generateSignal(params);
    setSignalData(signal);
  };

  const handleProcess = async (params) => {
    const processed = await api.processing.processSignal(params);
    setProcessedData(processed);
  };

  return (
    <div>
      <ParameterPanel onGenerateSignal={handleGenerate} />
      <ProcessingPanel 
        currentSignalId={selectedId}
        onProcessSignal={handleProcess} 
      />
      <SignalChart 
        signalData={signalData} 
        processedData={processedData} 
      />
      <SignalSelector
        signals={signals}
        selectedSignalId={selectedId}
        onSelectSignal={setSelectedId}
        onRefresh={loadSignals}
      />
    </div>
  );
}
```

---

## Dependencies

- **React**: UI framework
- **Recharts**: Charting library for signal visualization
- **TypeScript**: Type safety
- **Vitest**: Testing framework
- **@testing-library/react**: React component testing utilities
- **fast-check**: Property-based testing library
