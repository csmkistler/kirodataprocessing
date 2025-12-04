using System.ComponentModel.DataAnnotations;

namespace SignalProcessing.Api.Models;

/// <summary>
/// Request model for checking a value against the trigger threshold.
/// </summary>
public record TriggerCheckRequest
{
    /// <summary>
    /// Value to check against the configured threshold.
    /// </summary>
    [Required]
    public double Value { get; init; }
}
