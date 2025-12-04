using System.ComponentModel.DataAnnotations;

namespace SignalProcessing.Api.Models;

/// <summary>
/// Request model for signal generation.
/// </summary>
public record SignalGenerationRequest
{
    /// <summary>
    /// Type of signal to generate (Sine, Square, Sawtooth, Noise).
    /// </summary>
    [Required]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Frequency in Hz.
    /// </summary>
    [Required]
    [Range(0.1, 20000, ErrorMessage = "Frequency must be between 0.1 and 20000 Hz")]
    public double Frequency { get; init; }

    /// <summary>
    /// Amplitude in arbitrary units.
    /// </summary>
    [Required]
    [Range(0.01, 100, ErrorMessage = "Amplitude must be between 0.01 and 100")]
    public double Amplitude { get; init; }

    /// <summary>
    /// Phase in radians.
    /// </summary>
    [Required]
    [Range(-6.28, 6.28, ErrorMessage = "Phase must be between -2π and 2π radians")]
    public double Phase { get; init; }

    /// <summary>
    /// Duration in seconds.
    /// </summary>
    [Required]
    [Range(0.1, 60, ErrorMessage = "Duration must be between 0.1 and 60 seconds")]
    public double Duration { get; init; }

    /// <summary>
    /// Sample rate in samples per second.
    /// </summary>
    [Required]
    [Range(100, 192000, ErrorMessage = "Sample rate must be between 100 and 192000 samples/second")]
    public int SampleRate { get; init; }
}
