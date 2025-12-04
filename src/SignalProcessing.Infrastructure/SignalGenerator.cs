using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Infrastructure;

public class SignalGenerator : ISignalGenerator
{
    public ValidationResult Validate(SignalGeneratorParams parameters)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate frequency
        if (parameters.Frequency <= 0)
        {
            result.AddError("Frequency must be positive");
        }

        // Validate amplitude
        if (parameters.Amplitude <= 0)
        {
            result.AddError("Amplitude must be positive");
        }

        // Validate duration
        if (parameters.Duration <= 0)
        {
            result.AddError("Duration must be positive");
        }

        // Validate phase (between -2π and 2π)
        const double twoPi = 2 * Math.PI;
        if (parameters.Phase < -twoPi || parameters.Phase > twoPi)
        {
            result.AddError("Phase must be between -2π and 2π");
        }

        // Validate sample rate (Nyquist criterion: sample rate must be at least 2x frequency)
        if (parameters.SampleRate < 2 * parameters.Frequency)
        {
            result.AddError("Sample rate must satisfy Nyquist criterion (at least 2x frequency)");
        }

        // Validate sample rate is positive
        if (parameters.SampleRate <= 0)
        {
            result.AddError("Sample rate must be positive");
        }

        return result;
    }

    public Task<Signal> Generate(SignalGeneratorParams parameters)
    {
        // Validate parameters first
        var validationResult = Validate(parameters);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid parameters: {string.Join(", ", validationResult.Errors)}");
        }

        // Calculate number of samples
        int sampleCount = (int)(parameters.Duration * parameters.SampleRate);
        
        // Initialize arrays
        var samples = new double[sampleCount];
        var timestamps = new double[sampleCount];

        // Generate timestamps
        for (int i = 0; i < sampleCount; i++)
        {
            timestamps[i] = i / (double)parameters.SampleRate;
        }

        // Generate samples based on signal type
        switch (parameters.Type)
        {
            case SignalType.Sine:
                GenerateSineWave(samples, timestamps, parameters);
                break;
            case SignalType.Square:
                GenerateSquareWave(samples, timestamps, parameters);
                break;
            case SignalType.Sawtooth:
                GenerateSawtoothWave(samples, timestamps, parameters);
                break;
            case SignalType.Noise:
                GenerateWhiteNoise(samples, parameters);
                break;
            default:
                throw new NotSupportedException($"Signal type {parameters.Type} is not supported");
        }

        // Create signal entity
        var signal = new Signal
        {
            Id = Guid.NewGuid(),
            Type = parameters.Type,
            Samples = samples,
            Timestamps = timestamps,
            Metadata = parameters,
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(signal);
    }

    private void GenerateSineWave(double[] samples, double[] timestamps, SignalGeneratorParams parameters)
    {
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = parameters.Amplitude * Math.Sin(2 * Math.PI * parameters.Frequency * timestamps[i] + parameters.Phase);
        }
    }

    private void GenerateSquareWave(double[] samples, double[] timestamps, SignalGeneratorParams parameters)
    {
        for (int i = 0; i < samples.Length; i++)
        {
            double sineValue = Math.Sin(2 * Math.PI * parameters.Frequency * timestamps[i] + parameters.Phase);
            samples[i] = parameters.Amplitude * (sineValue >= 0 ? 1 : -1);
        }
    }

    private void GenerateSawtoothWave(double[] samples, double[] timestamps, SignalGeneratorParams parameters)
    {
        for (int i = 0; i < samples.Length; i++)
        {
            // Calculate phase at current time
            double phase = (2 * Math.PI * parameters.Frequency * timestamps[i] + parameters.Phase) % (2 * Math.PI);
            
            // Normalize to [-π, π]
            if (phase > Math.PI)
                phase -= 2 * Math.PI;
            
            // Sawtooth: linear ramp from -1 to 1
            samples[i] = parameters.Amplitude * (phase / Math.PI);
        }
    }

    private void GenerateWhiteNoise(double[] samples, SignalGeneratorParams parameters)
    {
        var random = new Random();
        for (int i = 0; i < samples.Length; i++)
        {
            // Generate random value between -1 and 1, then scale by amplitude
            samples[i] = parameters.Amplitude * (2 * random.NextDouble() - 1);
        }
    }
}
