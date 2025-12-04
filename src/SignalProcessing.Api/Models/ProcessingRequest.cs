using System.ComponentModel.DataAnnotations;

namespace SignalProcessing.Api.Models;

/// <summary>
/// Request model for signal processing.
/// </summary>
public record ProcessingRequest
{
    /// <summary>
    /// ID of the signal to process.
    /// </summary>
    [Required]
    public Guid SignalId { get; init; }

    /// <summary>
    /// Type of processing operation (LowPass, HighPass, BandPass, Gain).
    /// </summary>
    [Required]
    public string Operation { get; init; } = string.Empty;

    /// <summary>
    /// Cutoff frequency in Hz (for LowPass and HighPass filters).
    /// </summary>
    [Range(0.1, 96000, ErrorMessage = "Cutoff frequency must be between 0.1 and 96000 Hz")]
    public double? CutoffFrequency { get; init; }

    /// <summary>
    /// Low cutoff frequency in Hz (for BandPass filter).
    /// </summary>
    [Range(0.1, 96000, ErrorMessage = "Low cutoff must be between 0.1 and 96000 Hz")]
    public double? LowCutoff { get; init; }

    /// <summary>
    /// High cutoff frequency in Hz (for BandPass filter).
    /// </summary>
    [Range(0.1, 96000, ErrorMessage = "High cutoff must be between 0.1 and 96000 Hz")]
    public double? HighCutoff { get; init; }

    /// <summary>
    /// Gain multiplier (for Gain operation).
    /// </summary>
    [Range(0.01, 100, ErrorMessage = "Gain must be between 0.01 and 100")]
    public double? Gain { get; init; }

    /// <summary>
    /// Filter order (for filter operations).
    /// </summary>
    [Range(1, 10, ErrorMessage = "Order must be between 1 and 10")]
    public int? Order { get; init; }
}
