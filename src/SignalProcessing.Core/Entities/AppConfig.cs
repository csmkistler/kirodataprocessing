using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Entities;

public class AppConfig
{
    public Guid Id { get; set; }
    public SignalGeneratorParams? LastSignalParams { get; set; }
    public ProcessingParams? LastProcessingParams { get; set; }
    public TriggerConfig? TriggerConfig { get; set; }
    public UiPreferences? UiPreferences { get; set; }
}
