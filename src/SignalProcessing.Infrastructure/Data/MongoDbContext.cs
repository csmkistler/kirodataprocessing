using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace SignalProcessing.Infrastructure.Data;

/// <summary>
/// MongoDB context for managing database connections and collections.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") 
            ?? "mongodb://localhost:27017";
        
        _client = new MongoClient(connectionString);
        
        var databaseName = configuration["MongoDB:DatabaseName"] 
            ?? "signal_processing";
        
        _database = _client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Gets a MongoDB collection by name.
    /// </summary>
    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
