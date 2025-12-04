namespace SignalProcessing.Core.ValueObjects;

public record TriggerConfig(
    double Threshold,
    bool Enabled
);
