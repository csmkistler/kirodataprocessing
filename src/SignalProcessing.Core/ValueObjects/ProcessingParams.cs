namespace SignalProcessing.Core.ValueObjects;

public record ProcessingParams(
    OperationType Operation,
    double? CutoffFrequency = null,  // Hz (for filters)
    double? LowCutoff = null,        // Hz (for bandpass)
    double? HighCutoff = null,       // Hz (for bandpass)
    double? Gain = null,             // Multiplier (for gain)
    int? Order = null                // Filter order
);
