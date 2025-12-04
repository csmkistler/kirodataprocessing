using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;

namespace SignalProcessing.Demo;

public static class VisualDemo
{
    public static async Task Run()
    {
        Console.WriteLine("=== Signal Processing Visual Demo ===\n");

        var generator = new SignalGenerator();

        // Demo 1: Sine Wave
        Console.WriteLine("1. SINE WAVE - 5 Hz");
        var sineParams = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 5.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 1.0,
            SampleRate: 200
        );
        var sineSignal = await generator.Generate(sineParams);
        SignalVisualizer.PlotSignal(sineSignal.Samples, "Sine Wave (5 Hz, 1 second)", width: 100, height: 15);

        // Demo 2: Square Wave
        Console.WriteLine("\n2. SQUARE WAVE - 3 Hz");
        var squareParams = new SignalGeneratorParams(
            SignalType.Square,
            Frequency: 3.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 1.0,
            SampleRate: 200
        );
        var squareSignal = await generator.Generate(squareParams);
        SignalVisualizer.PlotSignal(squareSignal.Samples, "Square Wave (3 Hz, 1 second)", width: 100, height: 15);

        // Demo 3: Sawtooth Wave
        Console.WriteLine("\n3. SAWTOOTH WAVE - 4 Hz");
        var sawtoothParams = new SignalGeneratorParams(
            SignalType.Sawtooth,
            Frequency: 4.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 1.0,
            SampleRate: 200
        );
        var sawtoothSignal = await generator.Generate(sawtoothParams);
        SignalVisualizer.PlotSignal(sawtoothSignal.Samples, "Sawtooth Wave (4 Hz, 1 second)", width: 100, height: 15);

        // Demo 4: White Noise
        Console.WriteLine("\n4. WHITE NOISE");
        var noiseParams = new SignalGeneratorParams(
            SignalType.Noise,
            Frequency: 1.0,
            Amplitude: 0.8,
            Phase: 0.0,
            Duration: 1.0,
            SampleRate: 200
        );
        var noiseSignal = await generator.Generate(noiseParams);
        SignalVisualizer.PlotSignal(noiseSignal.Samples, "White Noise (1 second)", width: 100, height: 15);

        // Demo 5: Compare different amplitudes
        Console.WriteLine("\n5. AMPLITUDE COMPARISON - Sine Waves");
        var amp1 = await generator.Generate(new SignalGeneratorParams(SignalType.Sine, 5.0, 0.5, 0.0, 0.5, 200));
        SignalVisualizer.PlotSignal(amp1.Samples, "Amplitude = 0.5", width: 80, height: 10);
        
        var amp2 = await generator.Generate(new SignalGeneratorParams(SignalType.Sine, 5.0, 1.0, 0.0, 0.5, 200));
        SignalVisualizer.PlotSignal(amp2.Samples, "Amplitude = 1.0", width: 80, height: 10);
        
        var amp3 = await generator.Generate(new SignalGeneratorParams(SignalType.Sine, 5.0, 2.0, 0.0, 0.5, 200));
        SignalVisualizer.PlotSignal(amp3.Samples, "Amplitude = 2.0", width: 80, height: 10);

        Console.WriteLine("\n=== Visual Demo Complete ===");
    }
}
