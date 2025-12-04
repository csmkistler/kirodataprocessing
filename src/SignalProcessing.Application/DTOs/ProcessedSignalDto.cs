namespace SignalProcessing.Application.DTOs;

public record ProcessedSignalDto(
    Guid Id,
    string Type,
    SignalMetadataDto Metadata,
    DateTime CreatedAt,
    int SampleCount,
    Guid OriginalSignalId,
    ProcessingParamsDto ProcessingParams
);
