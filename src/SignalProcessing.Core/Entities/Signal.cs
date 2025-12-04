using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Entities;

public class Signal
{
    public Guid Id { get; set; }
    public SignalType Type { get; set; }
    public double[] Samples { get; set; } = Array.Empty<double>();      // Amplitude values
    public double[] Timestamps { get; set; } = Array.Empty<double>();   // Time values in seconds
    public SignalGeneratorParams Metadata { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
