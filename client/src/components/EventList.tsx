import React, { useState, useEffect, useCallback } from 'react';
import { TriggerEventDto } from '../services/api';

// ============================================================================
// Types
// ============================================================================

interface EventListProps {
  onFetchEvents: () => Promise<TriggerEventDto[]>;
  autoRefreshInterval?: number; // milliseconds, default 5000
  maxEvents?: number; // maximum events to display, default 50
}

// ============================================================================
// EventList Component
// ============================================================================

export const EventList: React.FC<EventListProps> = ({
  onFetchEvents,
  autoRefreshInterval = 5000,
  maxEvents = 50
}) => {
  const [events, setEvents] = useState<TriggerEventDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());

  // Fetch events from API
  const fetchEvents = useCallback(async () => {
    setIsLoading(true);
    setError('');
    
    try {
      const fetchedEvents = await onFetchEvents();
      
      // Sort events in reverse chronological order (most recent first)
      const sortedEvents = [...fetchedEvents].sort((a, b) => {
        return new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime();
      });
      
      // Limit to maxEvents
      const limitedEvents = sortedEvents.slice(0, maxEvents);
      
      setEvents(limitedEvents);
      setLastRefresh(new Date());
    } catch (err) {
      setError('Failed to fetch events');
      console.error('Error fetching events:', err);
    } finally {
      setIsLoading(false);
    }
  }, [onFetchEvents, maxEvents]);

  // Initial fetch
  useEffect(() => {
    fetchEvents();
  }, [fetchEvents]);

  // Auto-refresh
  useEffect(() => {
    if (autoRefreshInterval <= 0) return;

    const intervalId = setInterval(() => {
      fetchEvents();
    }, autoRefreshInterval);

    return () => clearInterval(intervalId);
  }, [fetchEvents, autoRefreshInterval]);

  // Format timestamp for display
  const formatTimestamp = (timestamp: string): string => {
    const date = new Date(timestamp);
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
    });
  };

  // Format relative time (e.g., "2 minutes ago")
  const formatRelativeTime = (timestamp: string): string => {
    const now = new Date();
    const eventTime = new Date(timestamp);
    const diffMs = now.getTime() - eventTime.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHour = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHour / 24);

    if (diffSec < 60) return `${diffSec} second${diffSec !== 1 ? 's' : ''} ago`;
    if (diffMin < 60) return `${diffMin} minute${diffMin !== 1 ? 's' : ''} ago`;
    if (diffHour < 24) return `${diffHour} hour${diffHour !== 1 ? 's' : ''} ago`;
    return `${diffDay} day${diffDay !== 1 ? 's' : ''} ago`;
  };

  return (
    <div style={{ padding: '20px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: '#fff' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <h3 style={{ margin: 0 }}>Trigger Events</h3>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <span style={{ fontSize: '12px', color: '#666' }}>
            Last updated: {lastRefresh.toLocaleTimeString()}
          </span>
          <button
            onClick={fetchEvents}
            disabled={isLoading}
            style={{
              padding: '6px 12px',
              backgroundColor: isLoading ? '#ccc' : '#1976d2',
              color: '#fff',
              border: 'none',
              borderRadius: '4px',
              fontSize: '12px',
              cursor: isLoading ? 'not-allowed' : 'pointer'
            }}
          >
            {isLoading ? 'Refreshing...' : 'Refresh'}
          </button>
        </div>
      </div>

      {error && (
        <div
          style={{
            padding: '12px',
            marginBottom: '16px',
            backgroundColor: '#ffebee',
            color: '#c62828',
            borderRadius: '4px',
            border: '1px solid #ef5350'
          }}
        >
          {error}
        </div>
      )}

      {events.length === 0 && !isLoading && !error && (
        <div
          style={{
            padding: '40px',
            textAlign: 'center',
            color: '#999',
            backgroundColor: '#f5f5f5',
            borderRadius: '4px'
          }}
        >
          <p style={{ margin: 0, fontSize: '16px' }}>No trigger events yet</p>
          <p style={{ margin: '8px 0 0 0', fontSize: '14px' }}>
            Events will appear here when values exceed the configured threshold
          </p>
        </div>
      )}

      {events.length > 0 && (
        <div style={{ maxHeight: '500px', overflowY: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ backgroundColor: '#f5f5f5', borderBottom: '2px solid #ddd' }}>
                <th style={{ padding: '12px', textAlign: 'left', fontWeight: 'bold' }}>Value</th>
                <th style={{ padding: '12px', textAlign: 'left', fontWeight: 'bold' }}>Threshold</th>
                <th style={{ padding: '12px', textAlign: 'left', fontWeight: 'bold' }}>Timestamp</th>
                <th style={{ padding: '12px', textAlign: 'left', fontWeight: 'bold' }}>Time Ago</th>
              </tr>
            </thead>
            <tbody>
              {events.map((event, index) => (
                <tr
                  key={event.id}
                  style={{
                    borderBottom: '1px solid #eee',
                    backgroundColor: index % 2 === 0 ? '#fff' : '#fafafa'
                  }}
                >
                  <td style={{ padding: '12px' }}>
                    <span
                      style={{
                        fontWeight: 'bold',
                        color: '#d32f2f',
                        fontSize: '16px'
                      }}
                    >
                      {event.value.toFixed(2)}
                    </span>
                  </td>
                  <td style={{ padding: '12px' }}>
                    <span style={{ color: '#666' }}>
                      {event.threshold.toFixed(2)}
                    </span>
                  </td>
                  <td style={{ padding: '12px', fontSize: '14px', color: '#666' }}>
                    {formatTimestamp(event.timestamp)}
                  </td>
                  <td style={{ padding: '12px', fontSize: '14px', color: '#999', fontStyle: 'italic' }}>
                    {formatRelativeTime(event.timestamp)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {events.length > 0 && (
        <div
          style={{
            marginTop: '12px',
            padding: '8px',
            backgroundColor: '#f5f5f5',
            borderRadius: '4px',
            fontSize: '12px',
            color: '#666',
            textAlign: 'center'
          }}
        >
          Showing {events.length} event{events.length !== 1 ? 's' : ''} 
          {events.length >= maxEvents && ` (limited to ${maxEvents} most recent)`}
          {autoRefreshInterval > 0 && ` • Auto-refreshing every ${autoRefreshInterval / 1000}s`}
        </div>
      )}
    </div>
  );
};

export default EventList;
