using SignalProcessing.Application.DTOs;
using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Application.Services;

/// <summary>
/// Application service for managing trigger operations and events.
/// </summary>
public class TriggerService
{
    private readonly ITriggerComponent _triggerComponent;
    private readonly IMetadataDatabase _database;

    public TriggerService(
        ITriggerComponent triggerComponent,
        IMetadataDatabase database)
    {
        _triggerComponent = triggerComponent ?? throw new ArgumentNullException(nameof(triggerComponent));
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Configures the trigger threshold.
    /// </summary>
    /// <param name="threshold">Threshold value.</param>
    /// <param name="enabled">Whether the trigger is enabled.</param>
    public void ConfigureThreshold(double threshold, bool enabled = true)
    {
        var config = new TriggerConfig(threshold, enabled);
        _triggerComponent.Configure(config);
    }

    /// <summary>
    /// Checks a value against the configured threshold and emits an event if exceeded.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>DTO representing the trigger event if threshold was exceeded, null otherwise.</returns>
    public async Task<TriggerEventDto?> CheckValueAsync(double value)
    {
        var triggerEvent = await _triggerComponent.CheckValue(value);
        
        if (triggerEvent != null)
        {
            return MapToDto(triggerEvent);
        }

        return null;
    }

    /// <summary>
    /// Retrieves recent trigger events.
    /// </summary>
    /// <param name="limit">Maximum number of events to retrieve.</param>
    /// <returns>List of trigger event DTOs in reverse chronological order.</returns>
    public async Task<List<TriggerEventDto>> GetEventsAsync(int limit = 100)
    {
        var events = await _database.GetEvents(limit);
        
        // Events should already be in reverse chronological order from database
        return events.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Clears all trigger events from the database.
    /// </summary>
    public async Task ClearEventsAsync()
    {
        await _database.ClearEvents();
    }

    private static TriggerEventDto MapToDto(TriggerEvent triggerEvent)
    {
        return new TriggerEventDto(
            Id: triggerEvent.Id,
            Value: triggerEvent.Value,
            Threshold: triggerEvent.Threshold,
            Timestamp: triggerEvent.Timestamp
        );
    }
}
