using SignalProcessing.Core.Entities;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Interfaces;

public interface ISignalProcessor
{
    Task<ProcessedSignal> Process(Guid signalId, ProcessingParams parameters);
    ValidationResult Validate(ProcessingParams parameters);
}
