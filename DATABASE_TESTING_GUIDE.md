# Database Layer Testing Guide

This guide explains how to test the database layer implementation for the Signal Processing application.

## Prerequisites

To test the database functionality, you need to have MongoDB and InfluxDB running locally.

### Option 1: Install Locally (Recommended for Development)

#### MongoDB
1. Download MongoDB Community Edition: https://www.mongodb.com/try/download/community
2. Install and start the MongoDB service
3. Default connection: `mongodb://localhost:27017`

#### InfluxDB
1. Download InfluxDB: https://portal.influxdata.com/downloads/
2. Install and start InfluxDB
3. Default connection: `http://localhost:8086`
4. For this demo, no authentication token is required (empty token)

### Option 2: Use Docker (Quick Setup)

```bash
# Start MongoDB
docker run -d -p 27017:27017 --name mongodb mongo:latest

# Start InfluxDB
docker run -d -p 8086:8086 --name influxdb influxdb:latest
```

## Testing Methods

### Method 1: Run the Database Demo (Easiest)

The database demo is a console application that demonstrates all database functionality:

```bash
# Navigate to the project root
cd src/SignalProcessing.Demo

# Run the database demo
dotnet run --database
```

**What it tests:**
- ✓ MongoDB connection and metadata storage
- ✓ InfluxDB connection and time-series data storage
- ✓ Signal generation and persistence
- ✓ Signal retrieval (round-trip)
- ✓ Processed signal creation and storage
- ✓ Referential integrity (processed signals reference originals)
- ✓ Querying recent signals

**Expected Output:**
```
=== Signal Processing Database Demo ===

Initializing database connections...
✓ Database connections initialized

Generating a test signal...
✓ Generated signal: Sine, 4410 samples
  ID: [guid]
  Frequency: 440 Hz
  Duration: 0.1 seconds

Saving signal to database...
✓ Signal saved to MongoDB (metadata) and InfluxDB (samples)

Retrieving signal from database...
✓ Retrieved signal: Sine, 4410 samples
  Metadata matches: True
  Sample count matches: True

[... more output ...]

=== Demo completed successfully! ===
```

### Method 2: Run Unit Tests

The property-based tests are comprehensive but require databases to be running:

```bash
# Run all tests (skipped tests will be shown)
dotnet test src/SignalProcessing.Tests/SignalProcessing.Tests.csproj

# To run the database tests, remove the Skip attribute from the tests
# Edit: src/SignalProcessing.Tests/SignalPersistencePropertyTests.cs
# Change: [Fact(Skip = "Requires MongoDB and InfluxDB to be running - manual test only")]
# To:     [Fact]

# Then run tests again
dotnet test src/SignalProcessing.Tests/SignalProcessing.Tests.csproj --filter "FullyQualifiedName~Persistence"
```

**Tests included:**
1. `SignalPersistencePreservesAllData_ManualTest` - Tests Property 13 (Requirements 6.1, 6.2)
2. `ProcessedSignalPersistencePreservesMetadata_ManualTest` - Tests processed signal metadata
3. `ProcessedSignalMaintainsReferentialIntegrity_ManualTest` - Tests Property 15 (Requirement 6.4)
4. `MultipleProcessedSignalsMaintainReferentialIntegrity_ManualTest` - Tests multiple references

### Method 3: Manual Testing with MongoDB Compass

1. Install MongoDB Compass: https://www.mongodb.com/try/download/compass
2. Connect to `mongodb://localhost:27017`
3. Run the database demo (Method 1)
4. In Compass, browse the `signal_processing_demo` database
5. Inspect collections:
   - `signal_metadata` - Contains signal generation parameters
   - `processed_signal_metadata` - Contains processing parameters
   - `trigger_events` - Contains trigger events (if any)
   - `app_config` - Contains application configuration

### Method 4: Manual Testing with InfluxDB UI

1. Open browser to `http://localhost:8086`
2. Create an account (first time only)
3. Run the database demo (Method 1)
4. In InfluxDB UI:
   - Navigate to Data Explorer
   - Select bucket: `signals_demo`
   - Query measurements: `signal_samples` and `processed_signal_samples`
   - View the time-series data

## Troubleshooting

### "Connection refused" or "Unable to connect"

**Problem:** MongoDB or InfluxDB is not running.

**Solution:**
```bash
# Check if MongoDB is running
# Windows: Check Services app for "MongoDB"
# Linux/Mac: ps aux | grep mongod

# Check if InfluxDB is running
# Windows: Check Services app for "InfluxDB"
# Linux/Mac: ps aux | grep influxd

# Or check ports
netstat -an | findstr "27017"  # MongoDB
netstat -an | findstr "8086"   # InfluxDB
```

### "Database not found" or "Bucket not found"

**Problem:** First-time setup - databases/buckets don't exist yet.

**Solution:** The application will create them automatically on first run. Just run the demo again.

### Tests are skipped

**Problem:** Tests have `Skip` attribute to prevent failures in CI/CD.

**Solution:** This is expected. Either:
1. Use the database demo (Method 1) instead
2. Remove the `Skip` attribute from tests manually

## What Gets Tested

### Property 13: Signal Persistence with Metadata (Requirements 6.1, 6.2)
- Signal samples are stored in InfluxDB
- Signal metadata is stored in MongoDB
- Round-trip: Save → Retrieve → Verify all data matches
- Works for both regular and processed signals

### Property 15: Referential Integrity (Requirement 6.4)
- Processed signals store reference to original signal ID
- Original signal can be retrieved using the reference
- Multiple processed signals can reference the same original
- References remain valid across database operations

## Database Schema

### MongoDB Collections

**signal_metadata:**
```json
{
  "_id": "guid",
  "type": "Sine",
  "frequency": 440.0,
  "amplitude": 1.0,
  "phase": 0.0,
  "duration": 0.1,
  "sampleRate": 44100,
  "createdAt": "2025-12-04T10:00:00Z"
}
```

**processed_signal_metadata:**
```json
{
  "_id": "guid",
  "originalSignalId": "original-guid",
  "operation": "Gain",
  "gain": 2.0,
  "createdAt": "2025-12-04T10:01:00Z"
}
```

### InfluxDB Measurements

**signal_samples:**
- Tags: `signal_id`, `signal_type`, `is_processed`
- Fields: `amplitude` (double)
- Timestamp: sample timestamp

## Next Steps

After verifying the database layer works:

1. ✓ Task 3 is complete - Database layer implemented
2. → Task 4: Implement Signal Processor component
3. → Task 5: Implement Trigger Component
4. → Task 6: Implement Application Services layer

## Additional Resources

- MongoDB Documentation: https://docs.mongodb.com/
- InfluxDB Documentation: https://docs.influxdata.com/
- Design Document: `.kiro/specs/signal-processing-viz/design.md`
- Requirements: `.kiro/specs/signal-processing-viz/requirements.md`
