namespace SignalProcessing.Application.DTOs;

public record TriggerEventDto(
    Guid Id,
    double Value,
    double Threshold,
    DateTime Timestamp
);
