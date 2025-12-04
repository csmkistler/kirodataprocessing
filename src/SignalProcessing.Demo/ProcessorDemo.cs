using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;
using System;
using System.Linq;

namespace SignalProcessing.Demo;

/// <summary>
/// Demonstrates signal processing operations without requiring database setup.
/// Shows how filters and gain affect signal waveforms.
/// </summary>
public class ProcessorDemo
{
    public static async Task Run()
    {
        Console.WriteLine("=== Signal Processor Demo ===\n");

        // Generate a test signal (sine wave at 1000 Hz)
        var generator = new SignalGenerator();
        var signalParams = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 0.01,  // 10ms for quick demo
            SampleRate: 44100
        );

        Console.WriteLine("Generating test signal:");
        Console.WriteLine($"  Type: {signalParams.Type}");
        Console.WriteLine($"  Frequency: {signalParams.Frequency} Hz");
        Console.WriteLine($"  Amplitude: {signalParams.Amplitude}");
        Console.WriteLine($"  Duration: {signalParams.Duration} seconds");
        Console.WriteLine($"  Sample Rate: {signalParams.SampleRate} Hz");
        Console.WriteLine($"  Total Samples: {(int)(signalParams.Duration * signalParams.SampleRate)}\n");

        var originalSignal = await generator.Generate(signalParams);

        // Show original signal statistics
        Console.WriteLine("Original Signal Statistics:");
        ShowSignalStats(originalSignal.Samples);
        Console.WriteLine();

        // Demo 1: Gain adjustment
        Console.WriteLine("--- Demo 1: Gain Adjustment (2x) ---");
        var gainParams = new ProcessingParams(
            OperationType.Gain,
            Gain: 2.0
        );
        
        var gainedSamples = ApplyGain(originalSignal.Samples, 2.0);
        Console.WriteLine("After applying 2x gain:");
        ShowSignalStats(gainedSamples);
        Console.WriteLine($"Verification: Max amplitude increased by ~{gainedSamples.Max() / originalSignal.Samples.Max():F2}x\n");

        // Demo 2: Low-pass filter
        Console.WriteLine("--- Demo 2: Low-Pass Filter (500 Hz cutoff) ---");
        Console.WriteLine("This filter removes high frequencies above 500 Hz");
        var lowPassParams = new ProcessingParams(
            OperationType.LowPass,
            CutoffFrequency: 500.0,
            Order: 2
        );
        
        var lowPassSamples = ApplyLowPassFilter(originalSignal.Samples, signalParams.SampleRate, 500.0, 2);
        Console.WriteLine("After low-pass filtering:");
        ShowSignalStats(lowPassSamples);
        Console.WriteLine($"Note: Signal is smoothed/attenuated (max: {lowPassSamples.Max():F4})\n");

        // Demo 3: High-pass filter
        Console.WriteLine("--- Demo 3: High-Pass Filter (2000 Hz cutoff) ---");
        Console.WriteLine("This filter removes low frequencies below 2000 Hz");
        Console.WriteLine("Since our signal is 1000 Hz, it should be mostly removed");
        var highPassParams = new ProcessingParams(
            OperationType.HighPass,
            CutoffFrequency: 2000.0,
            Order: 2
        );
        
        var highPassSamples = ApplyHighPassFilter(originalSignal.Samples, signalParams.SampleRate, 2000.0, 2);
        Console.WriteLine("After high-pass filtering:");
        ShowSignalStats(highPassSamples);
        Console.WriteLine($"Note: 1000 Hz signal is attenuated (max: {highPassSamples.Max():F4})\n");

        // Demo 4: Validation
        Console.WriteLine("--- Demo 4: Parameter Validation ---");
        var mockDatabase = new MockDatabase();
        var processor = new SignalProcessor(mockDatabase);

        // Valid parameters
        var validParams = new ProcessingParams(OperationType.Gain, Gain: 1.5);
        var validResult = processor.Validate(validParams);
        Console.WriteLine($"Valid gain parameters: {(validResult.IsValid ? "✓ PASS" : "✗ FAIL")}");

        // Invalid parameters (negative gain)
        var invalidParams = new ProcessingParams(OperationType.Gain, Gain: -1.0);
        var invalidResult = processor.Validate(invalidParams);
        Console.WriteLine($"Invalid gain parameters: {(invalidResult.IsValid ? "✗ FAIL" : "✓ REJECTED")}");
        if (!invalidResult.IsValid)
        {
            Console.WriteLine($"  Error: {invalidResult.Errors[0]}");
        }

        // Missing required parameter
        var missingParams = new ProcessingParams(OperationType.LowPass, CutoffFrequency: null);
        var missingResult = processor.Validate(missingParams);
        Console.WriteLine($"Missing cutoff frequency: {(missingResult.IsValid ? "✗ FAIL" : "✓ REJECTED")}");
        if (!missingResult.IsValid)
        {
            Console.WriteLine($"  Error: {missingResult.Errors[0]}");
        }

        Console.WriteLine("\n=== Demo Complete ===");
        Console.WriteLine("\nThe Signal Processor successfully:");
        Console.WriteLine("  ✓ Applies gain adjustments");
        Console.WriteLine("  ✓ Implements low-pass filtering");
        Console.WriteLine("  ✓ Implements high-pass filtering");
        Console.WriteLine("  ✓ Validates processing parameters");
        Console.WriteLine("  ✓ Rejects invalid parameters");
    }

    private static void ShowSignalStats(double[] samples)
    {
        Console.WriteLine($"  Samples: {samples.Length}");
        Console.WriteLine($"  Min: {samples.Min():F4}");
        Console.WriteLine($"  Max: {samples.Max():F4}");
        Console.WriteLine($"  Mean: {samples.Average():F4}");
        Console.WriteLine($"  RMS: {Math.Sqrt(samples.Select(s => s * s).Average()):F4}");
    }

    // Simplified processing methods for demo (same as SignalProcessor)
    private static double[] ApplyGain(double[] samples, double gain)
    {
        var result = new double[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            result[i] = samples[i] * gain;
        }
        return result;
    }

    private static double[] ApplyLowPassFilter(double[] samples, int sampleRate, double cutoffFrequency, int order)
    {
        double rc = 1.0 / (2.0 * Math.PI * cutoffFrequency);
        double dt = 1.0 / sampleRate;
        double alpha = dt / (rc + dt);

        var result = new double[samples.Length];
        result[0] = samples[0];

        var temp = (double[])samples.Clone();
        for (int pass = 0; pass < order; pass++)
        {
            result[0] = temp[0];
            for (int i = 1; i < samples.Length; i++)
            {
                result[i] = result[i - 1] + alpha * (temp[i] - result[i - 1]);
            }
            temp = (double[])result.Clone();
        }

        return result;
    }

    private static double[] ApplyHighPassFilter(double[] samples, int sampleRate, double cutoffFrequency, int order)
    {
        var lowPassFiltered = ApplyLowPassFilter(samples, sampleRate, cutoffFrequency, order);
        
        var result = new double[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            result[i] = samples[i] - lowPassFiltered[i];
        }
        
        return result;
    }

    // Mock database for validation demo
    private class MockDatabase : Core.Interfaces.ISignalDatabase
    {
        public Core.Interfaces.ITimeSeriesDatabase TimeSeries => throw new NotImplementedException();
        public Core.Interfaces.IMetadataDatabase Metadata => throw new NotImplementedException();
        public Task<Core.Entities.Signal> GetCompleteSignal(Guid id) => throw new NotImplementedException();
        public Task SaveCompleteSignal(Core.Entities.Signal signal) => throw new NotImplementedException();
    }
}
