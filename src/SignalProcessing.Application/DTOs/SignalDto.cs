namespace SignalProcessing.Application.DTOs;

public record SignalDto(
    Guid Id,
    string Type,
    SignalMetadataDto Metadata,
    DateTime CreatedAt,
    int SampleCount
);
