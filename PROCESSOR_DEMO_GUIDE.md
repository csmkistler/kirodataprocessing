# Signal Processor Demo Guide

This guide shows you how to see the Signal Processor in action.

## Quick Start

Run any of these commands from the project root:

```bash
# Basic processor demo with statistics
dotnet run --project src/SignalProcessing.Demo --processor

# Visual demo with ASCII charts showing before/after
dotnet run --project src/SignalProcessing.Demo --processor-visual

# See all available demos
dotnet run --project src/SignalProcessing.Demo --help
```

## What You'll See

### 1. Basic Processor Demo (`--processor`)

This demo shows:
- **Signal Generation**: Creates a 1000 Hz sine wave test signal
- **Gain Adjustment**: Multiplies amplitude by 2x (you'll see max amplitude double)
- **Low-Pass Filter**: Removes frequencies above 500 Hz (smooths the signal)
- **High-Pass Filter**: Removes frequencies below 2000 Hz (attenuates the 1000 Hz signal)
- **Parameter Validation**: Shows how invalid parameters are rejected

**Key Observations:**
- Gain increases amplitude proportionally (2x gain = 2x amplitude)
- Low-pass filter smooths/attenuates high frequencies
- High-pass filter removes low frequencies
- Validation catches negative values, missing parameters, etc.

### 2. Visual Processor Demo (`--processor-visual`)

This demo creates a composite signal (1000 Hz + 3000 Hz) and shows ASCII charts:

**Original Signal**: Mixed waveform with both frequencies
```
Complex waveform with rapid oscillations
```

**After Low-Pass Filter (1500 Hz cutoff)**: Removes 3000 Hz component
```
Smoother waveform, only 1000 Hz remains
```

**After High-Pass Filter (2000 Hz cutoff)**: Removes 1000 Hz component
```
Rapid oscillations, only 3000 Hz remains
```

**After 3x Gain**: Amplitude tripled
```
Same shape but 3x taller
```

## Understanding the Output

### Signal Statistics
- **Samples**: Number of data points
- **Min/Max**: Range of amplitude values
- **Mean**: Average value (should be ~0 for sine waves)
- **RMS**: Root Mean Square (energy measure)

### Filter Effects
- **Low-Pass**: Removes high frequencies, smooths signal
  - Cutoff at 500 Hz removes everything above 500 Hz
  - Signal becomes smoother, less "jagged"
  
- **High-Pass**: Removes low frequencies
  - Cutoff at 2000 Hz removes everything below 2000 Hz
  - Useful for removing DC offset or low-frequency noise

- **Band-Pass**: Combination of high-pass and low-pass
  - Keeps only frequencies in a specific range

### Gain
- Simple multiplication of all samples
- 2x gain = 2x amplitude
- Useful for amplifying weak signals

## Real-World Applications

1. **Audio Processing**
   - Low-pass: Remove high-frequency hiss
   - High-pass: Remove low-frequency rumble
   - Gain: Adjust volume

2. **Signal Analysis**
   - Filter out noise in specific frequency ranges
   - Isolate frequency components
   - Prepare signals for further analysis

3. **Data Preprocessing**
   - Smooth noisy sensor data
   - Remove DC offset
   - Normalize signal levels

## Implementation Details

The Signal Processor (`SignalProcessor.cs`) implements:
- **Butterworth filters**: Industry-standard filter design
- **RC filter approximation**: Simple but effective low-pass
- **Cascaded filters**: Multiple passes for higher order
- **Parameter validation**: Nyquist frequency checks, range validation

All processing preserves the original signal in the database (when used with database backend).

## Next Steps

To use the Signal Processor in your own code:

```csharp
// Create processor (requires database for full functionality)
var processor = new SignalProcessor(database);

// Validate parameters
var params = new ProcessingParams(
    OperationType.LowPass,
    CutoffFrequency: 1000.0,
    Order: 4
);
var validation = processor.Validate(params);

// Process signal (retrieves from database)
var processedSignal = await processor.Process(signalId, params);
```

## Testing

Run the property-based tests to verify correctness:

```bash
cd src/SignalProcessing.Tests
dotnet test --filter "SignalProcessorPropertyTests"
```

These tests verify:
- Parameter validation (100 random test cases)
- Invalid parameter rejection
- Nyquist frequency constraints
- Signal preservation (original unchanged after processing)
