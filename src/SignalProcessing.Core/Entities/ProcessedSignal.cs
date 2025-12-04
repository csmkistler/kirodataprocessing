using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Entities;

public class ProcessedSignal : Signal
{
    public Guid OriginalSignalId { get; set; }       // Reference to original
    public ProcessingParams ProcessingParams { get; set; } = null!;
}
