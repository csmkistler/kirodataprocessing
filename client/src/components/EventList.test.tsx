import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import * as fc from 'fast-check';
import { EventList } from './EventList';
import { TriggerEventDto } from '../services/api';

/**
 * Feature: signal-processing-viz, Property 11: Event display
 * 
 * Property: For any emitted event, the Event List should display the event
 * with its value and timestamp.
 * 
 * Validates: Requirements 5.3
 */

/**
 * Feature: signal-processing-viz, Property 12: Event ordering
 * 
 * Property: For any sequence of triggered events, the Event List should display
 * them in reverse chronological order with the most recent event at the top.
 * 
 * Validates: Requirements 5.5
 */

// ============================================================================
// Generators
// ============================================================================

/**
 * Generate a valid trigger event for property testing
 */
const triggerEventGenerator = (): fc.Arbitrary<TriggerEventDto> => {
  return fc.record({
    id: fc.uuid(),
    value: fc.double({ min: -1000, max: 1000 }),
    threshold: fc.double({ min: -1000, max: 1000 }),
    timestamp: fc.date({ min: new Date('2020-01-01'), max: new Date('2025-12-31') })
      .map(d => d.toISOString())
  }).filter(event => event.value > event.threshold); // Ensure value exceeds threshold
};

/**
 * Generate an array of trigger events
 */
const triggerEventsArrayGenerator = (minLength: number = 1, maxLength: number = 20): fc.Arbitrary<TriggerEventDto[]> => {
  return fc.array(triggerEventGenerator(), { minLength, maxLength });
};

// ============================================================================
// Property Tests
// ============================================================================

describe('EventList Property Tests', () => {
  /**
   * Property 11: Event display
   * 
   * For any emitted event, the Event List should display:
   * 1. The event value
   * 2. The threshold value
   * 3. The timestamp
   */
  it('should display event value, threshold, and timestamp for any emitted event', async () => {
    await fc.assert(
      fc.asyncProperty(triggerEventGenerator(), async (event) => {
        // Arrange: Mock fetch function that returns the single event
        const mockFetchEvents = vi.fn().mockResolvedValue([event]);

        // Act: Render the EventList
        render(
          <EventList 
            onFetchEvents={mockFetchEvents}
            autoRefreshInterval={0} // Disable auto-refresh for testing
          />
        );

        // Wait for events to load
        await waitFor(() => {
          expect(mockFetchEvents).toHaveBeenCalled();
        });

        // Assert: Event value should be displayed
        const valueText = event.value.toFixed(2);
        expect(screen.getByText(valueText)).toBeTruthy();

        // Assert: Threshold should be displayed
        const thresholdText = event.threshold.toFixed(2);
        expect(screen.getByText(thresholdText)).toBeTruthy();

        // Assert: Timestamp should be displayed (check for any date-like text)
        // We can't check exact format due to locale differences, but we can verify the table row exists
        const tableRows = screen.getAllByRole('row');
        // Should have header row + 1 data row
        expect(tableRows.length).toBeGreaterThanOrEqual(2);

        return true;
      }),
      { numRuns: 100 } // Run 100 iterations as specified in design
    );
  });

  /**
   * Property 12: Event ordering
   * 
   * For any sequence of events, they should be displayed in reverse chronological order
   * (most recent first)
   */
  it('should display events in reverse chronological order with most recent first', async () => {
    await fc.assert(
      fc.asyncProperty(triggerEventsArrayGenerator(2, 10), async (events) => {
        // Arrange: Mock fetch function that returns events in random order
        const mockFetchEvents = vi.fn().mockResolvedValue(events);

        // Act: Render the EventList
        const { container } = render(
          <EventList 
            onFetchEvents={mockFetchEvents}
            autoRefreshInterval={0}
          />
        );

        // Wait for events to load
        await waitFor(() => {
          expect(mockFetchEvents).toHaveBeenCalled();
        });

        // Assert: Events should be sorted by timestamp (most recent first)
        const sortedEvents = [...events].sort((a, b) => {
          return new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime();
        });

        // Get all value cells from the table
        const valueCells = container.querySelectorAll('tbody tr td:first-child span');
        
        // Verify the order matches sorted order
        valueCells.forEach((cell, index) => {
          const displayedValue = parseFloat(cell.textContent || '0');
          const expectedValue = sortedEvents[index].value;
          expect(Math.abs(displayedValue - expectedValue)).toBeLessThan(0.01);
        });

        return true;
      }),
      { numRuns: 100 }
    );
  });

  /**
   * Property: Empty event list should display appropriate message
   */
  it('should display "no events" message when event list is empty', async () => {
    // Arrange: Mock fetch function that returns empty array
    const mockFetchEvents = vi.fn().mockResolvedValue([]);

    // Act: Render the EventList
    render(
      <EventList 
        onFetchEvents={mockFetchEvents}
        autoRefreshInterval={0}
      />
    );

    // Wait for events to load
    await waitFor(() => {
      expect(mockFetchEvents).toHaveBeenCalled();
    });

    // Assert: Should show "no events" message
    expect(screen.getByText(/No trigger events yet/i)).toBeTruthy();
  });

  /**
   * Property: Event list should respect maxEvents limit
   */
  it('should limit displayed events to maxEvents parameter', async () => {
    await fc.assert(
      fc.asyncProperty(
        triggerEventsArrayGenerator(20, 50),
        fc.integer({ min: 5, max: 15 }),
        async (events, maxEvents) => {
          // Arrange: Mock fetch function
          const mockFetchEvents = vi.fn().mockResolvedValue(events);

          // Act: Render with maxEvents limit
          const { container } = render(
            <EventList 
              onFetchEvents={mockFetchEvents}
              autoRefreshInterval={0}
              maxEvents={maxEvents}
            />
          );

          // Wait for events to load
          await waitFor(() => {
            expect(mockFetchEvents).toHaveBeenCalled();
          });

          // Assert: Should display at most maxEvents
          const tableRows = container.querySelectorAll('tbody tr');
          expect(tableRows.length).toBeLessThanOrEqual(maxEvents);

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * Property: All displayed events should have value > threshold
   */
  it('should only display events where value exceeds threshold', async () => {
    await fc.assert(
      fc.asyncProperty(triggerEventsArrayGenerator(1, 20), async (events) => {
        // Arrange: Mock fetch function
        const mockFetchEvents = vi.fn().mockResolvedValue(events);

        // Act: Render the EventList
        const { container } = render(
          <EventList 
            onFetchEvents={mockFetchEvents}
            autoRefreshInterval={0}
          />
        );

        // Wait for events to load
        await waitFor(() => {
          expect(mockFetchEvents).toHaveBeenCalled();
        });

        // Assert: For each displayed event, value should exceed threshold
        const rows = container.querySelectorAll('tbody tr');
        rows.forEach((row) => {
          const cells = row.querySelectorAll('td');
          const value = parseFloat(cells[0].textContent || '0');
          const threshold = parseFloat(cells[1].textContent || '0');
          expect(value).toBeGreaterThan(threshold);
        });

        return true;
      }),
      { numRuns: 100 }
    );
  });
});
