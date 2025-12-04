using InfluxDB.Client;
using Microsoft.Extensions.Configuration;

namespace SignalProcessing.Infrastructure.Data;

/// <summary>
/// InfluxDB context for managing time-series database connections.
/// </summary>
public class InfluxDbContext : IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly string _bucket;
    private readonly string _organization;

    public InfluxDbContext(IConfiguration configuration)
    {
        var url = configuration.GetConnectionString("InfluxDB") 
            ?? "http://localhost:8086";
        
        var token = configuration["InfluxDB:Token"] 
            ?? string.Empty;
        
        _bucket = configuration["InfluxDB:Bucket"] 
            ?? "signals";
        
        _organization = configuration["InfluxDB:Organization"] 
            ?? "signal-processing";

        _client = new InfluxDBClient(url, token);
    }

    public InfluxDBClient Client => _client;
    public string Bucket => _bucket;
    public string Organization => _organization;

    public void Dispose()
    {
        _client?.Dispose();
    }
}
