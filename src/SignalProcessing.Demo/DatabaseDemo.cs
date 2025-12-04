using SignalProcessing.Core.Entities;
using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;
using SignalProcessing.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace SignalProcessing.Demo;

/// <summary>
/// Demonstrates database functionality with MongoDB and InfluxDB.
/// </summary>
public class DatabaseDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("=== Signal Processing Database Demo ===\n");

        try
        {
            // Create configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MongoDB"] = "mongodb://localhost:27017",
                    ["MongoDB:DatabaseName"] = "signal_processing_demo",
                    ["ConnectionStrings:InfluxDB"] = "http://localhost:8086",
                    ["InfluxDB:Token"] = "",
                    ["InfluxDB:Organization"] = "signal-processing",
                    ["InfluxDB:Bucket"] = "signals_demo"
                })
                .Build();

            // Initialize database
            Console.WriteLine("Initializing database connections...");
            var mongoContext = new MongoDbContext(configuration);
            var influxContext = new InfluxDbContext(configuration);
            var metadataDb = new MongoMetadataDatabase(mongoContext);
            var timeSeriesDb = new InfluxTimeSeriesDatabase(influxContext);
            var database = new SignalDatabase(timeSeriesDb, metadataDb);
            Console.WriteLine("✓ Database connections initialized\n");

            // Generate a signal
            Console.WriteLine("Generating a test signal...");
            var generator = new SignalGenerator();
            var parameters = new SignalGeneratorParams(
                SignalType.Sine,
                Frequency: 440.0,  // A4 note
                Amplitude: 1.0,
                Phase: 0.0,
                Duration: 0.1,     // 100ms
                SampleRate: 44100
            );

            var signal = await generator.Generate(parameters);
            Console.WriteLine($"✓ Generated signal: {signal.Type}, {signal.Samples.Length} samples");
            Console.WriteLine($"  ID: {signal.Id}");
            Console.WriteLine($"  Frequency: {signal.Metadata.Frequency} Hz");
            Console.WriteLine($"  Duration: {signal.Metadata.Duration} seconds\n");

            // Save signal to database
            Console.WriteLine("Saving signal to database...");
            await database.SaveCompleteSignal(signal);
            Console.WriteLine("✓ Signal saved to MongoDB (metadata) and InfluxDB (samples)\n");

            // Retrieve signal from database
            Console.WriteLine("Retrieving signal from database...");
            var retrievedSignal = await database.GetCompleteSignal(signal.Id);
            Console.WriteLine($"✓ Retrieved signal: {retrievedSignal.Type}, {retrievedSignal.Samples.Length} samples");
            Console.WriteLine($"  Metadata matches: {retrievedSignal.Metadata.Frequency == signal.Metadata.Frequency}");
            Console.WriteLine($"  Sample count matches: {retrievedSignal.Samples.Length == signal.Samples.Length}\n");

            // Create a processed signal
            Console.WriteLine("Creating processed signal (2x gain)...");
            var processedSignal = new ProcessedSignal
            {
                Id = Guid.NewGuid(),
                Type = signal.Type,
                Samples = signal.Samples.Select(s => s * 2).ToArray(),
                Timestamps = signal.Timestamps,
                Metadata = signal.Metadata,
                CreatedAt = DateTime.UtcNow,
                OriginalSignalId = signal.Id,
                ProcessingParams = new ProcessingParams(OperationType.Gain, Gain: 2.0)
            };
            Console.WriteLine($"✓ Created processed signal: {processedSignal.Id}\n");

            // Save processed signal
            Console.WriteLine("Saving processed signal...");
            await database.SaveCompleteSignal(processedSignal);
            Console.WriteLine("✓ Processed signal saved\n");

            // Retrieve processed signal and verify referential integrity
            Console.WriteLine("Verifying referential integrity...");
            var retrievedProcessed = await database.GetCompleteSignal(processedSignal.Id);
            if (retrievedProcessed is ProcessedSignal processed)
            {
                Console.WriteLine($"✓ Retrieved as ProcessedSignal");
                Console.WriteLine($"  Original signal ID: {processed.OriginalSignalId}");
                Console.WriteLine($"  Processing operation: {processed.ProcessingParams.Operation}");
                Console.WriteLine($"  Gain: {processed.ProcessingParams.Gain}\n");

                // Retrieve original signal using the reference
                Console.WriteLine("Retrieving original signal via reference...");
                var originalViaRef = await database.GetCompleteSignal(processed.OriginalSignalId);
                Console.WriteLine($"✓ Original signal retrieved: {originalViaRef.Id}");
                Console.WriteLine($"  IDs match: {originalViaRef.Id == signal.Id}\n");
            }

            // Get recent signals
            Console.WriteLine("Retrieving recent signals...");
            var recentSignals = await database.Metadata.GetRecentSignalMetadata(10);
            Console.WriteLine($"✓ Found {recentSignals.Count} recent signals");
            foreach (var meta in recentSignals.Take(3))
            {
                Console.WriteLine($"  - {meta.Params.Type} @ {meta.Params.Frequency} Hz (ID: {meta.Id})");
            }

            Console.WriteLine("\n=== Demo completed successfully! ===");

            // Cleanup
            influxContext.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine("\nMake sure MongoDB and InfluxDB are running:");
            Console.WriteLine("  MongoDB: mongodb://localhost:27017");
            Console.WriteLine("  InfluxDB: http://localhost:8086");
            Console.WriteLine("\nTo install:");
            Console.WriteLine("  MongoDB: https://www.mongodb.com/try/download/community");
            Console.WriteLine("  InfluxDB: https://portal.influxdata.com/downloads/");
        }
    }
}
