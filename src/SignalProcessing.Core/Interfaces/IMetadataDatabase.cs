using SignalProcessing.Core.Entities;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Interfaces;

public interface IMetadataDatabase
{
    // Signal metadata operations
    Task SaveSignalMetadata(Guid id, SignalGeneratorParams metadata, DateTime createdAt);
    Task<SignalMetadata?> GetSignalMetadata(Guid id);
    Task<List<SignalMetadata>> GetRecentSignalMetadata(int limit);
    
    // Processed signal metadata
    Task SaveProcessedSignalMetadata(Guid id, Guid originalId, ProcessingParams parameters, DateTime createdAt);
    Task<ProcessedSignalMetadata?> GetProcessedSignalMetadata(Guid id);
    
    // Configuration operations
    Task SaveConfig(AppConfig config);
    Task<AppConfig?> LoadConfig();
    
    // Event operations
    Task SaveEvent(TriggerEvent triggerEvent);
    Task<List<TriggerEvent>> GetEvents(int limit);
    Task ClearEvents();
}

public class SignalMetadata
{
    public Guid Id { get; set; }
    public SignalGeneratorParams Params { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ProcessedSignalMetadata
{
    public Guid Id { get; set; }
    public Guid OriginalSignalId { get; set; }
    public ProcessingParams Params { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
