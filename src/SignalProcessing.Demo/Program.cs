using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;
using SignalProcessing.Demo;

// Check command line arguments
if (args.Length > 0)
{
    if (args[0] == "--visual")
    {
        await VisualDemo.Run();
        return;
    }
    else if (args[0] == "--database" || args[0] == "--db")
    {
        await DatabaseDemo.RunDemo();
        return;
    }
    else if (args[0] == "--processor" || args[0] == "--process")
    {
        await ProcessorDemo.Run();
        return;
    }
    else if (args[0] == "--processor-visual" || args[0] == "--pv")
    {
        await ProcessorVisualDemo.Run();
        return;
    }
    else if (args[0] == "--help" || args[0] == "-h")
    {
        Console.WriteLine("Signal Processing Demo");
        Console.WriteLine("\nUsage:");
        Console.WriteLine("  dotnet run                      Run basic signal generation demo");
        Console.WriteLine("  dotnet run --visual             Run visual ASCII chart demo");
        Console.WriteLine("  dotnet run --processor          Run signal processor demo");
        Console.WriteLine("  dotnet run --processor-visual   Run visual processor demo (before/after)");
        Console.WriteLine("  dotnet run --database           Run database persistence demo");
        Console.WriteLine("  dotnet run --help               Show this help message");
        return;
    }
}

Console.WriteLine("=== Signal Processing Demo ===\n");

var generator = new SignalGenerator();

// Demo 1: Generate a sine wave
Console.WriteLine("1. Generating a 440 Hz Sine Wave (A note)");
Console.WriteLine("   Duration: 0.01 seconds, Sample Rate: 44100 Hz");
var sineParams = new SignalGeneratorParams(
    SignalType.Sine,
    Frequency: 440.0,
    Amplitude: 1.0,
    Phase: 0.0,
    Duration: 0.01,
    SampleRate: 44100
);

var sineSignal = await generator.Generate(sineParams);
Console.WriteLine($"   Generated {sineSignal.Samples.Length} samples");
Console.WriteLine($"   Signal ID: {sineSignal.Id}");
Console.WriteLine($"   First 10 samples: {string.Join(", ", sineSignal.Samples.Take(10).Select(s => s.ToString("F4")))}");
Console.WriteLine($"   Max amplitude: {sineSignal.Samples.Max():F4}, Min amplitude: {sineSignal.Samples.Min():F4}\n");

// Demo 2: Generate a square wave
Console.WriteLine("2. Generating a 100 Hz Square Wave");
Console.WriteLine("   Duration: 0.05 seconds, Sample Rate: 10000 Hz");
var squareParams = new SignalGeneratorParams(
    SignalType.Square,
    Frequency: 100.0,
    Amplitude: 2.0,
    Phase: 0.0,
    Duration: 0.05,
    SampleRate: 10000
);

var squareSignal = await generator.Generate(squareParams);
Console.WriteLine($"   Generated {squareSignal.Samples.Length} samples");
Console.WriteLine($"   First 20 samples: {string.Join(", ", squareSignal.Samples.Take(20).Select(s => s.ToString("F1")))}");
Console.WriteLine($"   Unique values: {string.Join(", ", squareSignal.Samples.Distinct().OrderBy(x => x).Select(s => s.ToString("F1")))}\n");

// Demo 3: Generate a sawtooth wave
Console.WriteLine("3. Generating a 50 Hz Sawtooth Wave");
Console.WriteLine("   Duration: 0.04 seconds, Sample Rate: 5000 Hz");
var sawtoothParams = new SignalGeneratorParams(
    SignalType.Sawtooth,
    Frequency: 50.0,
    Amplitude: 1.5,
    Phase: 0.0,
    Duration: 0.04,
    SampleRate: 5000
);

var sawtoothSignal = await generator.Generate(sawtoothParams);
Console.WriteLine($"   Generated {sawtoothSignal.Samples.Length} samples");
Console.WriteLine($"   First 15 samples: {string.Join(", ", sawtoothSignal.Samples.Take(15).Select(s => s.ToString("F4")))}");
Console.WriteLine($"   Max: {sawtoothSignal.Samples.Max():F4}, Min: {sawtoothSignal.Samples.Min():F4}\n");

// Demo 4: Generate white noise
Console.WriteLine("4. Generating White Noise");
Console.WriteLine("   Duration: 0.01 seconds, Sample Rate: 8000 Hz");
var noiseParams = new SignalGeneratorParams(
    SignalType.Noise,
    Frequency: 1000.0, // Not used for noise
    Amplitude: 0.5,
    Phase: 0.0,
    Duration: 0.01,
    SampleRate: 8000
);

var noiseSignal = await generator.Generate(noiseParams);
Console.WriteLine($"   Generated {noiseSignal.Samples.Length} samples");
Console.WriteLine($"   First 10 samples: {string.Join(", ", noiseSignal.Samples.Take(10).Select(s => s.ToString("F4")))}");
Console.WriteLine($"   Max: {noiseSignal.Samples.Max():F4}, Min: {noiseSignal.Samples.Min():F4}");
Console.WriteLine($"   Average: {noiseSignal.Samples.Average():F4}\n");

// Demo 5: Validation test
Console.WriteLine("5. Testing Parameter Validation");
var invalidParams = new SignalGeneratorParams(
    SignalType.Sine,
    Frequency: -100.0, // Invalid: negative
    Amplitude: 1.0,
    Phase: 10.0, // Invalid: outside [-2π, 2π]
    Duration: 1.0,
    SampleRate: 100 // Invalid: violates Nyquist (should be >= 200)
);

var validationResult = generator.Validate(invalidParams);
Console.WriteLine($"   Validation result: {(validationResult.IsValid ? "VALID" : "INVALID")}");
if (!validationResult.IsValid)
{
    Console.WriteLine("   Errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"   - {error}");
    }
}

Console.WriteLine("\n=== Demo Complete ===");
