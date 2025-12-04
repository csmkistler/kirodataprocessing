import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import * as fc from 'fast-check';
import { ProcessingPanel } from './ProcessingPanel';
import { ProcessingRequest } from '../services/api';

/**
 * Feature: signal-processing-viz, Property 7: Parameter application triggers processing
 * 
 * Property: For any valid processing parameters applied through the UI, the Visualization
 * Component should invoke the Signal Processor with exactly those parameters.
 * 
 * Validates: Requirements 4.3
 */

// ============================================================================
// Generators
// ============================================================================

/**
 * Generate valid cutoff frequency values
 */
const validCutoffFrequencyGenerator = () => fc.double({ min: 1, max: 22000 });

/**
 * Generate valid gain values
 */
const validGainGenerator = () => fc.double({ min: 0.01, max: 100 });

/**
 * Generate valid filter order
 */
const validFilterOrderGenerator = () => fc.integer({ min: 1, max: 10 });

/**
 * Generate valid band-pass frequencies
 */
const validBandPassGenerator = () => fc.record({
  lowCutoff: fc.double({ min: 1, max: 10000 }),
  highCutoff: fc.double({ min: 10001, max: 22000 })
});

// ============================================================================
// Property Tests
// ============================================================================

describe('ProcessingPanel Property Tests', () => {
  const mockSignalId = 'test-signal-id-123';

  /**
   * Property 7: Parameter application triggers processing with exact parameters
   * 
   * For Low-Pass filter operation
   */
  it('should invoke onProcessSignal with exact low-pass filter parameters', async () => {
    fc.assert(
      fc.property(
        validCutoffFrequencyGenerator(),
        validFilterOrderGenerator(),
        async (cutoffFrequency, order) => {
          const mockCallback = vi.fn().mockResolvedValue(undefined);
          
          render(
            <ProcessingPanel 
              currentSignalId={mockSignalId}
              onProcessSignal={mockCallback}
            />
          );

          // Select Low-Pass operation (should be default)
          const operationSelect = screen.getByLabelText(/Operation Type/i) as HTMLSelectElement;
          fireEvent.change(operationSelect, { target: { value: 'LowPass' } });

          // Set cutoff frequency
          const cutoffInput = screen.getByLabelText(/Cutoff Frequency/i) as HTMLInputElement;
          fireEvent.change(cutoffInput, { target: { value: cutoffFrequency.toString() } });

          // Set filter order
          const orderInput = screen.getByLabelText(/Filter Order/i) as HTMLInputElement;
          fireEvent.change(orderInput, { target: { value: order.toString() } });

          // Submit the form
          const submitButton = screen.getByText(/Apply Processing/i);
          fireEvent.click(submitButton);

          // Verify callback was called with exact parameters
          await waitFor(() => {
            expect(mockCallback).toHaveBeenCalledTimes(1);
            const callArgs = mockCallback.mock.calls[0][0] as ProcessingRequest;
            
            expect(callArgs.signalId).toBe(mockSignalId);
            expect(callArgs.operation).toBe('LowPass');
            expect(callArgs.cutoffFrequency).toBeCloseTo(cutoffFrequency, 2);
            expect(callArgs.order).toBe(order);
          });

          return true;
        }
      ),
      { numRuns: 50 } // Reduced for async tests
    );
  });

  /**
   * Property 7: Parameter application for High-Pass filter
   */
  it('should invoke onProcessSignal with exact high-pass filter parameters', async () => {
    fc.assert(
      fc.property(
        validCutoffFrequencyGenerator(),
        validFilterOrderGenerator(),
        async (cutoffFrequency, order) => {
          const mockCallback = vi.fn().mockResolvedValue(undefined);
          
          render(
            <ProcessingPanel 
              currentSignalId={mockSignalId}
              onProcessSignal={mockCallback}
            />
          );

          // Select High-Pass operation
          const operationSelect = screen.getByLabelText(/Operation Type/i) as HTMLSelectElement;
          fireEvent.change(operationSelect, { target: { value: 'HighPass' } });

          // Set cutoff frequency
          const cutoffInput = screen.getByLabelText(/Cutoff Frequency/i) as HTMLInputElement;
          fireEvent.change(cutoffInput, { target: { value: cutoffFrequency.toString() } });

          // Set filter order
          const orderInput = screen.getByLabelText(/Filter Order/i) as HTMLInputElement;
          fireEvent.change(orderInput, { target: { value: order.toString() } });

          // Submit the form
          const submitButton = screen.getByText(/Apply Processing/i);
          fireEvent.click(submitButton);

          // Verify callback was called with exact parameters
          await waitFor(() => {
            expect(mockCallback).toHaveBeenCalledTimes(1);
            const callArgs = mockCallback.mock.calls[0][0] as ProcessingRequest;
            
            expect(callArgs.signalId).toBe(mockSignalId);
            expect(callArgs.operation).toBe('HighPass');
            expect(callArgs.cutoffFrequency).toBeCloseTo(cutoffFrequency, 2);
            expect(callArgs.order).toBe(order);
          });

          return true;
        }
      ),
      { numRuns: 50 }
    );
  });

  /**
   * Property 7: Parameter application for Band-Pass filter
   */
  it('should invoke onProcessSignal with exact band-pass filter parameters', async () => {
    fc.assert(
      fc.property(
        validBandPassGenerator(),
        validFilterOrderGenerator(),
        async (frequencies, order) => {
          const mockCallback = vi.fn().mockResolvedValue(undefined);
          
          render(
            <ProcessingPanel 
              currentSignalId={mockSignalId}
              onProcessSignal={mockCallback}
            />
          );

          // Select Band-Pass operation
          const operationSelect = screen.getByLabelText(/Operation Type/i) as HTMLSelectElement;
          fireEvent.change(operationSelect, { target: { value: 'BandPass' } });

          // Set low cutoff frequency
          const lowCutoffInput = screen.getByLabelText(/Low Cutoff Frequency/i) as HTMLInputElement;
          fireEvent.change(lowCutoffInput, { target: { value: frequencies.lowCutoff.toString() } });

          // Set high cutoff frequency
          const highCutoffInput = screen.getByLabelText(/High Cutoff Frequency/i) as HTMLInputElement;
          fireEvent.change(highCutoffInput, { target: { value: frequencies.highCutoff.toString() } });

          // Set filter order
          const orderInput = screen.getByLabelText(/Filter Order/i) as HTMLInputElement;
          fireEvent.change(orderInput, { target: { value: order.toString() } });

          // Submit the form
          const submitButton = screen.getByText(/Apply Processing/i);
          fireEvent.click(submitButton);

          // Verify callback was called with exact parameters
          await waitFor(() => {
            expect(mockCallback).toHaveBeenCalledTimes(1);
            const callArgs = mockCallback.mock.calls[0][0] as ProcessingRequest;
            
            expect(callArgs.signalId).toBe(mockSignalId);
            expect(callArgs.operation).toBe('BandPass');
            expect(callArgs.lowCutoff).toBeCloseTo(frequencies.lowCutoff, 2);
            expect(callArgs.highCutoff).toBeCloseTo(frequencies.highCutoff, 2);
            expect(callArgs.order).toBe(order);
          });

          return true;
        }
      ),
      { numRuns: 50 }
    );
  });

  /**
   * Property 7: Parameter application for Gain adjustment
   */
  it('should invoke onProcessSignal with exact gain parameters', async () => {
    fc.assert(
      fc.property(
        validGainGenerator(),
        async (gain) => {
          const mockCallback = vi.fn().mockResolvedValue(undefined);
          
          render(
            <ProcessingPanel 
              currentSignalId={mockSignalId}
              onProcessSignal={mockCallback}
            />
          );

          // Select Gain operation
          const operationSelect = screen.getByLabelText(/Operation Type/i) as HTMLSelectElement;
          fireEvent.change(operationSelect, { target: { value: 'Gain' } });

          // Set gain value
          const gainInput = screen.getByLabelText(/Gain Multiplier/i) as HTMLInputElement;
          fireEvent.change(gainInput, { target: { value: gain.toString() } });

          // Submit the form
          const submitButton = screen.getByText(/Apply Processing/i);
          fireEvent.click(submitButton);

          // Verify callback was called with exact parameters
          await waitFor(() => {
            expect(mockCallback).toHaveBeenCalledTimes(1);
            const callArgs = mockCallback.mock.calls[0][0] as ProcessingRequest;
            
            expect(callArgs.signalId).toBe(mockSignalId);
            expect(callArgs.operation).toBe('Gain');
            expect(callArgs.gain).toBeCloseTo(gain, 2);
          });

          return true;
        }
      ),
      { numRuns: 50 }
    );
  });

  /**
   * Property: Should not process when no signal is selected
   */
  it('should prevent processing when currentSignalId is null', () => {
    const mockCallback = vi.fn().mockResolvedValue(undefined);
    
    render(
      <ProcessingPanel 
        currentSignalId={null}
        onProcessSignal={mockCallback}
      />
    );

    // Should show warning message
    expect(screen.getByText(/Please generate a signal first/i)).toBeTruthy();

    // Submit button should be disabled
    const submitButton = screen.getByText(/Apply Processing/i) as HTMLButtonElement;
    expect(submitButton.disabled).toBe(true);

    // Try to click anyway
    fireEvent.click(submitButton);

    // Callback should not be called
    expect(mockCallback).not.toHaveBeenCalled();
  });

  /**
   * Property: Should validate parameters before processing
   */
  it('should prevent processing with invalid cutoff frequency', () => {
    fc.assert(
      fc.property(
        fc.oneof(
          fc.double({ min: -1000, max: 0 }), // Negative
          fc.double({ min: 22050, max: 100000 }) // Above Nyquist
        ),
        (invalidCutoff) => {
          const mockCallback = vi.fn().mockResolvedValue(undefined);
          
          render(
            <ProcessingPanel 
              currentSignalId={mockSignalId}
              onProcessSignal={mockCallback}
            />
          );

          // Set invalid cutoff frequency
          const cutoffInput = screen.getByLabelText(/Cutoff Frequency/i) as HTMLInputElement;
          fireEvent.change(cutoffInput, { target: { value: invalidCutoff.toString() } });

          // Submit button should be disabled
          const submitButton = screen.getByText(/Apply Processing/i) as HTMLButtonElement;
          expect(submitButton.disabled).toBe(true);

          // Callback should not be called
          expect(mockCallback).not.toHaveBeenCalled();

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * Property: Should validate gain values
   */
  it('should prevent processing with invalid gain values', () => {
    fc.assert(
      fc.property(
        fc.oneof(
          fc.double({ min: -100, max: 0 }), // Negative or zero
          fc.double({ min: 101, max: 1000 }) // Too high
        ),
        (invalidGain) => {
          const mockCallback = vi.fn().mockResolvedValue(undefined);
          
          render(
            <ProcessingPanel 
              currentSignalId={mockSignalId}
              onProcessSignal={mockCallback}
            />
          );

          // Select Gain operation
          const operationSelect = screen.getByLabelText(/Operation Type/i) as HTMLSelectElement;
          fireEvent.change(operationSelect, { target: { value: 'Gain' } });

          // Set invalid gain
          const gainInput = screen.getByLabelText(/Gain Multiplier/i) as HTMLInputElement;
          fireEvent.change(gainInput, { target: { value: invalidGain.toString() } });

          // Submit button should be disabled
          const submitButton = screen.getByText(/Apply Processing/i) as HTMLButtonElement;
          expect(submitButton.disabled).toBe(true);

          // Callback should not be called
          expect(mockCallback).not.toHaveBeenCalled();

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });
});
