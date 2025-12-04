using FsCheck;
using FsCheck.Xunit;
using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;
using Xunit;

namespace SignalProcessing.Tests;

/// <summary>
/// Property-based tests for SignalGenerator component
/// Feature: signal-processing-viz
/// </summary>
public class SignalGeneratorPropertyTests
{
    /// <summary>
    /// Property 2: Invalid parameter rejection
    /// For any invalid generation parameters (negative frequency, zero duration, out-of-range values),
    /// the Signal Generator should reject the request and return an error message without creating signal data.
    /// Validates: Requirements 1.4
    /// </summary>
    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_NegativeFrequency(int negativeFreq)
    {
        if (negativeFreq > 0) return true; // Skip valid values
        
        var generator = new SignalGenerator();
        var parameters = new SignalGeneratorParams(
            SignalType.Sine,
            negativeFreq,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 1.0,
            SampleRate: 44100
        );

        var validationResult = generator.Validate(parameters);
        
        return !validationResult.IsValid && validationResult.Errors.Count > 0;
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_NegativeAmplitude(int negativeAmp)
    {
        var amplitude = negativeAmp / 10.0;
        if (amplitude > 0) return true; // Skip valid values
        
        var generator = new SignalGenerator();
        var parameters = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            amplitude,
            Phase: 0.0,
            Duration: 1.0,
            SampleRate: 44100
        );

        var validationResult = generator.Validate(parameters);
        
        return !validationResult.IsValid && validationResult.Errors.Count > 0;
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_NegativeDuration(int negativeDur)
    {
        var duration = negativeDur / 10.0;
        if (duration > 0) return true; // Skip valid values
        
        var generator = new SignalGenerator();
        var parameters = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            Amplitude: 1.0,
            Phase: 0.0,
            duration,
            SampleRate: 44100
        );

        var validationResult = generator.Validate(parameters);
        
        return !validationResult.IsValid && validationResult.Errors.Count > 0;
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_PhaseOutOfRange(double phase)
    {
        const double twoPi = 2 * Math.PI;
        var isOutOfRange = phase < -twoPi || phase > twoPi;
        
        if (!isOutOfRange) return true; // Skip valid values
        
        var generator = new SignalGenerator();
        var parameters = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            Amplitude: 1.0,
            phase,
            Duration: 1.0,
            SampleRate: 44100
        );

        var validationResult = generator.Validate(parameters);
        
        return !validationResult.IsValid && validationResult.Errors.Count > 0;
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_ViolatesNyquist(PositiveInt sampleRate)
    {
        var sr = sampleRate.Get;
        if (sr >= 2000) return true; // Skip valid values
        
        var generator = new SignalGenerator();
        var parameters = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 1.0,
            sr
        );

        var validationResult = generator.Validate(parameters);
        
        return !validationResult.IsValid && validationResult.Errors.Count > 0;
    }

    /// <summary>
    /// Property 1: Signal generation with parameters
    /// For any valid signal type and generation parameters (frequency, amplitude, phase, duration),
    /// generating a signal should produce Signal Data with samples and timestamps that reflect
    /// the specified parameters, and the signal should be stored in the Database with all metadata intact.
    /// Validates: Requirements 1.1, 1.3, 1.5
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SignalGenerationReflectsParameters(PositiveInt freq, PositiveInt amp, PositiveInt dur, PositiveInt sr)
    {
        var generator = new SignalGenerator();
        
        // Generate valid parameters with better constraints
        var frequency = Math.Max(1.0, (freq.Get % 10000) + 1.0); // 1-10000 Hz
        var amplitude = Math.Max(0.1, (amp.Get % 100) + 0.1); // 0.1-100
        var duration = Math.Max(0.1, (dur.Get % 10) + 0.1); // 0.1-10 seconds
        var sampleRate = Math.Max((int)(frequency * 2 + 1), (sr.Get % 100000) + 100); // Satisfy Nyquist with minimum 100
        
        var parameters = new SignalGeneratorParams(
            SignalType.Sine,
            frequency,
            amplitude,
            Phase: 0.0,
            duration,
            sampleRate
        );

        // Generate signal
        var signal = generator.Generate(parameters).Result;

        // Verify signal properties
        var hasCorrectMetadata = 
            signal.Metadata.Frequency == frequency &&
            signal.Metadata.Amplitude == amplitude &&
            signal.Metadata.Duration == duration &&
            signal.Metadata.SampleRate == sampleRate;

        var expectedSampleCount = (int)(duration * sampleRate);
        var hasCorrectSampleCount = 
            signal.Samples.Length == expectedSampleCount &&
            signal.Timestamps.Length == expectedSampleCount;

        var hasValidTimestamps = signal.Timestamps.Length > 0 &&
            signal.Timestamps[0] >= 0 &&
            (signal.Timestamps.Length == 0 || signal.Timestamps[^1] <= duration + 0.001); // Allow small tolerance

        var hasValidSamples = signal.Samples.Length > 0 &&
            signal.Samples.All(s => Math.Abs(s) <= amplitude * 1.01); // Allow small floating point error

        var hasValidId = signal.Id != Guid.Empty;
        var hasCreatedAt = signal.CreatedAt != default;

        return hasCorrectMetadata && hasCorrectSampleCount && 
               hasValidTimestamps && hasValidSamples && 
               hasValidId && hasCreatedAt;
    }

    [Property(MaxTest = 100)]
    public bool SignalGenerationWorksForAllSignalTypes(PositiveInt typeIndex)
    {
        var generator = new SignalGenerator();
        var signalTypes = new[] { SignalType.Sine, SignalType.Square, SignalType.Sawtooth, SignalType.Noise };
        var signalType = signalTypes[typeIndex.Get % signalTypes.Length];

        var parameters = new SignalGeneratorParams(
            signalType,
            Frequency: 1000.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 0.1,
            SampleRate: 44100
        );

        // Generate signal
        var signal = generator.Generate(parameters).Result;

        // Verify signal was generated
        return signal != null &&
               signal.Type == signalType &&
               signal.Samples.Length > 0 &&
               signal.Timestamps.Length > 0 &&
               signal.Samples.Length == signal.Timestamps.Length;
    }

    [Property(MaxTest = 100)]
    public bool SignalAmplitudeRespected(PositiveInt amp)
    {
        var generator = new SignalGenerator();
        var amplitude = Math.Max(0.1, amp.Get % 100);

        var parameters = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            amplitude,
            Phase: 0.0,
            Duration: 0.1,
            SampleRate: 44100
        );

        var signal = generator.Generate(parameters).Result;

        // For sine wave, max amplitude should be close to specified amplitude
        var maxSample = signal.Samples.Max();
        var minSample = signal.Samples.Min();
        
        // Allow 1% tolerance for floating point errors
        return Math.Abs(maxSample) <= amplitude * 1.01 &&
               Math.Abs(minSample) <= amplitude * 1.01;
    }
}
