using SignalProcessing.Core.Entities;

namespace SignalProcessing.Core.Interfaces;

public interface ISignalDatabase
{
    ITimeSeriesDatabase TimeSeries { get; }
    IMetadataDatabase Metadata { get; }
    
    // Convenience methods that coordinate both databases
    Task<Signal> GetCompleteSignal(Guid id);
    Task SaveCompleteSignal(Signal signal);
}
