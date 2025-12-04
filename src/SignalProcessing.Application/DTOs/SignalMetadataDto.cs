namespace SignalProcessing.Application.DTOs;

public record SignalMetadataDto(
    double Frequency,
    double Amplitude,
    double Phase,
    double Duration,
    int SampleRate
);
