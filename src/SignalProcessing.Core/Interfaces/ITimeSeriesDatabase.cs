using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Interfaces;

public interface ITimeSeriesDatabase
{
    Task WriteSamples(Guid signalId, SignalType type, double[] samples, double[] timestamps, bool isProcessed = false);
    Task<(double[] samples, double[] timestamps)> ReadSamples(Guid signalId, DateTime? start = null, DateTime? end = null);
    Task DeleteSamples(Guid signalId);
}
