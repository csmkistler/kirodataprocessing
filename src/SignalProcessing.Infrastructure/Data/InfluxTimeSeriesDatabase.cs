using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Infrastructure.Data;

/// <summary>
/// InfluxDB implementation of time-series database for storing signal samples.
/// </summary>
public class InfluxTimeSeriesDatabase : ITimeSeriesDatabase
{
    private readonly InfluxDbContext _context;

    public InfluxTimeSeriesDatabase(InfluxDbContext context)
    {
        _context = context;
    }

    public async Task WriteSamples(Guid signalId, SignalType type, double[] samples, double[] timestamps, bool isProcessed = false)
    {
        if (samples.Length != timestamps.Length)
        {
            throw new ArgumentException("Samples and timestamps arrays must have the same length");
        }

        var writeApi = _context.Client.GetWriteApiAsync();

        var points = new List<PointData>();
        var measurement = isProcessed ? "processed_signal_samples" : "signal_samples";

        for (int i = 0; i < samples.Length; i++)
        {
            var timestamp = DateTime.UnixEpoch.AddSeconds(timestamps[i]);
            
            var point = PointData
                .Measurement(measurement)
                .Tag("signal_id", signalId.ToString())
                .Tag("signal_type", type.ToString())
                .Tag("is_processed", isProcessed.ToString().ToLower())
                .Field("amplitude", samples[i])
                .Timestamp(timestamp, WritePrecision.Ms);

            points.Add(point);
        }

        // Write in batches for efficiency
        const int batchSize = 1000;
        for (int i = 0; i < points.Count; i += batchSize)
        {
            var batch = points.Skip(i).Take(batchSize).ToList();
            await writeApi.WritePointsAsync(batch, _context.Bucket, _context.Organization);
        }
    }

    public async Task<(double[] samples, double[] timestamps)> ReadSamples(Guid signalId, DateTime? start = null, DateTime? end = null)
    {
        var queryApi = _context.Client.GetQueryApi();

        var startTime = start?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "0";
        var endTime = end?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "now()";

        var flux = $@"
            from(bucket: ""{_context.Bucket}"")
                |> range(start: {startTime}, stop: {endTime})
                |> filter(fn: (r) => r[""_measurement""] == ""signal_samples"" or r[""_measurement""] == ""processed_signal_samples"")
                |> filter(fn: (r) => r[""signal_id""] == ""{signalId}"")
                |> filter(fn: (r) => r[""_field""] == ""amplitude"")
                |> sort(columns: [""_time""])
        ";

        var tables = await queryApi.QueryAsync(flux, _context.Organization);

        var samplesList = new List<double>();
        var timestampsList = new List<double>();

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                var amplitude = Convert.ToDouble(record.GetValue());
                var instant = record.GetTime();
                if (instant.HasValue)
                {
                    var time = instant.Value.ToDateTimeUtc();
                    var timestampSeconds = (time - DateTime.UnixEpoch).TotalSeconds;

                    samplesList.Add(amplitude);
                    timestampsList.Add(timestampSeconds);
                }
            }
        }

        return (samplesList.ToArray(), timestampsList.ToArray());
    }

    public async Task DeleteSamples(Guid signalId)
    {
        var deleteApi = _context.Client.GetDeleteApi();

        var start = DateTime.UnixEpoch;
        var stop = DateTime.UtcNow.AddDays(1);

        var predicate = $"signal_id=\"{signalId}\"";

        await deleteApi.Delete(start, stop, predicate, _context.Bucket, _context.Organization);
    }
}
