import React, { useState } from 'react';
import { SignalGenerationRequest } from '../services/api';

// ============================================================================
// Types
// ============================================================================

interface ParameterPanelProps {
  onGenerateSignal: (params: SignalGenerationRequest) => Promise<void>;
  isLoading?: boolean;
}

interface ValidationErrors {
  frequency?: string;
  amplitude?: string;
  phase?: string;
  duration?: string;
  sampleRate?: string;
}

// ============================================================================
// Validation Functions
// ============================================================================

function validateFrequency(value: number): string | undefined {
  if (value <= 0) return 'Frequency must be positive';
  if (value > 20000) return 'Frequency must be ≤ 20,000 Hz';
  return undefined;
}

function validateAmplitude(value: number): string | undefined {
  if (value <= 0) return 'Amplitude must be positive';
  if (value > 100) return 'Amplitude must be ≤ 100';
  return undefined;
}

function validatePhase(value: number): string | undefined {
  if (value < -6.28) return 'Phase must be ≥ -2π (-6.28)';
  if (value > 6.28) return 'Phase must be ≤ 2π (6.28)';
  return undefined;
}

function validateDuration(value: number): string | undefined {
  if (value <= 0) return 'Duration must be positive';
  if (value > 60) return 'Duration must be ≤ 60 seconds';
  return undefined;
}

function validateSampleRate(value: number, frequency: number): string | undefined {
  if (value < 1) return 'Sample rate must be ≥ 1';
  if (value < frequency * 2) {
    return `Sample rate must be at least ${(frequency * 2).toFixed(0)} Hz (2× frequency) to satisfy Nyquist criterion`;
  }
  return undefined;
}

// ============================================================================
// ParameterPanel Component
// ============================================================================

export const ParameterPanel: React.FC<ParameterPanelProps> = ({
  onGenerateSignal,
  isLoading = false
}) => {
  // Form state
  const [signalType, setSignalType] = useState<'Sine' | 'Square' | 'Sawtooth' | 'Noise'>('Sine');
  const [frequency, setFrequency] = useState<string>('1000');
  const [amplitude, setAmplitude] = useState<string>('1');
  const [phase, setPhase] = useState<string>('0');
  const [duration, setDuration] = useState<string>('1');
  const [sampleRate, setSampleRate] = useState<string>('44100');

  // Validation errors
  const [errors, setErrors] = useState<ValidationErrors>({});

  // Real-time validation
  const validateField = (field: keyof ValidationErrors, value: string) => {
    const numValue = parseFloat(value);
    if (isNaN(numValue)) {
      setErrors(prev => ({ ...prev, [field]: 'Must be a valid number' }));
      return;
    }

    let error: string | undefined;
    switch (field) {
      case 'frequency':
        error = validateFrequency(numValue);
        break;
      case 'amplitude':
        error = validateAmplitude(numValue);
        break;
      case 'phase':
        error = validatePhase(numValue);
        break;
      case 'duration':
        error = validateDuration(numValue);
        break;
      case 'sampleRate':
        error = validateSampleRate(numValue, parseFloat(frequency));
        break;
    }

    setErrors(prev => {
      const newErrors = { ...prev };
      if (error) {
        newErrors[field] = error;
      } else {
        delete newErrors[field];
      }
      return newErrors;
    });
  };

  // Handle input changes with validation
  const handleFrequencyChange = (value: string) => {
    setFrequency(value);
    validateField('frequency', value);
    // Re-validate sample rate when frequency changes
    if (sampleRate) {
      validateField('sampleRate', sampleRate);
    }
  };

  const handleAmplitudeChange = (value: string) => {
    setAmplitude(value);
    validateField('amplitude', value);
  };

  const handlePhaseChange = (value: string) => {
    setPhase(value);
    validateField('phase', value);
  };

  const handleDurationChange = (value: string) => {
    setDuration(value);
    validateField('duration', value);
  };

  const handleSampleRateChange = (value: string) => {
    setSampleRate(value);
    validateField('sampleRate', value);
  };

  // Validate all fields
  const validateAll = (): boolean => {
    const newErrors: ValidationErrors = {};
    
    const freqNum = parseFloat(frequency);
    const ampNum = parseFloat(amplitude);
    const phaseNum = parseFloat(phase);
    const durNum = parseFloat(duration);
    const srNum = parseFloat(sampleRate);

    if (isNaN(freqNum)) newErrors.frequency = 'Must be a valid number';
    else {
      const err = validateFrequency(freqNum);
      if (err) newErrors.frequency = err;
    }

    if (isNaN(ampNum)) newErrors.amplitude = 'Must be a valid number';
    else {
      const err = validateAmplitude(ampNum);
      if (err) newErrors.amplitude = err;
    }

    if (isNaN(phaseNum)) newErrors.phase = 'Must be a valid number';
    else {
      const err = validatePhase(phaseNum);
      if (err) newErrors.phase = err;
    }

    if (isNaN(durNum)) newErrors.duration = 'Must be a valid number';
    else {
      const err = validateDuration(durNum);
      if (err) newErrors.duration = err;
    }

    if (isNaN(srNum)) newErrors.sampleRate = 'Must be a valid number';
    else {
      const err = validateSampleRate(srNum, freqNum);
      if (err) newErrors.sampleRate = err;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateAll()) {
      return;
    }

    const params: SignalGenerationRequest = {
      type: signalType,
      frequency: parseFloat(frequency),
      amplitude: parseFloat(amplitude),
      phase: parseFloat(phase),
      duration: parseFloat(duration),
      sampleRate: parseInt(sampleRate, 10)
    };

    await onGenerateSignal(params);
  };

  const hasErrors = Object.keys(errors).length > 0;

  return (
    <div style={{ padding: '20px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: '#fff' }}>
      <h3 style={{ marginTop: 0, marginBottom: '20px' }}>Signal Generation Parameters</h3>
      
      <form onSubmit={handleSubmit}>
        {/* Signal Type */}
        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Signal Type
          </label>
          <select
            value={signalType}
            onChange={(e) => setSignalType(e.target.value as any)}
            disabled={isLoading}
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: '1px solid #ccc'
            }}
          >
            <option value="Sine">Sine Wave</option>
            <option value="Square">Square Wave</option>
            <option value="Sawtooth">Sawtooth Wave</option>
            <option value="Noise">White Noise</option>
          </select>
        </div>

        {/* Frequency */}
        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Frequency (Hz)
          </label>
          <input
            type="number"
            value={frequency}
            onChange={(e) => handleFrequencyChange(e.target.value)}
            disabled={isLoading}
            step="0.1"
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: errors.frequency ? '1px solid #d32f2f' : '1px solid #ccc'
            }}
          />
          {errors.frequency && (
            <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {errors.frequency}
            </span>
          )}
        </div>

        {/* Amplitude */}
        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Amplitude
          </label>
          <input
            type="number"
            value={amplitude}
            onChange={(e) => handleAmplitudeChange(e.target.value)}
            disabled={isLoading}
            step="0.01"
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: errors.amplitude ? '1px solid #d32f2f' : '1px solid #ccc'
            }}
          />
          {errors.amplitude && (
            <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {errors.amplitude}
            </span>
          )}
        </div>

        {/* Phase */}
        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Phase (radians)
          </label>
          <input
            type="number"
            value={phase}
            onChange={(e) => handlePhaseChange(e.target.value)}
            disabled={isLoading}
            step="0.1"
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: errors.phase ? '1px solid #d32f2f' : '1px solid #ccc'
            }}
          />
          {errors.phase && (
            <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {errors.phase}
            </span>
          )}
        </div>

        {/* Duration */}
        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Duration (seconds)
          </label>
          <input
            type="number"
            value={duration}
            onChange={(e) => handleDurationChange(e.target.value)}
            disabled={isLoading}
            step="0.1"
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: errors.duration ? '1px solid #d32f2f' : '1px solid #ccc'
            }}
          />
          {errors.duration && (
            <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {errors.duration}
            </span>
          )}
        </div>

        {/* Sample Rate */}
        <div style={{ marginBottom: '20px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Sample Rate (Hz)
          </label>
          <input
            type="number"
            value={sampleRate}
            onChange={(e) => handleSampleRateChange(e.target.value)}
            disabled={isLoading}
            step="1"
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: errors.sampleRate ? '1px solid #d32f2f' : '1px solid #ccc'
            }}
          />
          {errors.sampleRate && (
            <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {errors.sampleRate}
            </span>
          )}
        </div>

        {/* Submit Button */}
        <button
          type="submit"
          disabled={isLoading || hasErrors}
          style={{
            width: '100%',
            padding: '12px',
            backgroundColor: isLoading || hasErrors ? '#ccc' : '#1976d2',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            fontSize: '16px',
            fontWeight: 'bold',
            cursor: isLoading || hasErrors ? 'not-allowed' : 'pointer'
          }}
        >
          {isLoading ? 'Generating...' : 'Generate Signal'}
        </button>
      </form>
    </div>
  );
};

export default ParameterPanel;
