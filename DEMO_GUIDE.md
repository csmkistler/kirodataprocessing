# Signal Processing Demo Guide

## Running the Demos

### Basic Demo (Data Output)
Shows raw signal data and validation examples:
```bash
dotnet run --project src/SignalProcessing.Demo/SignalProcessing.Demo.csproj
```

### Visual Demo (ASCII Graphs)
Shows beautiful ASCII art visualizations of signals:
```bash
dotnet run --project src/SignalProcessing.Demo/SignalProcessing.Demo.csproj -- --visual
```

## What You Can See

### 1. Basic Demo Output
- **Sine Wave**: 440 Hz (musical note A), shows smooth oscillation
- **Square Wave**: 100 Hz, shows binary high/low values
- **Sawtooth Wave**: 50 Hz, shows linear ramp pattern
- **White Noise**: Random values within amplitude range
- **Validation**: Shows how invalid parameters are rejected

### 2. Visual Demo Output
- **Sine Wave Graph**: Smooth sinusoidal pattern
- **Square Wave Graph**: Sharp transitions between high and low
- **Sawtooth Wave Graph**: Linear ramps that reset
- **White Noise Graph**: Random scattered pattern
- **Amplitude Comparison**: Shows how amplitude affects signal strength

## Running Tests

### All Tests
```bash
dotnet test src/SignalProcessing.Tests/SignalProcessing.Tests.csproj
```

### Property-Based Tests Only
```bash
dotnet test src/SignalProcessing.Tests/SignalProcessing.Tests.csproj --filter "FullyQualifiedName~PropertyTests"
```

## What's Been Implemented

✅ **Signal Generator Component** (Task 2)
- ✅ 2.1: SignalGeneratorParams with validation
- ✅ 2.2: Property tests for parameter validation (5 tests)
- ✅ 2.3: Signal generation algorithms (Sine, Square, Sawtooth, Noise)
- ✅ 2.4: Property tests for signal generation (3 tests)

## Test Coverage

**8 Property-Based Tests** running 100 iterations each:
1. Invalid frequency rejection
2. Invalid amplitude rejection
3. Invalid duration rejection
4. Invalid phase rejection
5. Nyquist criterion violation rejection
6. Signal generation reflects parameters
7. Signal generation works for all types
8. Signal amplitude is respected

## Key Features

- **Validation**: Comprehensive parameter validation with clear error messages
- **All Signal Types**: Sine, Square, Sawtooth, and White Noise
- **Property-Based Testing**: 800+ test cases automatically generated
- **Visual Feedback**: ASCII art graphs for signal visualization
- **Type Safety**: Full C# type safety with nullable reference types

## Next Steps

The next task in the implementation plan is:
- **Task 3**: Implement database layer (MongoDB + InfluxDB)
- **Task 4**: Implement Signal Processor component
- **Task 5**: Implement Trigger Component

## Example Usage in Code

```csharp
using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;

var generator = new SignalGenerator();

// Create parameters
var parameters = new SignalGeneratorParams(
    SignalType.Sine,
    Frequency: 440.0,      // Hz
    Amplitude: 1.0,        // Arbitrary units
    Phase: 0.0,            // Radians
    Duration: 1.0,         // Seconds
    SampleRate: 44100      // Samples per second
);

// Validate (optional - Generate will validate automatically)
var validation = generator.Validate(parameters);
if (!validation.IsValid)
{
    Console.WriteLine($"Errors: {string.Join(", ", validation.Errors)}");
    return;
}

// Generate signal
var signal = await generator.Generate(parameters);

// Use the signal
Console.WriteLine($"Generated {signal.Samples.Length} samples");
Console.WriteLine($"Signal ID: {signal.Id}");
Console.WriteLine($"Created at: {signal.CreatedAt}");
```
