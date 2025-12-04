---
inclusion: always
---

# MongoDB Patterns and Best Practices

## Connection Management

### MongoDB Client Setup
```csharp
// Singleton client (thread-safe)
public class MongoDbContext
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB");
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase("signal_processing");
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
```

### Connection String Configuration
```json
// appsettings.json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "signal_processing",
    "MaxConnectionPoolSize": 100
  }
}
```

## Repository Pattern

### Generic Repository Interface
```csharp
public interface IMongoRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> filter);
    Task InsertAsync(T entity);
    Task UpdateAsync(Guid id, T entity);
    Task DeleteAsync(Guid id);
}
```

### Repository Implementation
```csharp
public class MongoRepository<T> : IMongoRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(MongoDbContext context, string collectionName)
    {
        _collection = context.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task InsertAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(Guid id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        await _collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        await _collection.DeleteOneAsync(filter);
    }
}
```

## Document Modeling

### Entity Attributes
```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class SignalMetadata
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public SignalType Type { get; set; }

    [BsonElement("frequency")]
    public double Frequency { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonIgnoreIfNull]
    public ProcessingParams? ProcessingParams { get; set; }
}
```

### Enum Serialization
```csharp
// Register enum as string (not int)
BsonSerializer.RegisterSerializer(new EnumSerializer<SignalType>(BsonType.String));
```

## Querying Patterns

### Filter Builders
```csharp
// Simple equality
var filter = Builders<SignalMetadata>.Filter.Eq(s => s.Type, SignalType.Sine);

// Multiple conditions (AND)
var filter = Builders<SignalMetadata>.Filter.And(
    Builders<SignalMetadata>.Filter.Eq(s => s.Type, SignalType.Sine),
    Builders<SignalMetadata>.Filter.Gt(s => s.Frequency, 1000)
);

// OR conditions
var filter = Builders<SignalMetadata>.Filter.Or(
    Builders<SignalMetadata>.Filter.Eq(s => s.Type, SignalType.Sine),
    Builders<SignalMetadata>.Filter.Eq(s => s.Type, SignalType.Square)
);

// Date range
var filter = Builders<SignalMetadata>.Filter.And(
    Builders<SignalMetadata>.Filter.Gte(s => s.CreatedAt, startDate),
    Builders<SignalMetadata>.Filter.Lte(s => s.CreatedAt, endDate)
);
```

### Projection
```csharp
// Select specific fields
var projection = Builders<SignalMetadata>.Projection
    .Include(s => s.Id)
    .Include(s => s.Type)
    .Include(s => s.Frequency)
    .Exclude("_id"); // Exclude MongoDB's internal _id if needed

var results = await _collection
    .Find(filter)
    .Project<SignalMetadataDto>(projection)
    .ToListAsync();
```

### Sorting and Pagination
```csharp
public async Task<List<SignalMetadata>> GetRecentSignalsAsync(int limit)
{
    return await _collection
        .Find(Builders<SignalMetadata>.Filter.Empty)
        .SortByDescending(s => s.CreatedAt)
        .Limit(limit)
        .ToListAsync();
}

// Pagination
public async Task<List<SignalMetadata>> GetPagedSignalsAsync(int page, int pageSize)
{
    return await _collection
        .Find(Builders<SignalMetadata>.Filter.Empty)
        .SortByDescending(s => s.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();
}
```

## Indexing

### Create Indexes
```csharp
public class MongoDbInitializer
{
    private readonly MongoDbContext _context;

    public async Task CreateIndexesAsync()
    {
        var signalCollection = _context.GetCollection<SignalMetadata>("signal_metadata");

        // Single field index
        var idIndex = Builders<SignalMetadata>.IndexKeys.Ascending(s => s.Id);
        await signalCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SignalMetadata>(idIndex, new CreateIndexOptions { Unique = true })
        );

        // Compound index
        var typeFreqIndex = Builders<SignalMetadata>.IndexKeys
            .Ascending(s => s.Type)
            .Descending(s => s.Frequency);
        await signalCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SignalMetadata>(typeFreqIndex)
        );

        // Date index for time-based queries
        var dateIndex = Builders<SignalMetadata>.IndexKeys.Descending(s => s.CreatedAt);
        await signalCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SignalMetadata>(dateIndex)
        );
    }
}
```

## Transactions (if needed)

### Using Transactions
```csharp
public async Task SaveSignalWithMetadataAsync(Signal signal)
{
    using var session = await _client.StartSessionAsync();
    session.StartTransaction();

    try
    {
        // Insert signal metadata
        await _metadataCollection.InsertOneAsync(session, signal.Metadata);

        // Insert related data
        await _eventsCollection.InsertOneAsync(session, new TriggerEvent { ... });

        await session.CommitTransactionAsync();
    }
    catch
    {
        await session.AbortTransactionAsync();
        throw;
    }
}
```

## Error Handling

### Common MongoDB Exceptions
```csharp
try
{
    await _collection.InsertOneAsync(entity);
}
catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
{
    throw new DuplicateEntityException($"Entity with ID {entity.Id} already exists");
}
catch (MongoConnectionException ex)
{
    _logger.LogError(ex, "Failed to connect to MongoDB");
    throw new DatabaseConnectionException("Database connection failed", ex);
}
catch (MongoException ex)
{
    _logger.LogError(ex, "MongoDB operation failed");
    throw new DatabaseOperationException("Database operation failed", ex);
}
```

## Best Practices

### 1. Use Async Methods
Always use async methods for database operations to avoid blocking threads.

### 2. Connection Pooling
MongoDB driver handles connection pooling automatically. Use a singleton MongoClient.

### 3. Batch Operations
```csharp
// Bulk insert
await _collection.InsertManyAsync(entities);

// Bulk write
var bulkOps = entities.Select(e => 
    new InsertOneModel<SignalMetadata>(e)
);
await _collection.BulkWriteAsync(bulkOps);
```

### 4. Avoid N+1 Queries
```csharp
// Bad: N+1 queries
foreach (var signalId in signalIds)
{
    var signal = await _collection.Find(s => s.Id == signalId).FirstOrDefaultAsync();
}

// Good: Single query
var filter = Builders<SignalMetadata>.Filter.In(s => s.Id, signalIds);
var signals = await _collection.Find(filter).ToListAsync();
```

### 5. Use Projections
Only retrieve fields you need to reduce network overhead.

### 6. Index Frequently Queried Fields
Create indexes on fields used in filters, sorts, and joins.
