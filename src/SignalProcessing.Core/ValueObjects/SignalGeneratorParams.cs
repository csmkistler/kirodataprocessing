namespace SignalProcessing.Core.ValueObjects;

public record SignalGeneratorParams(
    SignalType Type,
    double Frequency,      // Hz
    double Amplitude,      // Arbitrary units
    double Phase,          // Radians
    double Duration,       // Seconds
    int SampleRate         // Samples per second
);
