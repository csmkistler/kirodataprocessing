using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;
using System;
using System.Linq;

namespace SignalProcessing.Demo;

/// <summary>
/// Visual demonstration of signal processing with ASCII charts showing before/after comparison.
/// </summary>
public class ProcessorVisualDemo
{
    public static async Task Run()
    {
        Console.WriteLine("=== Signal Processor Visual Demo ===\n");

        // Generate a composite signal (mix of frequencies)
        var generator = new SignalGenerator();
        
        Console.WriteLine("Creating a composite signal (1000 Hz + 3000 Hz)...\n");
        
        // Generate 1000 Hz component
        var signal1000 = await generator.Generate(new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            Amplitude: 0.7,
            Phase: 0.0,
            Duration: 0.005,
            SampleRate: 44100
        ));

        // Generate 3000 Hz component
        var signal3000 = await generator.Generate(new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 3000.0,
            Amplitude: 0.3,
            Phase: 0.0,
            Duration: 0.005,
            SampleRate: 44100
        ));

        // Mix the signals
        var mixedSamples = new double[signal1000.Samples.Length];
        for (int i = 0; i < mixedSamples.Length; i++)
        {
            mixedSamples[i] = signal1000.Samples[i] + signal3000.Samples[i];
        }

        Console.WriteLine("Original Mixed Signal (1000 Hz + 3000 Hz):");
        PlotSignal(mixedSamples, 0, 100);
        Console.WriteLine();

        // Apply low-pass filter to remove 3000 Hz
        Console.WriteLine("After Low-Pass Filter (1500 Hz cutoff - removes 3000 Hz):");
        var lowPassSamples = ApplyLowPassFilter(mixedSamples, 44100, 1500.0, 4);
        PlotSignal(lowPassSamples, 0, 100);
        Console.WriteLine("Notice: Signal is smoother, high frequency removed\n");

        // Apply high-pass filter to remove 1000 Hz
        Console.WriteLine("After High-Pass Filter (2000 Hz cutoff - removes 1000 Hz):");
        var highPassSamples = ApplyHighPassFilter(mixedSamples, 44100, 2000.0, 4);
        PlotSignal(highPassSamples, 0, 100);
        Console.WriteLine("Notice: Only high frequency component remains\n");

        // Apply gain
        Console.WriteLine("After 3x Gain:");
        var gainedSamples = mixedSamples.Select(s => s * 3.0).ToArray();
        PlotSignal(gainedSamples, 0, 100);
        Console.WriteLine("Notice: Amplitude is 3x larger\n");

        Console.WriteLine("=== Demo Complete ===");
    }

    private static void PlotSignal(double[] samples, int start, int count)
    {
        const int height = 15;
        const int width = 80;
        
        var subset = samples.Skip(start).Take(Math.Min(count, samples.Length - start)).ToArray();
        if (subset.Length == 0) return;

        var max = subset.Max();
        var min = subset.Min();
        var range = max - min;
        if (range < 0.0001) range = 1.0;

        // Create the plot
        for (int row = 0; row < height; row++)
        {
            var threshold = max - (row * range / (height - 1));
            Console.Write("  ");
            
            for (int col = 0; col < Math.Min(width, subset.Length); col++)
            {
                var sampleIndex = (int)(col * (double)subset.Length / width);
                var value = subset[sampleIndex];
                
                if (row == height / 2)
                {
                    Console.Write(Math.Abs(value - threshold) < range / height ? "━" : " ");
                }
                else if (value >= threshold && (row == 0 || subset[sampleIndex] < (max - ((row - 1) * range / (height - 1)))))
                {
                    Console.Write("█");
                }
                else
                {
                    Console.Write(" ");
                }
            }
            
            if (row == 0)
                Console.WriteLine($"  {max:F3}");
            else if (row == height / 2)
                Console.WriteLine($"  0.000");
            else if (row == height - 1)
                Console.WriteLine($"  {min:F3}");
            else
                Console.WriteLine();
        }
        
        Console.WriteLine($"  Samples: {subset.Length}, Range: [{min:F3}, {max:F3}]");
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
}
