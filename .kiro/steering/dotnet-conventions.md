---
inclusion: always
---

# .NET Coding Conventions

## Naming Conventions

### General Rules
- Use PascalCase for class names, method names, properties, and public fields
- Use camelCase for local variables and private fields
- Use UPPER_CASE for constants
- Prefix interfaces with `I` (e.g., `ISignalGenerator`)
- Use meaningful, descriptive names that convey intent

### Examples
```csharp
// Classes and Interfaces
public class SignalGenerator : ISignalGenerator { }
public interface ISignalProcessor { }

// Methods and Properties
public async Task<Signal> GenerateSignal(SignalGeneratorParams parameters) { }
public double Frequency { get; set; }

// Private fields (with underscore prefix)
private readonly ISignalDatabase _database;
private double _threshold;

// Local variables
var signalData = await _generator.Generate(params);
int sampleCount = signal.Samples.Length;

// Constants
private const int MAX_SAMPLE_RATE = 192000;
```

## Project Structure (Onion Architecture)

### Layer Organization
```
Core/           - Domain entities, interfaces, value objects (no dependencies)
Application/    - Use cases, services, DTOs (depends on Core only)
Infrastructure/ - Database, external services (implements Core interfaces)
Api/            - Controllers, API models (depends on Application)
```

### Dependency Rules
- Core has NO dependencies on other layers
- Application depends ONLY on Core
- Infrastructure implements Core interfaces
- Api depends on Application (not Infrastructure directly)
- Use dependency injection to wire up implementations

## File Organization

### One Class Per File
- Each class, interface, or enum should be in its own file
- File name must match the type name exactly
- Place related types in the same namespace/folder

### Namespace Structure
```csharp
namespace SignalProcessing.Core.Entities;
namespace SignalProcessing.Core.Interfaces;
namespace SignalProcessing.Application.Services;
namespace SignalProcessing.Infrastructure.Data;
namespace SignalProcessing.Api.Controllers;
```

## Code Style

### Use Modern C# Features
```csharp
// Records for immutable data
public record SignalGeneratorParams(
    SignalType Type,
    double Frequency,
    double Amplitude
);

// Nullable reference types
public Signal? GetSignal(Guid id);

// Pattern matching
var result = signal switch
{
    { Type: SignalType.Sine } => ProcessSine(signal),
    { Type: SignalType.Square } => ProcessSquare(signal),
    _ => throw new NotSupportedException()
};

// Expression-bodied members
public bool IsValid => Frequency > 0 && Amplitude > 0;
```

### Async/Await
- Use async/await for I/O operations (database, file system, network)
- Suffix async methods with `Async` (e.g., `GenerateSignalAsync`)
- Always use `ConfigureAwait(false)` in library code
- Return `Task<T>` or `Task` for async methods

```csharp
public async Task<Signal> GenerateAsync(SignalGeneratorParams parameters)
{
    var signal = CreateSignal(parameters);
    await _database.SaveSignalAsync(signal).ConfigureAwait(false);
    return signal;
}
```

### Exception Handling
- Use specific exception types
- Don't catch exceptions you can't handle
- Log exceptions with context
- Use custom exceptions for domain errors

```csharp
public class InvalidSignalParametersException : Exception
{
    public InvalidSignalParametersException(string message) : base(message) { }
}

// Usage
if (parameters.Frequency <= 0)
{
    throw new InvalidSignalParametersException("Frequency must be positive");
}
```

## Dependency Injection

### Constructor Injection
```csharp
public class SignalGenerationService
{
    private readonly ISignalGenerator _generator;
    private readonly ISignalDatabase _database;
    private readonly ILogger<SignalGenerationService> _logger;

    public SignalGenerationService(
        ISignalGenerator generator,
        ISignalDatabase database,
        ILogger<SignalGenerationService> logger)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### Registration in Program.cs
```csharp
// Core services
builder.Services.AddScoped<ISignalGenerator, SignalGenerator>();
builder.Services.AddScoped<ISignalProcessor, SignalProcessor>();

// Application services
builder.Services.AddScoped<SignalGenerationService>();
builder.Services.AddScoped<SignalProcessingService>();

// Infrastructure
builder.Services.AddSingleton<ITimeSeriesDatabase, InfluxDbAdapter>();
builder.Services.AddSingleton<IMetadataDatabase, MongoDbAdapter>();
```

## Validation

### Use Data Annotations for DTOs
```csharp
public class SignalGenerationRequest
{
    [Required]
    [EnumDataType(typeof(SignalType))]
    public SignalType Type { get; set; }

    [Range(0.1, 20000)]
    public double Frequency { get; set; }

    [Range(0.01, 100)]
    public double Amplitude { get; set; }
}
```

### Domain Validation in Entities
```csharp
public class Signal
{
    public Guid Id { get; private set; }
    public double Frequency { get; private set; }

    public void SetFrequency(double frequency)
    {
        if (frequency <= 0)
            throw new ArgumentException("Frequency must be positive", nameof(frequency));
        
        Frequency = frequency;
    }
}
```

## Comments and Documentation

### XML Documentation for Public APIs
```csharp
/// <summary>
/// Generates a signal based on the specified parameters.
/// </summary>
/// <param name="parameters">The signal generation parameters.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains the generated signal.</returns>
/// <exception cref="InvalidSignalParametersException">Thrown when parameters are invalid.</exception>
public async Task<Signal> GenerateAsync(SignalGeneratorParams parameters)
{
    // Implementation
}
```

### Inline Comments
- Use comments to explain WHY, not WHAT
- Avoid obvious comments
- Keep comments up-to-date with code changes

```csharp
// Good: Explains reasoning
// Use Butterworth filter for smooth frequency response
var filter = new ButterworthFilter(order: 4);

// Bad: States the obvious
// Set frequency to 1000
frequency = 1000;
```

## LINQ and Collections

### Prefer LINQ for Readability
```csharp
// Good
var recentSignals = signals
    .Where(s => s.CreatedAt > DateTime.UtcNow.AddDays(-7))
    .OrderByDescending(s => s.CreatedAt)
    .Take(10)
    .ToList();

// Avoid complex nested loops when LINQ is clearer
```

### Use Collection Expressions (C# 12+)
```csharp
// Collection expressions
int[] numbers = [1, 2, 3, 4, 5];
List<Signal> signals = [signal1, signal2, signal3];
```

## Testing Conventions

### Test Class Naming
- Name test classes after the class being tested with `Tests` suffix
- Example: `SignalGeneratorTests`, `SignalProcessorTests`

### Test Method Naming
- Use descriptive names: `MethodName_Scenario_ExpectedBehavior`
- Example: `Generate_WithValidParams_ReturnsSignal`
- Example: `Process_WithInvalidCutoff_ThrowsException`

### Arrange-Act-Assert Pattern
```csharp
[Fact]
public async Task Generate_WithValidParams_ReturnsSignalWithCorrectFrequency()
{
    // Arrange
    var generator = new SignalGenerator();
    var parameters = new SignalGeneratorParams(SignalType.Sine, 1000, 1.0, 0, 1.0, 44100);

    // Act
    var signal = await generator.GenerateAsync(parameters);

    // Assert
    Assert.NotNull(signal);
    Assert.Equal(1000, signal.Metadata.Frequency);
}
```
