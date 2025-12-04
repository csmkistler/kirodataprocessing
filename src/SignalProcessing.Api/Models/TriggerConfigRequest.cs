using System.ComponentModel.DataAnnotations;

namespace SignalProcessing.Api.Models;

/// <summary>
/// Request model for configuring trigger threshold.
/// </summary>
public record TriggerConfigRequest
{
    /// <summary>
    /// Threshold value for trigger detection.
    /// </summary>
    [Required]
    public double Threshold { get; init; }

    /// <summary>
    /// Whether the trigger is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
