using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;

namespace SignalProcessing.Infrastructure.Data;

/// <summary>
/// Unified database implementation that coordinates both time-series and metadata databases.
/// </summary>
public class SignalDatabase : ISignalDatabase
{
    private readonly ITimeSeriesDatabase _timeSeries;
    private readonly IMetadataDatabase _metadata;

    public SignalDatabase(ITimeSeriesDatabase timeSeries, IMetadataDatabase metadata)
    {
        _timeSeries = timeSeries;
        _metadata = metadata;
    }

    public ITimeSeriesDatabase TimeSeries => _timeSeries;
    public IMetadataDatabase Metadata => _metadata;

    public async Task<Signal> GetCompleteSignal(Guid id)
    {
        // Retrieve metadata from MongoDB
        var signalMetadata = await _metadata.GetSignalMetadata(id);
        if (signalMetadata == null)
        {
            throw new InvalidOperationException($"Signal with ID {id} not found");
        }

        // Retrieve samples from InfluxDB
        var (samples, timestamps) = await _timeSeries.ReadSamples(id);

        // Check if this is a processed signal
        var processedMetadata = await _metadata.GetProcessedSignalMetadata(id);
        
        if (processedMetadata != null)
        {
            // Return as ProcessedSignal
            return new ProcessedSignal
            {
                Id = id,
                Type = signalMetadata.Params.Type,
                Samples = samples,
                Timestamps = timestamps,
                Metadata = signalMetadata.Params,
                CreatedAt = signalMetadata.CreatedAt,
                OriginalSignalId = processedMetadata.OriginalSignalId,
                ProcessingParams = processedMetadata.Params
            };
        }

        // Return as regular Signal
        return new Signal
        {
            Id = id,
            Type = signalMetadata.Params.Type,
            Samples = samples,
            Timestamps = timestamps,
            Metadata = signalMetadata.Params,
            CreatedAt = signalMetadata.CreatedAt
        };
    }

    public async Task SaveCompleteSignal(Signal signal)
    {
        // Save metadata to MongoDB
        await _metadata.SaveSignalMetadata(signal.Id, signal.Metadata, signal.CreatedAt);

        // If this is a processed signal, save processed metadata
        if (signal is ProcessedSignal processedSignal)
        {
            await _metadata.SaveProcessedSignalMetadata(
                processedSignal.Id,
                processedSignal.OriginalSignalId,
                processedSignal.ProcessingParams,
                processedSignal.CreatedAt
            );
        }

        // Save samples to InfluxDB
        bool isProcessed = signal is ProcessedSignal;
        await _timeSeries.WriteSamples(
            signal.Id,
            signal.Type,
            signal.Samples,
            signal.Timestamps,
            isProcessed
        );
    }
}
