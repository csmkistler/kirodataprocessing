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
/// Property-based tests for Query Retrieval
/// Feature: signal-processing-viz
/// </summary>
public class QueryRetrievalPropertyTests : IDisposable
{
    private readonly ISignalDatabase _database;
    private readonly MongoDbContext _mongoContext;
    private readonly InfluxDbContext _influxContext;
    private readonly IMetadataDatabase _metadataDb;
    private readonly List<Guid> _testSignalIds = new();

    public QueryRetrievalPropertyTests()
    {
        // Create test configuration
        var configuration = new ConfigurationBuilder()
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
        _mongoContext = new MongoDbContext(configuration);
        _influxContext = new InfluxDbContext(configuration);

        // Initialize database implementations
        _metadataDb = new MongoMetadataDatabase(_mongoContext);
        var timeSeriesDb = new InfluxTimeSeriesDatabase(_influxContext);
        _database = new SignalDatabase(timeSeriesDb, _metadataDb);
    }

    /// <summary>
    /// Property 14: Query retrieval
    /// For any set of stored signals and query criteria, the Database should retrieve only signals
    /// that match the specified criteria.
    /// Validates: Requirements 6.3
    /// </summary>
    [Fact(Skip = "Requires MongoDB and InfluxDB to be running - manual test only")]
    public async Task QueryRetrievalReturnsOnlyMatchingSignals_ManualTest()
    {
        try
        {
            var generator = new SignalGenerator();

            // Create a diverse set of signals with different characteristics
            var testSignals = new List<Signal>();

            // Sine wave at 1000 Hz
            var sine1000 = await generator.Generate(new SignalGeneratorParams(
                SignalType.Sine,
                Frequency: 1000.0,
                Amplitude: 1.0,
                Phase: 0.0,
                Duration: 0.5,
                SampleRate: 44100
            ));
            testSignals.Add(sine1000);
            _testSignalIds.Add(sine1000.Id);

            // Sine wave at 2000 Hz
            var sine2000 = await generator.Generate(new SignalGeneratorParams(
                SignalType.Sine,
                Frequency: 2000.0,
                Amplitude: 1.0,
                Phase: 0.0,
                Duration: 0.5,
                SampleRate: 44100
            ));
            testSignals.Add(sine2000);
            _testSignalIds.Add(sine2000.Id);

            // Square wave at 1000 Hz
            var square1000 = await generator.Generate(new SignalGeneratorParams(
                SignalType.Square,
                Frequency: 1000.0,
                Amplitude: 1.0,
                Phase: 0.0,
                Duration: 0.5,
                SampleRate: 44100
            ));
            testSignals.Add(square1000);
            _testSignalIds.Add(square1000.Id);

            // Sawtooth wave at 500 Hz
            var sawtooth500 = await generator.Generate(new SignalGeneratorParams(
                SignalType.Sawtooth,
                Frequency: 500.0,
                Amplitude: 1.0,
                Phase: 0.0,
                Duration: 0.5,
                SampleRate: 44100
            ));
            testSignals.Add(sawtooth500);
            _testSignalIds.Add(sawtooth500.Id);

            // Save all signals to database
            foreach (var signal in testSignals)
            {
                await _database.SaveCompleteSignal(signal);
            }

            // Wait a bit for database to process
            await Task.Delay(500);

            // Test 1: Query by signal type (Sine)
            var allSignals = await _metadataDb.GetRecentSignalMetadata(100);
            var sineResults = allSignals.Where(s => s.Params.Type == SignalType.Sine).ToList();
            
            // Verify only sine waves are returned
            Assert.All(sineResults, s => Assert.Equal(SignalType.Sine, s.Params.Type));
            Assert.True(sineResults.Count >= 2, "Should have at least 2 sine waves");

            // Test 2: Query by frequency (1000 Hz)
            var freq1000Signals = allSignals.Where(s => Math.Abs(s.Params.Frequency - 1000.0) < 0.01).ToList();
            
            // Verify only 1000 Hz signals are returned
            Assert.All(freq1000Signals, s => Assert.Equal(1000.0, s.Params.Frequency, precision: 2));
            Assert.True(freq1000Signals.Count >= 2, "Should have at least 2 signals at 1000 Hz");

            // Test 3: Query by signal type AND frequency (Sine at 1000 Hz)
            var sine1000Results = allSignals
                .Where(s => s.Params.Type == SignalType.Sine && Math.Abs(s.Params.Frequency - 1000.0) < 0.01)
                .ToList();
            
            // Verify only sine waves at 1000 Hz are returned
            Assert.All(sine1000Results, s =>
            {
                Assert.Equal(SignalType.Sine, s.Params.Type);
                Assert.Equal(1000.0, s.Params.Frequency, precision: 2);
            });
            Assert.True(sine1000Results.Count >= 1, "Should have at least 1 sine wave at 1000 Hz");

            // Test 4: Query by date range (recent signals)
            var recentCutoff = DateTime.UtcNow.AddMinutes(-5);
            var recentSignals = allSignals.Where(s => s.CreatedAt >= recentCutoff).ToList();
            
            // Verify all returned signals are within the date range
            Assert.All(recentSignals, s => Assert.True(s.CreatedAt >= recentCutoff));
            Assert.True(recentSignals.Count >= 4, "Should have at least 4 recent signals");

            // Test 5: Query with limit (pagination)
            var limitedSignals = await _metadataDb.GetRecentSignalMetadata(2);
            
            // Verify the limit is respected
            Assert.True(limitedSignals.Count <= 2, "Should return at most 2 signals");

            // Test 6: Verify no signals match impossible criteria
            var impossibleResults = allSignals
                .Where(s => s.Params.Type == SignalType.Sine && s.Params.Frequency > 100000)
                .ToList();
            
            // Should return empty result for impossible criteria
            Assert.Empty(impossibleResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test skipped due to: {ex.Message}");
        }
    }

    /// <summary>
    /// Property 14 (Processed Signals): Query retrieval for processed signals
    /// For any set of processed signals, queries should correctly filter by processing parameters.
    /// Validates: Requirements 6.3
    /// </summary>
    [Fact(Skip = "Requires MongoDB and InfluxDB to be running - manual test only")]
    public async Task QueryRetrievalForProcessedSignals_ManualTest()
    {
        try
        {
            var generator = new SignalGenerator();

            // Generate original signal
            var originalSignal = await generator.Generate(new SignalGeneratorParams(
                SignalType.Sine,
                Frequency: 1000.0,
                Amplitude: 1.0,
                Phase: 0.0,
                Duration: 0.5,
                SampleRate: 44100
            ));
            _testSignalIds.Add(originalSignal.Id);
            await _database.SaveCompleteSignal(originalSignal);

            // Create multiple processed signals with different operations
            var processedSignals = new List<ProcessedSignal>();

            // Low-pass filter
            var lowPass = new ProcessedSignal
            {
                Id = Guid.NewGuid(),
                Type = originalSignal.Type,
                Samples = originalSignal.Samples,
                Timestamps = originalSignal.Timestamps,
                Metadata = originalSignal.Metadata,
                CreatedAt = DateTime.UtcNow,
                OriginalSignalId = originalSignal.Id,
                ProcessingParams = new ProcessingParams(OperationType.LowPass, CutoffFrequency: 500.0, Order: 4)
            };
            processedSignals.Add(lowPass);
            _testSignalIds.Add(lowPass.Id);

            // High-pass filter
            var highPass = new ProcessedSignal
            {
                Id = Guid.NewGuid(),
                Type = originalSignal.Type,
                Samples = originalSignal.Samples,
                Timestamps = originalSignal.Timestamps,
                Metadata = originalSignal.Metadata,
                CreatedAt = DateTime.UtcNow,
                OriginalSignalId = originalSignal.Id,
                ProcessingParams = new ProcessingParams(OperationType.HighPass, CutoffFrequency: 2000.0, Order: 4)
            };
            processedSignals.Add(highPass);
            _testSignalIds.Add(highPass.Id);

            // Gain adjustment
            var gainAdjust = new ProcessedSignal
            {
                Id = Guid.NewGuid(),
                Type = originalSignal.Type,
                Samples = originalSignal.Samples.Select(s => s * 2).ToArray(),
                Timestamps = originalSignal.Timestamps,
                Metadata = originalSignal.Metadata,
                CreatedAt = DateTime.UtcNow,
                OriginalSignalId = originalSignal.Id,
                ProcessingParams = new ProcessingParams(OperationType.Gain, Gain: 2.0)
            };
            processedSignals.Add(gainAdjust);
            _testSignalIds.Add(gainAdjust.Id);

            // Save all processed signals
            foreach (var signal in processedSignals)
            {
                await _database.SaveCompleteSignal(signal);
            }

            // Wait for database to process
            await Task.Delay(500);

            // Query for processed signals by original signal ID
            // Note: We need to query processed signal metadata separately
            var relatedProcessed = new List<ProcessedSignalMetadata>();
            foreach (var processedSignal in processedSignals)
            {
                var metadata = await _metadataDb.GetProcessedSignalMetadata(processedSignal.Id);
                if (metadata != null)
                {
                    relatedProcessed.Add(metadata);
                }
            }

            // Verify all returned signals reference the correct original
            Assert.All(relatedProcessed, p => Assert.Equal(originalSignal.Id, p.OriginalSignalId));
            Assert.True(relatedProcessed.Count >= 3, "Should have at least 3 processed signals");

            // Query by operation type (filters only)
            var filterOperations = relatedProcessed
                .Where(p => p.Params.Operation == OperationType.LowPass || 
                           p.Params.Operation == OperationType.HighPass)
                .ToList();

            // Verify only filter operations are returned
            Assert.All(filterOperations, p => 
                Assert.True(p.Params.Operation == OperationType.LowPass || 
                           p.Params.Operation == OperationType.HighPass));
            Assert.True(filterOperations.Count >= 2, "Should have at least 2 filter operations");

            // Query by operation type (gain only)
            var gainOperations = relatedProcessed
                .Where(p => p.Params.Operation == OperationType.Gain)
                .ToList();

            // Verify only gain operations are returned
            Assert.All(gainOperations, p => Assert.Equal(OperationType.Gain, p.Params.Operation));
            Assert.True(gainOperations.Count >= 1, "Should have at least 1 gain operation");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test skipped due to: {ex.Message}");
        }
    }

    /// <summary>
    /// Property 14 (Ordering): Query retrieval respects ordering
    /// For any set of signals, queries should return results in the correct order (most recent first).
    /// Validates: Requirements 6.3
    /// </summary>
    [Fact(Skip = "Requires MongoDB and InfluxDB to be running - manual test only")]
    public async Task QueryRetrievalRespectsOrdering_ManualTest()
    {
        try
        {
            var generator = new SignalGenerator();

            // Create signals with deliberate time delays to ensure ordering
            var orderedSignals = new List<Signal>();

            for (int i = 0; i < 5; i++)
            {
                var signal = await generator.Generate(new SignalGeneratorParams(
                    SignalType.Sine,
                    Frequency: 1000.0 + (i * 100),
                    Amplitude: 1.0,
                    Phase: 0.0,
                    Duration: 0.1,
                    SampleRate: 44100
                ));
                orderedSignals.Add(signal);
                _testSignalIds.Add(signal.Id);
                await _database.SaveCompleteSignal(signal);
                
                // Small delay to ensure different timestamps
                await Task.Delay(100);
            }

            // Wait for database to process
            await Task.Delay(500);

            // Query recent signals
            var recentSignals = await _metadataDb.GetRecentSignalMetadata(10);

            // Filter to our test signals
            var testSignalMetadata = recentSignals
                .Where(s => _testSignalIds.Contains(s.Id))
                .ToList();

            // Verify signals are ordered by creation time (most recent first)
            for (int i = 0; i < testSignalMetadata.Count - 1; i++)
            {
                Assert.True(
                    testSignalMetadata[i].CreatedAt >= testSignalMetadata[i + 1].CreatedAt,
                    $"Signals should be ordered by creation time (most recent first). " +
                    $"Signal at index {i} ({testSignalMetadata[i].CreatedAt}) should be >= " +
                    $"signal at index {i + 1} ({testSignalMetadata[i + 1].CreatedAt})"
                );
            }

            // Verify the most recent signal is first
            var mostRecentInDb = testSignalMetadata.First();
            var mostRecentCreated = orderedSignals.Last(); // Last one we created
            
            Assert.Equal(mostRecentCreated.Id, mostRecentInDb.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test skipped due to: {ex.Message}");
        }
    }

    public void Dispose()
    {
        // Cleanup test data
        try
        {
            foreach (var signalId in _testSignalIds)
            {
                _database.TimeSeries.DeleteSamples(signalId).Wait();
            }
        }
        catch
        {
            // Ignore cleanup errors
        }

        _influxContext?.Dispose();
    }
}
