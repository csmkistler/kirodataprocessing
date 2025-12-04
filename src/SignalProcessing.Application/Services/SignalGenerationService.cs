using SignalProcessing.Application.DTOs;
using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Application.Services;

/// <summary>
/// Application service for orchestrating signal generation and storage operations.
/// </summary>
public class SignalGenerationService
{
    private readonly ISignalGenerator _generator;
    private readonly ISignalDatabase _database;

    public SignalGenerationService(
        ISignalGenerator generator,
        ISignalDatabase database)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Generates a new signal and stores it in the database.
    /// </summary>
    /// <param name="parameters">Signal generation parameters.</param>
    /// <returns>DTO representing the generated signal.</returns>
    public async Task<SignalDto> GenerateSignalAsync(SignalGeneratorParams parameters)
    {
        // Validate parameters
        var validationResult = _generator.Validate(parameters);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid signal parameters: {string.Join(", ", validationResult.Errors)}");
        }

        // Generate signal
        var signal = await _generator.Generate(parameters);

        // Store signal in database
        await _database.SaveCompleteSignal(signal);

        // Map to DTO and return
        return MapToDto(signal);
    }

    /// <summary>
    /// Retrieves a signal by its ID.
    /// </summary>
    /// <param name="id">Signal ID.</param>
    /// <returns>DTO representing the signal, or null if not found.</returns>
    public async Task<SignalDto?> GetSignalAsync(Guid id)
    {
        try
        {
            var signal = await _database.GetCompleteSignal(id);
            return MapToDto(signal);
        }
        catch
        {
            // Signal not found
            return null;
        }
    }

    /// <summary>
    /// Retrieves the most recent signals.
    /// </summary>
    /// <param name="limit">Maximum number of signals to retrieve.</param>
    /// <returns>List of signal DTOs.</returns>
    public async Task<List<SignalDto>> GetRecentSignalsAsync(int limit = 10)
    {
        var metadataList = await _database.Metadata.GetRecentSignalMetadata(limit);
        
        var signalDtos = new List<SignalDto>();
        foreach (var metadata in metadataList)
        {
            var signalDto = new SignalDto(
                Id: metadata.Id,
                Type: metadata.Params.Type.ToString(),
                Metadata: new SignalMetadataDto(
                    Frequency: metadata.Params.Frequency,
                    Amplitude: metadata.Params.Amplitude,
                    Phase: metadata.Params.Phase,
                    Duration: metadata.Params.Duration,
                    SampleRate: metadata.Params.SampleRate
                ),
                CreatedAt: metadata.CreatedAt,
                SampleCount: 0 // Metadata doesn't include sample count
            );
            signalDtos.Add(signalDto);
        }

        return signalDtos;
    }

    private static SignalDto MapToDto(Signal signal)
    {
        return new SignalDto(
            Id: signal.Id,
            Type: signal.Type.ToString(),
            Metadata: new SignalMetadataDto(
                Frequency: signal.Metadata.Frequency,
                Amplitude: signal.Metadata.Amplitude,
                Phase: signal.Metadata.Phase,
                Duration: signal.Metadata.Duration,
                SampleRate: signal.Metadata.SampleRate
            ),
            CreatedAt: signal.CreatedAt,
            SampleCount: signal.Samples.Length
        );
    }
}
