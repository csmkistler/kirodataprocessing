namespace SignalProcessing.Application.DTOs;

public record ProcessingParamsDto(
    string Operation,
    double? CutoffFrequency,
    double? LowCutoff,
    double? HighCutoff,
    double? Gain,
    int? Order
);
