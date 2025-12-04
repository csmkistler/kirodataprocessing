import React, { useState } from 'react';
import { ProcessingRequest } from '../services/api';

// ============================================================================
// Types
// ============================================================================

interface ProcessingPanelProps {
  currentSignalId: string | null;
  onProcessSignal: (params: ProcessingRequest) => Promise<void>;
  isLoading?: boolean;
}

interface ValidationErrors {
  cutoffFrequency?: string;
  lowCutoff?: string;
  highCutoff?: string;
  gain?: string;
  order?: string;
}

type OperationType = 'LowPass' | 'HighPass' | 'BandPass' | 'Gain';

// ============================================================================
// Validation Functions
// ============================================================================

function validateCutoffFrequency(value: number, nyquistFreq: number = 22050): string | undefined {
  if (value <= 0) return 'Cutoff frequency must be positive';
  if (value >= nyquistFreq) return `Cutoff frequency must be below Nyquist frequency (${nyquistFreq} Hz)`;
  return undefined;
}

function validateGain(value: number): string | undefined {
  if (value <= 0) return 'Gain must be positive';
  if (value > 100) return 'Gain must be ≤ 100';
  return undefined;
}

function validateFilterOrder(value: number): string | undefined {
  if (value < 1) return 'Filter order must be ≥ 1';
  if (value > 10) return 'Filter order must be ≤ 10';
  if (!Number.isInteger(value)) return 'Filter order must be an integer';
  return undefined;
}

function validateBandPassFrequencies(low: number, high: number, nyquistFreq: number = 22050): {
  lowError?: string;
  highError?: string;
} {
  const errors: { lowError?: string; highError?: string } = {};

  if (low <= 0) errors.lowError = 'Low cutoff must be positive';
  if (high <= 0) errors.highError = 'High cutoff must be positive';
  if (low >= high) errors.lowError = 'Low cutoff must be less than high cutoff';
  if (high >= nyquistFreq) errors.highError = `High cutoff must be below Nyquist frequency (${nyquistFreq} Hz)`;

  return errors;
}

// ============================================================================
// ProcessingPanel Component
// ============================================================================

export const ProcessingPanel: React.FC<ProcessingPanelProps> = ({
  currentSignalId,
  onProcessSignal,
  isLoading = false
}) => {
  // Form state
  const [operation, setOperation] = useState<OperationType>('LowPass');
  const [cutoffFrequency, setCutoffFrequency] = useState<string>('1000');
  const [lowCutoff, setLowCutoff] = useState<string>('500');
  const [highCutoff, setHighCutoff] = useState<string>('2000');
  const [gain, setGain] = useState<string>('2');
  const [order, setOrder] = useState<string>('4');

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
      case 'cutoffFrequency':
        error = validateCutoffFrequency(numValue);
        break;
      case 'gain':
        error = validateGain(numValue);
        break;
      case 'order':
        error = validateFilterOrder(numValue);
        break;
      case 'lowCutoff':
      case 'highCutoff':
        const lowNum = field === 'lowCutoff' ? numValue : parseFloat(lowCutoff);
        const highNum = field === 'highCutoff' ? numValue : parseFloat(highCutoff);
        const bandErrors = validateBandPassFrequencies(lowNum, highNum);
        setErrors(prev => ({
          ...prev,
          lowCutoff: bandErrors.lowError,
          highCutoff: bandErrors.highError
        }));
        return;
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

  // Handle input changes
  const handleCutoffChange = (value: string) => {
    setCutoffFrequency(value);
    validateField('cutoffFrequency', value);
  };

  const handleLowCutoffChange = (value: string) => {
    setLowCutoff(value);
    validateField('lowCutoff', value);
  };

  const handleHighCutoffChange = (value: string) => {
    setHighCutoff(value);
    validateField('highCutoff', value);
  };

  const handleGainChange = (value: string) => {
    setGain(value);
    validateField('gain', value);
  };

  const handleOrderChange = (value: string) => {
    setOrder(value);
    validateField('order', value);
  };

  // Validate all fields based on operation type
  const validateAll = (): boolean => {
    const newErrors: ValidationErrors = {};

    if (operation === 'LowPass' || operation === 'HighPass') {
      const cutoffNum = parseFloat(cutoffFrequency);
      const orderNum = parseFloat(order);

      if (isNaN(cutoffNum)) {
        newErrors.cutoffFrequency = 'Must be a valid number';
      } else {
        const err = validateCutoffFrequency(cutoffNum);
        if (err) newErrors.cutoffFrequency = err;
      }

      if (isNaN(orderNum)) {
        newErrors.order = 'Must be a valid number';
      } else {
        const err = validateFilterOrder(orderNum);
        if (err) newErrors.order = err;
      }
    } else if (operation === 'BandPass') {
      const lowNum = parseFloat(lowCutoff);
      const highNum = parseFloat(highCutoff);
      const orderNum = parseFloat(order);

      if (isNaN(lowNum)) {
        newErrors.lowCutoff = 'Must be a valid number';
      }
      if (isNaN(highNum)) {
        newErrors.highCutoff = 'Must be a valid number';
      }

      if (!isNaN(lowNum) && !isNaN(highNum)) {
        const bandErrors = validateBandPassFrequencies(lowNum, highNum);
        if (bandErrors.lowError) newErrors.lowCutoff = bandErrors.lowError;
        if (bandErrors.highError) newErrors.highCutoff = bandErrors.highError;
      }

      if (isNaN(orderNum)) {
        newErrors.order = 'Must be a valid number';
      } else {
        const err = validateFilterOrder(orderNum);
        if (err) newErrors.order = err;
      }
    } else if (operation === 'Gain') {
      const gainNum = parseFloat(gain);

      if (isNaN(gainNum)) {
        newErrors.gain = 'Must be a valid number';
      } else {
        const err = validateGain(gainNum);
        if (err) newErrors.gain = err;
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!currentSignalId) {
      alert('Please generate a signal first');
      return;
    }

    if (!validateAll()) {
      return;
    }

    const params: ProcessingRequest = {
      signalId: currentSignalId,
      operation
    };

    if (operation === 'LowPass' || operation === 'HighPass') {
      params.cutoffFrequency = parseFloat(cutoffFrequency);
      params.order = parseInt(order, 10);
    } else if (operation === 'BandPass') {
      params.lowCutoff = parseFloat(lowCutoff);
      params.highCutoff = parseFloat(highCutoff);
      params.order = parseInt(order, 10);
    } else if (operation === 'Gain') {
      params.gain = parseFloat(gain);
    }

    await onProcessSignal(params);
  };

  const hasErrors = Object.keys(errors).length > 0;
  const canProcess = currentSignalId !== null && !isLoading && !hasErrors;

  return (
    <div style={{ padding: '20px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: '#fff' }}>
      <h3 style={{ marginTop: 0, marginBottom: '20px' }}>Signal Processing</h3>

      {!currentSignalId && (
        <p style={{ color: '#d32f2f', marginBottom: '16px' }}>
          Please generate a signal first before applying processing.
        </p>
      )}

      <form onSubmit={handleSubmit}>
        {/* Operation Type */}
        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Operation Type
          </label>
          <select
            value={operation}
            onChange={(e) => {
              setOperation(e.target.value as OperationType);
              setErrors({}); // Clear errors when changing operation
            }}
            disabled={isLoading || !currentSignalId}
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: '1px solid #ccc'
            }}
          >
            <option value="LowPass">Low-Pass Filter</option>
            <option value="HighPass">High-Pass Filter</option>
            <option value="BandPass">Band-Pass Filter</option>
            <option value="Gain">Gain Adjustment</option>
          </select>
        </div>

        {/* Conditional inputs based on operation type */}
        {(operation === 'LowPass' || operation === 'HighPass') && (
          <>
            <div style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
                Cutoff Frequency (Hz)
              </label>
              <input
                type="number"
                value={cutoffFrequency}
                onChange={(e) => handleCutoffChange(e.target.value)}
                disabled={isLoading || !currentSignalId}
                step="0.1"
                style={{
                  width: '100%',
                  padding: '8px',
                  borderRadius: '4px',
                  border: errors.cutoffFrequency ? '1px solid #d32f2f' : '1px solid #ccc'
                }}
              />
              {errors.cutoffFrequency && (
                <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
                  {errors.cutoffFrequency}
                </span>
              )}
            </div>

            <div style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
                Filter Order
              </label>
              <input
                type="number"
                value={order}
                onChange={(e) => handleOrderChange(e.target.value)}
                disabled={isLoading || !currentSignalId}
                step="1"
                style={{
                  width: '100%',
                  padding: '8px',
                  borderRadius: '4px',
                  border: errors.order ? '1px solid #d32f2f' : '1px solid #ccc'
                }}
              />
              {errors.order && (
                <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
                  {errors.order}
                </span>
              )}
            </div>
          </>
        )}

        {operation === 'BandPass' && (
          <>
            <div style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
                Low Cutoff Frequency (Hz)
              </label>
              <input
                type="number"
                value={lowCutoff}
                onChange={(e) => handleLowCutoffChange(e.target.value)}
                disabled={isLoading || !currentSignalId}
                step="0.1"
                style={{
                  width: '100%',
                  padding: '8px',
                  borderRadius: '4px',
                  border: errors.lowCutoff ? '1px solid #d32f2f' : '1px solid #ccc'
                }}
              />
              {errors.lowCutoff && (
                <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
                  {errors.lowCutoff}
                </span>
              )}
            </div>

            <div style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
                High Cutoff Frequency (Hz)
              </label>
              <input
                type="number"
                value={highCutoff}
                onChange={(e) => handleHighCutoffChange(e.target.value)}
                disabled={isLoading || !currentSignalId}
                step="0.1"
                style={{
                  width: '100%',
                  padding: '8px',
                  borderRadius: '4px',
                  border: errors.highCutoff ? '1px solid #d32f2f' : '1px solid #ccc'
                }}
              />
              {errors.highCutoff && (
                <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
                  {errors.highCutoff}
                </span>
              )}
            </div>

            <div style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
                Filter Order
              </label>
              <input
                type="number"
                value={order}
                onChange={(e) => handleOrderChange(e.target.value)}
                disabled={isLoading || !currentSignalId}
                step="1"
                style={{
                  width: '100%',
                  padding: '8px',
                  borderRadius: '4px',
                  border: errors.order ? '1px solid #d32f2f' : '1px solid #ccc'
                }}
              />
              {errors.order && (
                <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
                  {errors.order}
                </span>
              )}
            </div>
          </>
        )}

        {operation === 'Gain' && (
          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
              Gain Multiplier
            </label>
            <input
              type="number"
              value={gain}
              onChange={(e) => handleGainChange(e.target.value)}
              disabled={isLoading || !currentSignalId}
              step="0.1"
              style={{
                width: '100%',
                padding: '8px',
                borderRadius: '4px',
                border: errors.gain ? '1px solid #d32f2f' : '1px solid #ccc'
              }}
            />
            {errors.gain && (
              <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
                {errors.gain}
              </span>
            )}
          </div>
        )}

        {/* Submit Button */}
        <button
          type="submit"
          disabled={!canProcess}
          style={{
            width: '100%',
            padding: '12px',
            backgroundColor: canProcess ? '#1976d2' : '#ccc',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            fontSize: '16px',
            fontWeight: 'bold',
            cursor: canProcess ? 'pointer' : 'not-allowed'
          }}
        >
          {isLoading ? 'Processing...' : 'Apply Processing'}
        </button>
      </form>
    </div>
  );
};

export default ProcessingPanel;
