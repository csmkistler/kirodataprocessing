using SignalProcessing.Application.DTOs;
using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Application.Services;

/// <summary>
/// Application service for orchestrating signal processing and storage operations.
/// </summary>
public class SignalProcessingService
{
    private readonly ISignalProcessor _processor;
    private readonly ISignalDatabase _database;

    public SignalProcessingService(
        ISignalProcessor processor,
        ISignalDatabase database)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Processes a signal with the specified parameters and stores the result.
    /// The original signal is preserved in the database.
    /// </summary>
    /// <param name="signalId">ID of the signal to process.</param>
    /// <param name="parameters">Processing parameters.</param>
    /// <returns>DTO representing the processed signal.</returns>
    public async Task<ProcessedSignalDto> ProcessSignalAsync(Guid signalId, ProcessingParams parameters)
    {
        // Validate parameters
        var validationResult = _processor.Validate(parameters);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid processing parameters: {string.Join(", ", validationResult.Errors)}");
        }

        // Verify original signal exists
        var originalSignal = await _database.GetCompleteSignal(signalId);
        if (originalSignal == null)
        {
            throw new InvalidOperationException($"Signal with ID {signalId} not found");
        }

        // Process signal (this retrieves the original from database internally)
        var processedSignal = await _processor.Process(signalId, parameters);

        // Store processed signal in database
        await _database.SaveCompleteSignal(processedSignal);

        // Verify original signal is still intact (requirement 2.4)
        var verifyOriginal = await _database.GetCompleteSignal(signalId);
        if (verifyOriginal == null || verifyOriginal.Samples.Length != originalSignal.Samples.Length)
        {
            throw new InvalidOperationException("Original signal was modified during processing");
        }

        // Map to DTO and return
        return MapToDto(processedSignal);
    }

    /// <summary>
    /// Retrieves a processed signal by its ID.
    /// </summary>
    /// <param name="id">Processed signal ID.</param>
    /// <returns>DTO representing the processed signal, or null if not found.</returns>
    public async Task<ProcessedSignalDto?> GetProcessedSignalAsync(Guid id)
    {
        try
        {
            var signal = await _database.GetCompleteSignal(id);
            if (signal is ProcessedSignal processedSignal)
            {
                return MapToDto(processedSignal);
            }
            return null;
        }
        catch
        {
            // Signal not found
            return null;
        }
    }

    private static ProcessedSignalDto MapToDto(ProcessedSignal signal)
    {
        return new ProcessedSignalDto(
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
            SampleCount: signal.Samples.Length,
            OriginalSignalId: signal.OriginalSignalId,
            ProcessingParams: new ProcessingParamsDto(
                Operation: signal.ProcessingParams.Operation.ToString(),
                CutoffFrequency: signal.ProcessingParams.CutoffFrequency,
                LowCutoff: signal.ProcessingParams.LowCutoff,
                HighCutoff: signal.ProcessingParams.HighCutoff,
                Gain: signal.ProcessingParams.Gain,
                Order: signal.ProcessingParams.Order
            )
        );
    }
}
