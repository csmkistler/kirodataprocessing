using FsCheck;
using FsCheck.Xunit;
using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;
using SignalProcessing.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace SignalProcessing.Tests;

/// <summary>
/// Property-based tests for SignalProcessor component
/// Feature: signal-processing-viz
/// </summary>
public class SignalProcessorPropertyTests
{
    private SignalProcessor CreateProcessorForValidation()
    {
        // For validation tests, we create a processor without database dependency
        // by using a mock that won't be called
        var mockDatabase = new MockSignalDatabase();
        return new SignalProcessor(mockDatabase);
    }

    // Simple mock database for validation tests (won't be used)
    private class MockSignalDatabase : ISignalDatabase
    {
        public ITimeSeriesDatabase TimeSeries => throw new NotImplementedException();
        public IMetadataDatabase Metadata => throw new NotImplementedException();
        public Task<Signal> GetCompleteSignal(Guid id) => throw new NotImplementedException();
        public Task SaveCompleteSignal(Signal signal) => throw new NotImplementedException();
    }

    /// <summary>
    /// Property 4: Invalid processing parameter rejection
    /// For any invalid processing parameters (negative cutoff frequency, invalid operation type),
    /// the Signal Processor should reject the operation, maintain the current state,
    /// and not create any processed signal data.
    /// Validates: Requirements 2.5
    /// </summary>
    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_NegativeCutoffFrequency(int negativeCutoff)
    {
        var cutoff = negativeCutoff / 10.0;
        if (cutoff > 0) return true; // Skip valid values
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.LowPass,
            CutoffFrequency: cutoff
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && validationResult.Errors.Count > 0;
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_MissingCutoffForLowPass()
    {
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.LowPass,
            CutoffFrequency: null
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("required"));
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_MissingCutoffForHighPass()
    {
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.HighPass,
            CutoffFrequency: null
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("required"));
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_MissingGain()
    {
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.Gain,
            Gain: null
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("required"));
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_NegativeGain(int negativeGain)
    {
        var gain = negativeGain / 10.0;
        if (gain > 0) return true; // Skip valid values
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.Gain,
            Gain: gain
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && validationResult.Errors.Count > 0;
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_BandPassMissingLowCutoff()
    {
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.BandPass,
            LowCutoff: null,
            HighCutoff: 1000.0
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("Low cutoff"));
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_BandPassMissingHighCutoff()
    {
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.BandPass,
            LowCutoff: 100.0,
            HighCutoff: null
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("High cutoff"));
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_BandPassInvalidRange(PositiveInt low, PositiveInt high)
    {
        var lowCutoff = (double)low.Get;
        var highCutoff = (double)high.Get;
        
        // Only test when low >= high (invalid case)
        if (lowCutoff < highCutoff) return true;
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.BandPass,
            LowCutoff: lowCutoff,
            HighCutoff: highCutoff
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("less than"));
    }

    [Property(MaxTest = 100)]
    public bool InvalidParametersAreRejected_NegativeFilterOrder(int negativeOrder)
    {
        if (negativeOrder > 0) return true; // Skip valid values
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.LowPass,
            CutoffFrequency: 1000.0,
            Order: negativeOrder
        );

        var validationResult = processor.Validate(parameters);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("order"));
    }

    [Property(MaxTest = 100)]
    public bool ValidParametersAreAccepted_LowPass(PositiveInt cutoff)
    {
        var cutoffFreq = Math.Max(1.0, cutoff.Get % 10000);
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.LowPass,
            CutoffFrequency: cutoffFreq,
            Order: 4
        );

        var validationResult = processor.Validate(parameters);
        
        return validationResult.IsValid;
    }

    [Property(MaxTest = 100)]
    public bool ValidParametersAreAccepted_HighPass(PositiveInt cutoff)
    {
        var cutoffFreq = Math.Max(1.0, cutoff.Get % 10000);
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.HighPass,
            CutoffFrequency: cutoffFreq,
            Order: 4
        );

        var validationResult = processor.Validate(parameters);
        
        return validationResult.IsValid;
    }

    [Property(MaxTest = 100)]
    public bool ValidParametersAreAccepted_BandPass(PositiveInt low, PositiveInt high)
    {
        var lowCutoff = Math.Max(1.0, low.Get % 5000);
        var highCutoff = lowCutoff + Math.Max(1.0, high.Get % 5000);
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.BandPass,
            LowCutoff: lowCutoff,
            HighCutoff: highCutoff,
            Order: 4
        );

        var validationResult = processor.Validate(parameters);
        
        return validationResult.IsValid;
    }

    [Property(MaxTest = 100)]
    public bool ValidParametersAreAccepted_Gain(PositiveInt gain)
    {
        var gainValue = Math.Max(0.1, gain.Get % 100);
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.Gain,
            Gain: gainValue
        );

        var validationResult = processor.Validate(parameters);
        
        return validationResult.IsValid;
    }

    [Property(MaxTest = 100)]
    public bool CutoffAboveNyquistIsRejected(PositiveInt sampleRate, PositiveInt cutoff)
    {
        var sr = Math.Max(100, sampleRate.Get % 100000);
        var nyquist = sr / 2.0;
        var cutoffFreq = nyquist + Math.Max(1.0, cutoff.Get % 10000);
        
        // Create a mock signal with the sample rate
        var signal = new Signal
        {
            Id = Guid.NewGuid(),
            Type = SignalType.Sine,
            Samples = new double[100],
            Timestamps = new double[100],
            Metadata = new SignalGeneratorParams(
                SignalType.Sine,
                Frequency: 1000.0,
                Amplitude: 1.0,
                Phase: 0.0,
                Duration: 1.0,
                SampleRate: sr
            ),
            CreatedAt = DateTime.UtcNow
        };
        
        var processor = CreateProcessorForValidation();
        var parameters = new ProcessingParams(
            OperationType.LowPass,
            CutoffFrequency: cutoffFreq
        );

        var validationResult = processor.ValidateWithSignal(parameters, signal);
        
        return !validationResult.IsValid && 
               validationResult.Errors.Any(e => e.Contains("Nyquist"));
    }

    /// <summary>
    /// Property 3: Signal processing preserves original
    /// For any signal and valid processing parameters, applying processing should create
    /// a new processed signal while leaving the original signal data completely unchanged in the Database.
    /// Validates: Requirements 2.1, 2.3, 2.4
    /// </summary>
    [Fact(Skip = "Requires MongoDB and InfluxDB to be running - manual test only")]
    public async Task SignalProcessingPreservesOriginal_Gain_ManualTest()
    {
        var gainValue = 2.0;
        
        // Create test configuration
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDB"] = "mongodb://localhost:27017",
                ["MongoDB:DatabaseName"] = "signal_processing_test",
                ["ConnectionStrings:InfluxDB"] = "http://localhost:8086",
                ["InfluxDB:Token"] = "",
                ["InfluxDB:Organization"] = "signal-processing-test",
                ["InfluxDB:Bucket"] = "signals_test"
            })
            .Build();

        // Initialize database contexts
        var mongoContext = new MongoDbContext(configuration);
        var influxContext = new InfluxDbContext(configuration);

        // Initialize database
        var metadataDb = new MongoMetadataDatabase(mongoContext);
        var timeSeriesDb = new InfluxTimeSeriesDatabase(influxContext);
        var database = new SignalDatabase(timeSeriesDb, metadataDb);
        
        // Create a test signal
        var generator = new SignalGenerator();
        var signalParams = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 0.1,
            SampleRate: 44100
        );
        var originalSignal = await generator.Generate(signalParams);
        
        // Store original samples for comparison
        var originalSamplesCopy = (double[])originalSignal.Samples.Clone();
        var originalTimestampsCopy = (double[])originalSignal.Timestamps.Clone();
        
        await database.SaveCompleteSignal(originalSignal);
        
        // Create processor and process signal
        var processor = new SignalProcessor(database);
        var processingParams = new ProcessingParams(
            OperationType.Gain,
            Gain: gainValue
        );
        
        var processedSignal = await processor.Process(originalSignal.Id, processingParams);
        
        // Retrieve original signal from database
        var retrievedOriginal = await database.GetCompleteSignal(originalSignal.Id);
        
        // Verify original signal is unchanged
        var originalUnchanged = 
            retrievedOriginal != null &&
            retrievedOriginal.Samples.Length == originalSamplesCopy.Length &&
            retrievedOriginal.Timestamps.Length == originalTimestampsCopy.Length &&
            retrievedOriginal.Samples.SequenceEqual(originalSamplesCopy) &&
            retrievedOriginal.Timestamps.SequenceEqual(originalTimestampsCopy);
        
        // Verify processed signal is different
        var processedIsDifferent = 
            processedSignal.Id != originalSignal.Id &&
            processedSignal.OriginalSignalId == originalSignal.Id &&
            processedSignal.Samples.Length == originalSignal.Samples.Length;
        
        // Verify gain was applied correctly
        var gainAppliedCorrectly = true;
        for (int i = 0; i < Math.Min(10, originalSignal.Samples.Length); i++)
        {
            var expected = originalSamplesCopy[i] * gainValue;
            var actual = processedSignal.Samples[i];
            if (Math.Abs(expected - actual) > 0.0001)
            {
                gainAppliedCorrectly = false;
                break;
            }
        }
        
        Assert.True(originalUnchanged && processedIsDifferent && gainAppliedCorrectly);
    }

    [Fact(Skip = "Requires MongoDB and InfluxDB to be running - manual test only")]
    public async Task SignalProcessingPreservesOriginal_LowPass_ManualTest()
    {
        var cutoffFreq = 500.0;
        
        // Create test configuration
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDB"] = "mongodb://localhost:27017",
                ["MongoDB:DatabaseName"] = "signal_processing_test",
                ["ConnectionStrings:InfluxDB"] = "http://localhost:8086",
                ["InfluxDB:Token"] = "",
                ["InfluxDB:Organization"] = "signal-processing-test",
                ["InfluxDB:Bucket"] = "signals_test"
            })
            .Build();

        // Initialize database contexts
        var mongoContext = new MongoDbContext(configuration);
        var influxContext = new InfluxDbContext(configuration);

        // Initialize database
        var metadataDb = new MongoMetadataDatabase(mongoContext);
        var timeSeriesDb = new InfluxTimeSeriesDatabase(influxContext);
        var database = new SignalDatabase(timeSeriesDb, metadataDb);
        
        // Create a test signal
        var generator = new SignalGenerator();
        var signalParams = new SignalGeneratorParams(
            SignalType.Sine,
            Frequency: 1000.0,
            Amplitude: 1.0,
            Phase: 0.0,
            Duration: 0.1,
            SampleRate: 44100
        );
        var originalSignal = await generator.Generate(signalParams);
        
        // Store original samples for comparison
        var originalSamplesCopy = (double[])originalSignal.Samples.Clone();
        
        await database.SaveCompleteSignal(originalSignal);
        
        // Create processor and process signal
        var processor = new SignalProcessor(database);
        var processingParams = new ProcessingParams(
            OperationType.LowPass,
            CutoffFrequency: cutoffFreq,
            Order: 2
        );
        
        var processedSignal = await processor.Process(originalSignal.Id, processingParams);
        
        // Retrieve original signal from database
        var retrievedOriginal = await database.GetCompleteSignal(originalSignal.Id);
        
        // Verify original signal is unchanged
        var originalUnchanged = 
            retrievedOriginal != null &&
            retrievedOriginal.Samples.SequenceEqual(originalSamplesCopy);
        
        // Verify processed signal references original
        var referencesOriginal = 
            processedSignal.OriginalSignalId == originalSignal.Id;
        
        Assert.True(originalUnchanged && referencesOriginal);
    }

    [Fact(Skip = "Requires MongoDB and InfluxDB to be running - manual test only")]
    public async Task ProcessedSignalHasCorrectMetadata_ManualTest()
    {
        var gainValue = 3.0;
        
        // Create test configuration
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDB"] = "mongodb://localhost:27017",
                ["MongoDB:DatabaseName"] = "signal_processing_test",
                ["ConnectionStrings:InfluxDB"] = "http://localhost:8086",
                ["InfluxDB:Token"] = "",
                ["InfluxDB:Organization"] = "signal-processing-test",
                ["InfluxDB:Bucket"] = "signals_test"
            })
            .Build();

        // Initialize database contexts
        var mongoContext = new MongoDbContext(configuration);
        var influxContext = new InfluxDbContext(configuration);

        // Initialize database
        var metadataDb = new MongoMetadataDatabase(mongoContext);
        var timeSeriesDb = new InfluxTimeSeriesDatabase(influxContext);
        var database = new SignalDatabase(timeSeriesDb, metadataDb);
        
        // Create a test signal
        var generator = new SignalGenerator();
        var signalParams = new SignalGeneratorParams(
            SignalType.Square,
            Frequency: 500.0,
            Amplitude: 2.0,
            Phase: 0.0,
            Duration: 0.05,
            SampleRate: 22050
        );
        var originalSignal = await generator.Generate(signalParams);
        
        await database.SaveCompleteSignal(originalSignal);
        
        // Create processor and process signal
        var processor = new SignalProcessor(database);
        var processingParams = new ProcessingParams(
            OperationType.Gain,
            Gain: gainValue
        );
        
        var processedSignal = await processor.Process(originalSignal.Id, processingParams);
        
        // Verify processed signal has correct metadata
        var hasCorrectMetadata = 
            processedSignal.OriginalSignalId == originalSignal.Id &&
            processedSignal.ProcessingParams.Operation == OperationType.Gain &&
            processedSignal.ProcessingParams.Gain == gainValue &&
            processedSignal.Type == originalSignal.Type &&
            processedSignal.Metadata.Frequency == originalSignal.Metadata.Frequency &&
            processedSignal.Metadata.SampleRate == originalSignal.Metadata.SampleRate;
        
        Assert.True(hasCorrectMetadata);
    }
}
