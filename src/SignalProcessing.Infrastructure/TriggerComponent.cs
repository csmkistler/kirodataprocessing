using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Infrastructure;

/// <summary>
/// Monitors input values and emits events when configured thresholds are exceeded.
/// </summary>
public class TriggerComponent : ITriggerComponent
{
    private readonly IMetadataDatabase _metadataDatabase;
    private TriggerConfig? _config;

    public TriggerComponent(IMetadataDatabase metadataDatabase)
    {
        _metadataDatabase = metadataDatabase ?? throw new ArgumentNullException(nameof(metadataDatabase));
    }

    /// <summary>
    /// Configures the trigger component with a threshold value.
    /// </summary>
    /// <param name="config">The trigger configuration containing threshold and enabled state.</param>
    public void Configure(TriggerConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Checks if the provided value exceeds the configured threshold.
    /// If threshold is exceeded, creates and stores a trigger event.
    /// </summary>
    /// <param name="value">The value to check against the threshold.</param>
    /// <returns>A TriggerEvent if threshold is exceeded, null otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when threshold is not configured.</exception>
    public async Task<TriggerEvent?> CheckValue(double value)
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Threshold must be configured before checking values");
        }

        if (!_config.Enabled)
        {
            return null;
        }

        // Check if value exceeds threshold
        if (value > _config.Threshold)
        {
            var triggerEvent = new TriggerEvent
            {
                Id = Guid.NewGuid(),
                Value = value,
                Threshold = _config.Threshold,
                Timestamp = DateTime.UtcNow
            };

            // Store event in database
            await _metadataDatabase.SaveEvent(triggerEvent);

            return triggerEvent;
        }

        return null;
    }

    /// <summary>
    /// Retrieves all trigger events from the database.
    /// </summary>
    /// <returns>A list of trigger events in reverse chronological order.</returns>
    public async Task<List<TriggerEvent>> GetEvents()
    {
        // Get all events (limit can be adjusted as needed)
        return await _metadataDatabase.GetEvents(limit: 100);
    }
}
