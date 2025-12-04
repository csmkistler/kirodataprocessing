import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import * as fc from 'fast-check';
import { ParameterPanel } from './ParameterPanel';

/**
 * Feature: signal-processing-viz, Property 6: Parameter validation feedback
 * Feature: signal-processing-viz, Property 8: Invalid parameter prevention
 * 
 * Property 6: For any parameter input (valid or invalid), the Visualization Component
 * should validate the input and provide immediate feedback indicating whether the value is acceptable.
 * 
 * Property 8: For any invalid parameter value, the Visualization Component should display
 * an error message and prevent the invalid value from being applied to the Signal Processor.
 * 
 * Validates: Requirements 4.2, 4.5
 */

// ============================================================================
// Generators
// ============================================================================

/**
 * Generate valid frequency values
 */
const validFrequencyGenerator = () => fc.double({ min: 0.1, max: 20000 });

/**
 * Generate invalid frequency values
 */
const invalidFrequencyGenerator = () => fc.oneof(
  fc.double({ min: -1000, max: 0 }), // Negative or zero
  fc.double({ min: 20001, max: 100000 }) // Too high
);

/**
 * Generate valid amplitude values
 */
const validAmplitudeGenerator = () => fc.double({ min: 0.01, max: 100 });

/**
 * Generate invalid amplitude values
 */
const invalidAmplitudeGenerator = () => fc.oneof(
  fc.double({ min: -100, max: 0 }), // Negative or zero
  fc.double({ min: 101, max: 1000 }) // Too high
);

/**
 * Generate valid phase values
 */
const validPhaseGenerator = () => fc.double({ min: -6.28, max: 6.28 });

/**
 * Generate invalid phase values
 */
const invalidPhaseGenerator = () => fc.oneof(
  fc.double({ min: -20, max: -6.29 }), // Too low
  fc.double({ min: 6.29, max: 20 }) // Too high
);

/**
 * Generate valid duration values
 */
const validDurationGenerator = () => fc.double({ min: 0.1, max: 60 });

/**
 * Generate invalid duration values
 */
const invalidDurationGenerator = () => fc.oneof(
  fc.double({ min: -10, max: 0 }), // Negative or zero
  fc.double({ min: 61, max: 1000 }) // Too high
);

/**
 * Generate valid sample rate values (must be at least 2x frequency)
 */
const validSampleRateGenerator = (frequency: number) => 
  fc.integer({ min: Math.ceil(frequency * 2), max: 192000 });

/**
 * Generate invalid sample rate values (violates Nyquist criterion)
 */
const invalidSampleRateGenerator = (frequency: number) => 
  fc.integer({ min: 1, max: Math.floor(frequency * 2) - 1 });

// ============================================================================
// Property Tests
// ============================================================================

describe('ParameterPanel Property Tests', () => {
  const mockOnGenerateSignal = vi.fn().mockResolvedValue(undefined);

  /**
   * Property 6: Parameter validation feedback
   * 
   * For any valid parameter value, the component should:
   * 1. Accept the input without showing an error
   * 2. Enable the submit button
   */
  it('should provide positive feedback for valid frequency values', () => {
    fc.assert(
      fc.property(validFrequencyGenerator(), (frequency) => {
        const { container } = render(
          <ParameterPanel onGenerateSignal={mockOnGenerateSignal} />
        );

        const frequencyInput = screen.getByLabelText(/Frequency/i) as HTMLInputElement;
        
        // Change the frequency input
        fireEvent.change(frequencyInput, { target: { value: frequency.toString() } });

        // Should not show error message for this field
        const errorMessages = container.querySelectorAll('[style*="color: #d32f2f"]');
        const frequencyErrors = Array.from(errorMessages).filter(el => 
          el.textContent?.includes('Frequency')
        );
        expect(frequencyErrors.length).toBe(0);

        return true;
      }),
      { numRuns: 100 }
    );
  });

  /**
   * Property 6: Parameter validation feedback for invalid values
   * 
   * For any invalid parameter value, the component should:
   * 1. Display an error message
   * 2. Disable the submit button
   */
  it('should provide negative feedback for invalid frequency values', () => {
    fc.assert(
      fc.property(invalidFrequencyGenerator(), (frequency) => {
        const { container } = render(
          <ParameterPanel onGenerateSignal={mockOnGenerateSignal} />
        );

        const frequencyInput = screen.getByLabelText(/Frequency/i) as HTMLInputElement;
        
        // Change the frequency input to invalid value
        fireEvent.change(frequencyInput, { target: { value: frequency.toString() } });

        // Should show error message
        const errorMessages = container.querySelectorAll('[style*="color: #d32f2f"]');
        expect(errorMessages.length).toBeGreaterThan(0);

        // Submit button should be disabled
        const submitButton = screen.getByText(/Generate Signal/i) as HTMLButtonElement;
        expect(submitButton.disabled).toBe(true);

        return true;
      }),
      { numRuns: 100 }
    );
  });

  /**
   * Property 6: Amplitude validation feedback
   */
  it('should validate amplitude values and provide immediate feedback', () => {
    fc.assert(
      fc.property(
        fc.oneof(validAmplitudeGenerator(), invalidAmplitudeGenerator()),
        (amplitude) => {
          const { container } = render(
            <ParameterPanel onGenerateSignal={mockOnGenerateSignal} />
          );

          const amplitudeInput = screen.getByLabelText(/Amplitude/i) as HTMLInputElement;
          fireEvent.change(amplitudeInput, { target: { value: amplitude.toString() } });

          const isValid = amplitude > 0 && amplitude <= 100;
          const errorMessages = container.querySelectorAll('[style*="color: #d32f2f"]');
          const amplitudeErrors = Array.from(errorMessages).filter(el => 
            el.textContent?.includes('Amplitude')
          );

          if (isValid) {
            expect(amplitudeErrors.length).toBe(0);
          } else {
            expect(amplitudeErrors.length).toBeGreaterThan(0);
          }

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * Property 6: Phase validation feedback
   */
  it('should validate phase values and provide immediate feedback', () => {
    fc.assert(
      fc.property(
        fc.oneof(validPhaseGenerator(), invalidPhaseGenerator()),
        (phase) => {
          const { container } = render(
            <ParameterPanel onGenerateSignal={mockOnGenerateSignal} />
          );

          const phaseInput = screen.getByLabelText(/Phase/i) as HTMLInputElement;
          fireEvent.change(phaseInput, { target: { value: phase.toString() } });

          const isValid = phase >= -6.28 && phase <= 6.28;
          const errorMessages = container.querySelectorAll('[style*="color: #d32f2f"]');
          const phaseErrors = Array.from(errorMessages).filter(el => 
            el.textContent?.includes('Phase')
          );

          if (isValid) {
            expect(phaseErrors.length).toBe(0);
          } else {
            expect(phaseErrors.length).toBeGreaterThan(0);
          }

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * Property 6: Duration validation feedback
   */
  it('should validate duration values and provide immediate feedback', () => {
    fc.assert(
      fc.property(
        fc.oneof(validDurationGenerator(), invalidDurationGenerator()),
        (duration) => {
          const { container } = render(
            <ParameterPanel onGenerateSignal={mockOnGenerateSignal} />
          );

          const durationInput = screen.getByLabelText(/Duration/i) as HTMLInputElement;
          fireEvent.change(durationInput, { target: { value: duration.toString() } });

          const isValid = duration > 0 && duration <= 60;
          const errorMessages = container.querySelectorAll('[style*="color: #d32f2f"]');
          const durationErrors = Array.from(errorMessages).filter(el => 
            el.textContent?.includes('Duration')
          );

          if (isValid) {
            expect(durationErrors.length).toBe(0);
          } else {
            expect(durationErrors.length).toBeGreaterThan(0);
          }

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * Property 6: Sample rate validation with Nyquist criterion
   */
  it('should validate sample rate against Nyquist criterion', () => {
    fc.assert(
      fc.property(
        validFrequencyGenerator(),
        (frequency) => {
          const { container } = render(
            <ParameterPanel onGenerateSignal={mockOnGenerateSignal} />
          );

          // Set frequency first
          const frequencyInput = screen.getByLabelText(/Frequency/i) as HTMLInputElement;
          fireEvent.change(frequencyInput, { target: { value: frequency.toString() } });

          // Test valid sample rate (>= 2x frequency)
          const validSampleRate = Math.ceil(frequency * 2);
          const sampleRateInput = screen.getByLabelText(/Sample Rate/i) as HTMLInputElement;
          fireEvent.change(sampleRateInput, { target: { value: validSampleRate.toString() } });

          const errorMessages = container.querySelectorAll('[style*="color: #d32f2f"]');
          const sampleRateErrors = Array.from(errorMessages).filter(el => 
            el.textContent?.includes('Sample rate') || el.textContent?.includes('Nyquist')
          );
          expect(sampleRateErrors.length).toBe(0);

          // Test invalid sample rate (< 2x frequency)
          if (frequency > 1) {
            const invalidSampleRate = Math.floor(frequency * 2) - 1;
            fireEvent.change(sampleRateInput, { target: { value: invalidSampleRate.toString() } });

            const errorMessages2 = container.querySelectorAll('[style*="color: #d32f2f"]');
            const sampleRateErrors2 = Array.from(errorMessages2).filter(el => 
              el.textContent?.includes('Sample rate') || el.textContent?.includes('Nyquist')
            );
            expect(sampleRateErrors2.length).toBeGreaterThan(0);
          }

          return true;
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * Property 8: Invalid parameter prevention
   * 
   * For any invalid parameter combination, the form should:
   * 1. Prevent submission
   * 2. Not call the onGenerateSignal callback
   */
  it('should prevent form submission when parameters are invalid', async () => {
    fc.assert(
      fc.property(invalidFrequencyGenerator(), async (frequency) => {
        const mockCallback = vi.fn().mockResolvedValue(undefined);
        const { container } = render(
          <ParameterPanel onGenerateSignal={mockCallback} />
        );

        const frequencyInput = screen.getByLabelText(/Frequency/i) as HTMLInputElement;
        fireEvent.change(frequencyInput, { target: { value: frequency.toString() } });

        const submitButton = screen.getByText(/Generate Signal/i) as HTMLButtonElement;
        
        // Button should be disabled
        expect(submitButton.disabled).toBe(true);

        // Try to submit anyway
        fireEvent.click(submitButton);

        // Callback should not be called
        await waitFor(() => {
          expect(mockCallback).not.toHaveBeenCalled();
        });

        return true;
      }),
      { numRuns: 50 } // Reduced runs for async test
    );
  });

  /**
   * Property 8: Valid parameters allow submission
   */
  it('should allow form submission when all parameters are valid', async () => {
    fc.assert(
      fc.property(
        validFrequencyGenerator(),
        validAmplitudeGenerator(),
        validPhaseGenerator(),
        validDurationGenerator(),
        async (frequency, amplitude, phase, duration) => {
          const mockCallback = vi.fn().mockResolvedValue(undefined);
          const { container } = render(
            <ParameterPanel onGenerateSignal={mockCallback} />
          );

          // Fill in all fields with valid values
          fireEvent.change(screen.getByLabelText(/Frequency/i), { 
            target: { value: frequency.toString() } 
          });
          fireEvent.change(screen.getByLabelText(/Amplitude/i), { 
            target: { value: amplitude.toString() } 
          });
          fireEvent.change(screen.getByLabelText(/Phase/i), { 
            target: { value: phase.toString() } 
          });
          fireEvent.change(screen.getByLabelText(/Duration/i), { 
            target: { value: duration.toString() } 
          });
          
          const sampleRate = Math.ceil(frequency * 2);
          fireEvent.change(screen.getByLabelText(/Sample Rate/i), { 
            target: { value: sampleRate.toString() } 
          });

          const submitButton = screen.getByText(/Generate Signal/i) as HTMLButtonElement;
          
          // Button should be enabled
          expect(submitButton.disabled).toBe(false);

          // Submit the form
          fireEvent.click(submitButton);

          // Callback should be called
          await waitFor(() => {
            expect(mockCallback).toHaveBeenCalledTimes(1);
          });

          return true;
        }
      ),
      { numRuns: 50 } // Reduced runs for async test
    );
  });
});
