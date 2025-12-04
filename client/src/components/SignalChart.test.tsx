import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import * as fc from 'fast-check';
import { SignalChart, SignalData } from './SignalChart';

/**
 * Feature: signal-processing-viz, Property 5: Chart renders signal data
 * 
 * Property: For any valid Signal Data, the Visualization Component should render
 * a chart containing all sample points with time on the x-axis and amplitude on the y-axis.
 * 
 * Validates: Requirements 3.1
 */

// ============================================================================
// Generators
// ============================================================================

/**
 * Generate valid signal data for property testing
 */
const signalDataGenerator = (): fc.Arbitrary<SignalData> => {
  return fc.record({
    id: fc.uuid(),
    type: fc.constantFrom('Sine', 'Square', 'Sawtooth', 'Noise'),
    samples: fc.array(fc.double({ min: -10, max: 10 }), { minLength: 10, maxLength: 1000 }),
    timestamps: fc.array(fc.double({ min: 0, max: 10 }), { minLength: 10, maxLength: 1000 }),
    metadata: fc.record({
      frequency: fc.double({ min: 0.1, max: 20000 }),
      amplitude: fc.double({ min: 0.01, max: 100 }),
      duration: fc.double({ min: 0.1, max: 60 }),
      sampleRate: fc.integer({ min: 1, max: 192000 })
    })
  }).map(data => {
    // Ensure timestamps and samples have the same length
    const length = Math.min(data.samples.length, data.timestamps.length);
    return {
      ...data,
      samples: data.samples.slice(0, length),
      timestamps: data.timestamps.slice(0, length).sort((a, b) => a - b) // Ensure timestamps are sorted
    };
  });
};

// ============================================================================
// Property Tests
// ============================================================================

describe('SignalChart Property Tests', () => {
  /**
   * Property 5: Chart renders signal data
   * 
   * For any valid signal data, the chart should:
   * 1. Render without errors
   * 2. Display the chart container
   * 3. Include axis labels for time and amplitude
   */
  it('should render chart with time and amplitude axes for any valid signal data', () => {
    fc.assert(
      fc.property(signalDataGenerator(), (signalData) => {
        // Act: Render the chart with generated signal data
        const { container } = render(
          <SignalChart signalData={signalData} processedData={null} />
        );

        // Assert: Chart should render
        expect(container.querySelector('.recharts-wrapper')).toBeTruthy();
        
        // Assert: Should have data points rendered
        const lines = container.querySelectorAll('.recharts-line');
        expect(lines.length).toBeGreaterThan(0);

        // Assert: Should have axes
        const xAxis = container.querySelector('.recharts-xAxis');
        const yAxis = container.querySelector('.recharts-yAxis');
        expect(xAxis).toBeTruthy();
        expect(yAxis).toBeTruthy();

        return true;
      }),
      { numRuns: 100 } // Run 100 iterations as specified in design
    );
  });

  /**
   * Property: Chart should handle empty signal gracefully
   */
  it('should display "no data" message when signal data is null', () => {
    const { container } = render(
      <SignalChart signalData={null} processedData={null} />
    );

    // Should show the "no data" message
    expect(screen.getByText(/No signal data to display/i)).toBeTruthy();
    
    // Should not render chart
    expect(container.querySelector('.recharts-wrapper')).toBeFalsy();
  });

  /**
   * Property: Chart should render both original and processed signals when both are provided
   */
  it('should render both original and processed signals when both are provided', () => {
    fc.assert(
      fc.property(
        signalDataGenerator(),
        signalDataGenerator(),
        (originalSignal, processedSignal) => {
          // Make processed signal reference the original
          const processedData = {
            ...processedSignal,
            originalSignalId: originalSignal.id,
            processingParams: {
              operation: 'LowPass',
              cutoffFrequency: 1000
            }
          };

          const { container } = render(
            <SignalChart signalData={originalSignal} processedData={processedData} />
          );

          // Should render two lines (original and processed)
          const lines = container.querySelectorAll('.recharts-line');
          expect(lines.length).toBe(2);

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * Property: Chart should downsample large datasets
   */
  it('should indicate downsampling for datasets larger than 10,000 points', () => {
    // Generate a large signal
    const largeSignal: SignalData = {
      id: 'test-id',
      type: 'Sine',
      samples: Array.from({ length: 15000 }, (_, i) => Math.sin(i * 0.01)),
      timestamps: Array.from({ length: 15000 }, (_, i) => i * 0.001),
      metadata: {
        frequency: 1000,
        amplitude: 1,
        duration: 15,
        sampleRate: 1000
      }
    };

    const { container } = render(
      <SignalChart signalData={largeSignal} processedData={null} />
    );

    // Should show downsampling note
    expect(screen.getByText(/downsampled for performance/i)).toBeTruthy();
  });
});
