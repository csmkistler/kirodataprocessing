using FsCheck;
using FsCheck.Xunit;
using Moq;
using SignalProcessing.Core.Entities;
using SignalProcessing.Core.Interfaces;
using SignalProcessing.Core.ValueObjects;
using SignalProcessing.Infrastructure;
using Xunit;

namespace SignalProcessing.Tests;

/// <summary>
/// Property-based tests for TriggerComponent
/// Feature: signal-processing-viz
/// </summary>
public class TriggerComponentPropertyTests
{
    /// <summary>
    /// Property 9: Threshold comparison
    /// For any numeric input value and configured threshold, the Trigger Component should
    /// correctly determine whether the value exceeds the threshold.
    /// Validates: Requirements 5.1
    /// </summary>
    [Property(MaxTest = 100)]
    public void ThresholdComparison_ValueAboveThreshold_ReturnsEvent(double threshold, PositiveInt delta)
    {
        // Skip extreme values that would cause overflow
        if (double.IsInfinity(threshold) || double.IsNaN(threshold) || 
            Math.Abs(threshold) > 1e100)
        {
            return;
        }

        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        mockDatabase.Setup(db => db.SaveEvent(It.IsAny<TriggerEvent>()))
            .Returns(Task.CompletedTask);

        var triggerComponent = new TriggerComponent(mockDatabase.Object);
        var config = new TriggerConfig(threshold, Enabled: true);
        triggerComponent.Configure(config);

        // Generate a value that is definitely above the threshold
        // Use a safe delta to avoid overflow
        var safeDelta = Math.Min(Math.Abs(delta.Get), 1e10);
        var value = threshold + safeDelta + 1.0;

        // Act
        var result = triggerComponent.CheckValue(value).Result;

        // Assert
        var eventWasEmitted = result != null;
        var eventHasCorrectValue = result?.Value == value;
        var eventHasCorrectThreshold = result?.Threshold == threshold;

        Assert.True(eventWasEmitted, $"Expected event for value {value} > threshold {threshold}");
        Assert.True(eventHasCorrectValue, "Event should contain the input value");
        Assert.True(eventHasCorrectThreshold, "Event should contain the threshold");
    }

    [Property(MaxTest = 100)]
    public void ThresholdComparison_ValueBelowThreshold_ReturnsNull(double threshold, PositiveInt delta)
    {
        // Skip extreme values that would cause overflow
        if (double.IsInfinity(threshold) || double.IsNaN(threshold) || 
            Math.Abs(threshold) > 1e100)
        {
            return;
        }

        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        var triggerComponent = new TriggerComponent(mockDatabase.Object);
        var config = new TriggerConfig(threshold, Enabled: true);
        triggerComponent.Configure(config);

        // Generate a value that is definitely below the threshold
        // Use a safe delta to avoid overflow
        var safeDelta = Math.Min(Math.Abs(delta.Get), 1e10);
        var value = threshold - safeDelta - 1.0;

        // Act
        var result = triggerComponent.CheckValue(value).Result;

        // Assert
        Assert.Null(result);
        
        // Verify SaveEvent was never called
        mockDatabase.Verify(db => db.SaveEvent(It.IsAny<TriggerEvent>()), Times.Never);
    }

    [Property(MaxTest = 100)]
    public void ThresholdComparison_ValueEqualToThreshold_ReturnsNull(double threshold)
    {
        // Skip extreme values
        if (double.IsInfinity(threshold) || double.IsNaN(threshold))
        {
            return;
        }

        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        var triggerComponent = new TriggerComponent(mockDatabase.Object);
        var config = new TriggerConfig(threshold, Enabled: true);
        triggerComponent.Configure(config);

        // Act - value exactly equal to threshold should NOT trigger
        var result = triggerComponent.CheckValue(threshold).Result;

        // Assert
        Assert.Null(result);
        
        // Verify SaveEvent was never called
        mockDatabase.Verify(db => db.SaveEvent(It.IsAny<TriggerEvent>()), Times.Never);
    }

    [Property(MaxTest = 100)]
    public void ThresholdComparison_DisabledConfig_ReturnsNull(double threshold, double value)
    {
        // Skip extreme values
        if (double.IsInfinity(threshold) || double.IsNaN(threshold) ||
            double.IsInfinity(value) || double.IsNaN(value))
        {
            return;
        }

        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        var triggerComponent = new TriggerComponent(mockDatabase.Object);
        var config = new TriggerConfig(threshold, Enabled: false);
        triggerComponent.Configure(config);

        // Act - even if value exceeds threshold, disabled config should return null
        var result = triggerComponent.CheckValue(value).Result;

        // Assert
        Assert.Null(result);
        
        // Verify SaveEvent was never called
        mockDatabase.Verify(db => db.SaveEvent(It.IsAny<TriggerEvent>()), Times.Never);
    }

    [Fact]
    public void ThresholdComparison_NotConfigured_ThrowsException()
    {
        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        var triggerComponent = new TriggerComponent(mockDatabase.Object);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await triggerComponent.CheckValue(10.0)
        ).Result;

        Assert.Contains("Threshold must be configured", exception.Message);
    }

    /// <summary>
    /// Property 10: Event emission on threshold exceeded
    /// For any input value that exceeds the configured threshold, the Trigger Component should
    /// emit an event containing the input value and a timestamp.
    /// Validates: Requirements 5.2
    /// </summary>
    [Property(MaxTest = 100)]
    public void EventEmission_ThresholdExceeded_EmitsEventWithCorrectData(double threshold, PositiveInt delta)
    {
        // Skip extreme values that would cause overflow
        if (double.IsInfinity(threshold) || double.IsNaN(threshold) || 
            Math.Abs(threshold) > 1e100)
        {
            return;
        }

        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        TriggerEvent? savedEvent = null;
        
        mockDatabase.Setup(db => db.SaveEvent(It.IsAny<TriggerEvent>()))
            .Callback<TriggerEvent>(e => savedEvent = e)
            .Returns(Task.CompletedTask);

        var triggerComponent = new TriggerComponent(mockDatabase.Object);
        var config = new TriggerConfig(threshold, Enabled: true);
        triggerComponent.Configure(config);

        // Generate a value that exceeds the threshold
        var safeDelta = Math.Min(Math.Abs(delta.Get), 1e10);
        var value = threshold + safeDelta + 1.0;
        var beforeTimestamp = DateTime.UtcNow;

        // Act
        var result = triggerComponent.CheckValue(value).Result;
        var afterTimestamp = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(savedEvent);
        
        // Verify event contains correct value
        Assert.Equal(value, result.Value);
        Assert.Equal(value, savedEvent.Value);
        
        // Verify event contains correct threshold
        Assert.Equal(threshold, result.Threshold);
        Assert.Equal(threshold, savedEvent.Threshold);
        
        // Verify event has a valid ID
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(Guid.Empty, savedEvent.Id);
        
        // Verify event has a timestamp within reasonable bounds
        Assert.True(result.Timestamp >= beforeTimestamp && result.Timestamp <= afterTimestamp,
            $"Timestamp {result.Timestamp} should be between {beforeTimestamp} and {afterTimestamp}");
        Assert.True(savedEvent.Timestamp >= beforeTimestamp && savedEvent.Timestamp <= afterTimestamp,
            $"Saved timestamp {savedEvent.Timestamp} should be between {beforeTimestamp} and {afterTimestamp}");
        
        // Verify SaveEvent was called exactly once
        mockDatabase.Verify(db => db.SaveEvent(It.IsAny<TriggerEvent>()), Times.Once);
    }

    [Property(MaxTest = 100)]
    public void EventEmission_ThresholdNotExceeded_NoEventEmitted(double threshold, PositiveInt delta)
    {
        // Skip extreme values that would cause overflow
        if (double.IsInfinity(threshold) || double.IsNaN(threshold) || 
            Math.Abs(threshold) > 1e100)
        {
            return;
        }

        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        var triggerComponent = new TriggerComponent(mockDatabase.Object);
        var config = new TriggerConfig(threshold, Enabled: true);
        triggerComponent.Configure(config);

        // Generate a value that does NOT exceed the threshold
        var safeDelta = Math.Min(Math.Abs(delta.Get), 1e10);
        var value = threshold - safeDelta - 1.0;

        // Act
        var result = triggerComponent.CheckValue(value).Result;

        // Assert
        Assert.Null(result);
        
        // Verify SaveEvent was never called
        mockDatabase.Verify(db => db.SaveEvent(It.IsAny<TriggerEvent>()), Times.Never);
    }

    [Property(MaxTest = 100)]
    public void EventEmission_MultipleValues_EmitsMultipleEvents(double threshold, PositiveInt count)
    {
        // Skip extreme values
        if (double.IsInfinity(threshold) || double.IsNaN(threshold) || 
            Math.Abs(threshold) > 1e50)
        {
            return;
        }

        // Arrange
        var mockDatabase = new Mock<IMetadataDatabase>();
        var emittedEvents = new List<TriggerEvent>();
        
        mockDatabase.Setup(db => db.SaveEvent(It.IsAny<TriggerEvent>()))
            .Callback<TriggerEvent>(e => emittedEvents.Add(e))
            .Returns(Task.CompletedTask);

        var triggerComponent = new TriggerComponent(mockDatabase.Object);
        var config = new TriggerConfig(threshold, Enabled: true);
        triggerComponent.Configure(config);

        // Generate multiple values that exceed the threshold
        var numValues = Math.Min(count.Get % 10 + 1, 10); // 1-10 values
        var values = new List<double>();
        
        for (int i = 0; i < numValues; i++)
        {
            values.Add(threshold + i + 1.0);
        }

        // Act
        var results = new List<TriggerEvent?>();
        foreach (var value in values)
        {
            results.Add(triggerComponent.CheckValue(value).Result);
        }

        // Assert
        Assert.Equal(numValues, results.Count);
        Assert.All(results, r => Assert.NotNull(r));
        Assert.Equal(numValues, emittedEvents.Count);
        
        // Verify each event has correct data
        for (int i = 0; i < numValues; i++)
        {
            Assert.Equal(values[i], results[i]!.Value);
            Assert.Equal(threshold, results[i]!.Threshold);
            Assert.Equal(values[i], emittedEvents[i].Value);
            Assert.Equal(threshold, emittedEvents[i].Threshold);
        }
        
        // Verify SaveEvent was called for each value
        mockDatabase.Verify(db => db.SaveEvent(It.IsAny<TriggerEvent>()), Times.Exactly(numValues));
    }
}
