using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Infrastructure;

public class SignalProcessor : ISignalProcessor
{
    private readonly ISignalDatabase _database;

    public SignalProcessor(ISignalDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public ValidationResult Validate(ProcessingParams parameters)
    {
        var result = new ValidationResult { IsValid = true };

        switch (parameters.Operation)
        {
            case OperationType.LowPass:
                ValidateLowPassFilter(parameters, result);
                break;
            case OperationType.HighPass:
                ValidateHighPassFilter(parameters, result);
                break;
            case OperationType.BandPass:
                ValidateBandPassFilter(parameters, result);
                break;
            case OperationType.Gain:
                ValidateGain(parameters, result);
                break;
            default:
                result.AddError($"Unsupported operation type: {parameters.Operation}");
                break;
        }

        return result;
    }

    public ValidationResult ValidateWithSignal(ProcessingParams parameters, Signal signal)
    {
        var result = Validate(parameters);
        
        if (!result.IsValid)
            return result;

        // Calculate Nyquist frequency from the signal's sample rate
        double nyquistFrequency = signal.Metadata.SampleRate / 2.0;

        // Validate cutoff frequencies against Nyquist frequency
        switch (parameters.Operation)
        {
            case OperationType.LowPass:
            case OperationType.HighPass:
                if (parameters.CutoffFrequency.HasValue && parameters.CutoffFrequency.Value >= nyquistFrequency)
                {
                    result.AddError($"Cutoff frequency must be below Nyquist frequency ({nyquistFrequency} Hz)");
                }
                break;
            case OperationType.BandPass:
                if (parameters.HighCutoff.HasValue && parameters.HighCutoff.Value >= nyquistFrequency)
                {
                    result.AddError($"High cutoff frequency must be below Nyquist frequency ({nyquistFrequency} Hz)");
                }
                break;
        }

        return result;
    }

    private void ValidateLowPassFilter(ProcessingParams parameters, ValidationResult result)
    {
        if (!parameters.CutoffFrequency.HasValue)
        {
            result.AddError("Cutoff frequency is required for low-pass filter");
        }
        else if (parameters.CutoffFrequency.Value <= 0)
        {
            result.AddError("Cutoff frequency must be positive");
        }

        if (parameters.Order.HasValue && parameters.Order.Value <= 0)
        {
            result.AddError("Filter order must be positive");
        }
    }

    private void ValidateHighPassFilter(ProcessingParams parameters, ValidationResult result)
    {
        if (!parameters.CutoffFrequency.HasValue)
        {
            result.AddError("Cutoff frequency is required for high-pass filter");
        }
        else if (parameters.CutoffFrequency.Value <= 0)
        {
            result.AddError("Cutoff frequency must be positive");
        }

        if (parameters.Order.HasValue && parameters.Order.Value <= 0)
        {
            result.AddError("Filter order must be positive");
        }
    }

    private void ValidateBandPassFilter(ProcessingParams parameters, ValidationResult result)
    {
        if (!parameters.LowCutoff.HasValue)
        {
            result.AddError("Low cutoff frequency is required for band-pass filter");
        }
        else if (parameters.LowCutoff.Value <= 0)
        {
            result.AddError("Low cutoff frequency must be positive");
        }

        if (!parameters.HighCutoff.HasValue)
        {
            result.AddError("High cutoff frequency is required for band-pass filter");
        }
        else if (parameters.HighCutoff.Value <= 0)
        {
            result.AddError("High cutoff frequency must be positive");
        }

        if (parameters.LowCutoff.HasValue && parameters.HighCutoff.HasValue &&
            parameters.LowCutoff.Value >= parameters.HighCutoff.Value)
        {
            result.AddError("Low cutoff frequency must be less than high cutoff frequency");
        }

        if (parameters.Order.HasValue && parameters.Order.Value <= 0)
        {
            result.AddError("Filter order must be positive");
        }
    }

    private void ValidateGain(ProcessingParams parameters, ValidationResult result)
    {
        if (!parameters.Gain.HasValue)
        {
            result.AddError("Gain is required for gain adjustment operation");
        }
        else if (parameters.Gain.Value <= 0)
        {
            result.AddError("Gain must be positive");
        }
    }

    public async Task<ProcessedSignal> Process(Guid signalId, ProcessingParams parameters)
    {
        // Retrieve original signal from database
        var originalSignal = await _database.GetCompleteSignal(signalId);
        if (originalSignal == null)
        {
            throw new ArgumentException($"Signal with ID {signalId} not found");
        }

        // Validate parameters with signal context
        var validationResult = ValidateWithSignal(parameters, originalSignal);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid processing parameters: {string.Join(", ", validationResult.Errors)}");
        }

        // Create processed signal
        var processedSignal = new ProcessedSignal
        {
            Id = Guid.NewGuid(),
            Type = originalSignal.Type,
            OriginalSignalId = signalId,
            ProcessingParams = parameters,
            Metadata = originalSignal.Metadata,
            CreatedAt = DateTime.UtcNow
        };

        // Apply processing based on operation type
        switch (parameters.Operation)
        {
            case OperationType.LowPass:
                processedSignal.Samples = ApplyLowPassFilter(originalSignal.Samples, originalSignal.Metadata.SampleRate, 
                    parameters.CutoffFrequency!.Value, parameters.Order ?? 4);
                break;
            case OperationType.HighPass:
                processedSignal.Samples = ApplyHighPassFilter(originalSignal.Samples, originalSignal.Metadata.SampleRate,
                    parameters.CutoffFrequency!.Value, parameters.Order ?? 4);
                break;
            case OperationType.BandPass:
                processedSignal.Samples = ApplyBandPassFilter(originalSignal.Samples, originalSignal.Metadata.SampleRate,
                    parameters.LowCutoff!.Value, parameters.HighCutoff!.Value, parameters.Order ?? 4);
                break;
            case OperationType.Gain:
                processedSignal.Samples = ApplyGain(originalSignal.Samples, parameters.Gain!.Value);
                break;
            default:
                throw new NotSupportedException($"Operation type {parameters.Operation} is not supported");
        }

        // Copy timestamps from original signal
        processedSignal.Timestamps = (double[])originalSignal.Timestamps.Clone();

        return processedSignal;
    }

    private double[] ApplyGain(double[] samples, double gain)
    {
        var result = new double[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            result[i] = samples[i] * gain;
        }
        return result;
    }

    private double[] ApplyLowPassFilter(double[] samples, int sampleRate, double cutoffFrequency, int order)
    {
        // Implement simple Butterworth low-pass filter
        // Using a simple first-order RC filter approximation for simplicity
        // For production, consider using a proper DSP library
        
        double rc = 1.0 / (2.0 * Math.PI * cutoffFrequency);
        double dt = 1.0 / sampleRate;
        double alpha = dt / (rc + dt);

        var result = new double[samples.Length];
        result[0] = samples[0];

        // Apply filter multiple times for higher order
        var temp = (double[])samples.Clone();
        for (int pass = 0; pass < order; pass++)
        {
            result[0] = temp[0];
            for (int i = 1; i < samples.Length; i++)
            {
                result[i] = result[i - 1] + alpha * (temp[i] - result[i - 1]);
            }
            temp = (double[])result.Clone();
        }

        return result;
    }

    private double[] ApplyHighPassFilter(double[] samples, int sampleRate, double cutoffFrequency, int order)
    {
        // High-pass filter = original signal - low-pass filtered signal
        var lowPassFiltered = ApplyLowPassFilter(samples, sampleRate, cutoffFrequency, order);
        
        var result = new double[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            result[i] = samples[i] - lowPassFiltered[i];
        }
        
        return result;
    }

    private double[] ApplyBandPassFilter(double[] samples, int sampleRate, double lowCutoff, double highCutoff, int order)
    {
        // Band-pass filter = low-pass(high-pass(signal))
        // First apply high-pass to remove low frequencies
        var highPassFiltered = ApplyHighPassFilter(samples, sampleRate, lowCutoff, order);
        
        // Then apply low-pass to remove high frequencies
        var bandPassFiltered = ApplyLowPassFilter(highPassFiltered, sampleRate, highCutoff, order);
        
        return bandPassFiltered;
    }
}
