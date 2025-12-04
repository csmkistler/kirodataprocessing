using SignalProcessing.Core.Entities;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Interfaces;

public interface ISignalGenerator
{
    Task<Signal> Generate(SignalGeneratorParams parameters);
    ValidationResult Validate(SignalGeneratorParams parameters);
}
