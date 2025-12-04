namespace SignalProcessing.Core.Entities;

public class TriggerEvent
{
    public Guid Id { get; set; }
    public double Value { get; set; }                // Input value that triggered event
    public double Threshold { get; set; }            // Threshold at time of trigger
    public DateTime Timestamp { get; set; }          // When event occurred
}
