using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.Entities;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Infrastructure.Data;

/// <summary>
/// MongoDB implementation of metadata database for storing signal metadata, events, and configuration.
/// </summary>
public class MongoMetadataDatabase : IMetadataDatabase
{
    private readonly IMongoCollection<SignalMetadataDocument> _signalMetadataCollection;
    private readonly IMongoCollection<ProcessedSignalMetadataDocument> _processedSignalMetadataCollection;
    private readonly IMongoCollection<TriggerEventDocument> _triggerEventsCollection;
    private readonly IMongoCollection<AppConfigDocument> _appConfigCollection;

    public MongoMetadataDatabase(MongoDbContext context)
    {
        _signalMetadataCollection = context.GetCollection<SignalMetadataDocument>("signal_metadata");
        _processedSignalMetadataCollection = context.GetCollection<ProcessedSignalMetadataDocument>("processed_signal_metadata");
        _triggerEventsCollection = context.GetCollection<TriggerEventDocument>("trigger_events");
        _appConfigCollection = context.GetCollection<AppConfigDocument>("app_config");
    }

    // Signal metadata operations
    public async Task SaveSignalMetadata(Guid id, SignalGeneratorParams metadata, DateTime createdAt)
    {
        var document = new SignalMetadataDocument
        {
            Id = id,
            Type = metadata.Type.ToString(),
            Frequency = metadata.Frequency,
            Amplitude = metadata.Amplitude,
            Phase = metadata.Phase,
            Duration = metadata.Duration,
            SampleRate = metadata.SampleRate,
            CreatedAt = createdAt
        };

        await _signalMetadataCollection.ReplaceOneAsync(
            filter: doc => doc.Id == id,
            replacement: document,
            options: new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<SignalMetadata?> GetSignalMetadata(Guid id)
    {
        var document = await _signalMetadataCollection
            .Find(doc => doc.Id == id)
            .FirstOrDefaultAsync();

        if (document == null)
            return null;

        return new SignalMetadata
        {
            Id = document.Id,
            Params = new SignalGeneratorParams(
                Type: Enum.Parse<SignalType>(document.Type),
                Frequency: document.Frequency,
                Amplitude: document.Amplitude,
                Phase: document.Phase,
                Duration: document.Duration,
                SampleRate: document.SampleRate
            ),
            CreatedAt = document.CreatedAt
        };
    }

    public async Task<List<SignalMetadata>> GetRecentSignalMetadata(int limit)
    {
        var documents = await _signalMetadataCollection
            .Find(Builders<SignalMetadataDocument>.Filter.Empty)
            .SortByDescending(doc => doc.CreatedAt)
            .Limit(limit)
            .ToListAsync();

        return documents.Select(doc => new SignalMetadata
        {
            Id = doc.Id,
            Params = new SignalGeneratorParams(
                Type: Enum.Parse<SignalType>(doc.Type),
                Frequency: doc.Frequency,
                Amplitude: doc.Amplitude,
                Phase: doc.Phase,
                Duration: doc.Duration,
                SampleRate: doc.SampleRate
            ),
            CreatedAt = doc.CreatedAt
        }).ToList();
    }

    // Processed signal metadata
    public async Task SaveProcessedSignalMetadata(Guid id, Guid originalId, ProcessingParams parameters, DateTime createdAt)
    {
        var document = new ProcessedSignalMetadataDocument
        {
            Id = id,
            OriginalSignalId = originalId,
            Operation = parameters.Operation.ToString(),
            CutoffFrequency = parameters.CutoffFrequency,
            LowCutoff = parameters.LowCutoff,
            HighCutoff = parameters.HighCutoff,
            Gain = parameters.Gain,
            Order = parameters.Order,
            CreatedAt = createdAt
        };

        await _processedSignalMetadataCollection.ReplaceOneAsync(
            filter: doc => doc.Id == id,
            replacement: document,
            options: new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<ProcessedSignalMetadata?> GetProcessedSignalMetadata(Guid id)
    {
        var document = await _processedSignalMetadataCollection
            .Find(doc => doc.Id == id)
            .FirstOrDefaultAsync();

        if (document == null)
            return null;

        return new ProcessedSignalMetadata
        {
            Id = document.Id,
            OriginalSignalId = document.OriginalSignalId,
            Params = new ProcessingParams(
                Operation: Enum.Parse<OperationType>(document.Operation),
                CutoffFrequency: document.CutoffFrequency,
                LowCutoff: document.LowCutoff,
                HighCutoff: document.HighCutoff,
                Gain: document.Gain,
                Order: document.Order
            ),
            CreatedAt = document.CreatedAt
        };
    }

    // Configuration operations
    public async Task SaveConfig(AppConfig config)
    {
        var document = new AppConfigDocument
        {
            Id = config.Id,
            LastSignalParams = config.LastSignalParams,
            LastProcessingParams = config.LastProcessingParams,
            TriggerConfig = config.TriggerConfig,
            UiPreferences = config.UiPreferences
        };

        await _appConfigCollection.ReplaceOneAsync(
            filter: doc => doc.Id == config.Id,
            replacement: document,
            options: new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<AppConfig?> LoadConfig()
    {
        var document = await _appConfigCollection
            .Find(Builders<AppConfigDocument>.Filter.Empty)
            .FirstOrDefaultAsync();

        if (document == null)
            return null;

        return new AppConfig
        {
            Id = document.Id,
            LastSignalParams = document.LastSignalParams,
            LastProcessingParams = document.LastProcessingParams,
            TriggerConfig = document.TriggerConfig,
            UiPreferences = document.UiPreferences
        };
    }

    // Event operations
    public async Task SaveEvent(TriggerEvent triggerEvent)
    {
        var document = new TriggerEventDocument
        {
            Id = triggerEvent.Id,
            Value = triggerEvent.Value,
            Threshold = triggerEvent.Threshold,
            Timestamp = triggerEvent.Timestamp
        };

        await _triggerEventsCollection.InsertOneAsync(document);
    }

    public async Task<List<TriggerEvent>> GetEvents(int limit)
    {
        var documents = await _triggerEventsCollection
            .Find(Builders<TriggerEventDocument>.Filter.Empty)
            .SortByDescending(doc => doc.Timestamp)
            .Limit(limit)
            .ToListAsync();

        return documents.Select(doc => new TriggerEvent
        {
            Id = doc.Id,
            Value = doc.Value,
            Threshold = doc.Threshold,
            Timestamp = doc.Timestamp
        }).ToList();
    }

    public async Task ClearEvents()
    {
        await _triggerEventsCollection.DeleteManyAsync(Builders<TriggerEventDocument>.Filter.Empty);
    }
}

// MongoDB document models
internal class SignalMetadataDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("frequency")]
    public double Frequency { get; set; }

    [BsonElement("amplitude")]
    public double Amplitude { get; set; }

    [BsonElement("phase")]
    public double Phase { get; set; }

    [BsonElement("duration")]
    public double Duration { get; set; }

    [BsonElement("sampleRate")]
    public int SampleRate { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
}

internal class ProcessedSignalMetadataDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("originalSignalId")]
    [BsonRepresentation(BsonType.String)]
    public Guid OriginalSignalId { get; set; }

    [BsonElement("operation")]
    public string Operation { get; set; } = string.Empty;

    [BsonElement("cutoffFrequency")]
    [BsonIgnoreIfNull]
    public double? CutoffFrequency { get; set; }

    [BsonElement("lowCutoff")]
    [BsonIgnoreIfNull]
    public double? LowCutoff { get; set; }

    [BsonElement("highCutoff")]
    [BsonIgnoreIfNull]
    public double? HighCutoff { get; set; }

    [BsonElement("gain")]
    [BsonIgnoreIfNull]
    public double? Gain { get; set; }

    [BsonElement("order")]
    [BsonIgnoreIfNull]
    public int? Order { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
}

internal class TriggerEventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("value")]
    public double Value { get; set; }

    [BsonElement("threshold")]
    public double Threshold { get; set; }

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; }
}

internal class AppConfigDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("lastSignalParams")]
    [BsonIgnoreIfNull]
    public SignalGeneratorParams? LastSignalParams { get; set; }

    [BsonElement("lastProcessingParams")]
    [BsonIgnoreIfNull]
    public ProcessingParams? LastProcessingParams { get; set; }

    [BsonElement("triggerConfig")]
    [BsonIgnoreIfNull]
    public TriggerConfig? TriggerConfig { get; set; }

    [BsonElement("uiPreferences")]
    [BsonIgnoreIfNull]
    public UiPreferences? UiPreferences { get; set; }
}
