import React, { useState } from 'react';
import { TriggerCheckRequest, TriggerConfigRequest, TriggerEventDto } from '../services/api';

// ============================================================================
// Types
// ============================================================================

interface TriggerInputProps {
  onCheckValue: (request: TriggerCheckRequest) => Promise<TriggerEventDto | null>;
  onConfigureThreshold: (request: TriggerConfigRequest) => Promise<void>;
  isLoading?: boolean;
}

interface ValidationErrors {
  value?: string;
  threshold?: string;
}

// ============================================================================
// Validation Functions
// ============================================================================

function validateNumericInput(value: string): string | undefined {
  if (value.trim() === '') return 'Value is required';
  const numValue = parseFloat(value);
  if (isNaN(numValue)) return 'Must be a valid number';
  return undefined;
}

// ============================================================================
// TriggerInput Component
// ============================================================================

export const TriggerInput: React.FC<TriggerInputProps> = ({
  onCheckValue,
  onConfigureThreshold,
  isLoading = false
}) => {
  // Form state
  const [inputValue, setInputValue] = useState<string>('');
  const [threshold, setThreshold] = useState<string>('5.0');
  const [thresholdEnabled, setThresholdEnabled] = useState<boolean>(true);
  
  // Feedback state
  const [feedback, setFeedback] = useState<string>('');
  const [feedbackType, setFeedbackType] = useState<'success' | 'info' | 'error'>('info');
  
  // Validation errors
  const [errors, setErrors] = useState<ValidationErrors>({});

  // Real-time validation
  const validateField = (field: keyof ValidationErrors, value: string) => {
    const error = validateNumericInput(value);
    
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

  // Handle input value change
  const handleInputValueChange = (value: string) => {
    setInputValue(value);
    validateField('value', value);
  };

  // Handle threshold change
  const handleThresholdChange = (value: string) => {
    setThreshold(value);
    validateField('threshold', value);
  };

  // Handle threshold configuration
  const handleConfigureThreshold = async () => {
    const error = validateNumericInput(threshold);
    if (error) {
      setErrors({ threshold: error });
      return;
    }

    try {
      await onConfigureThreshold({
        threshold: parseFloat(threshold),
        enabled: thresholdEnabled
      });
      
      setFeedback(`Threshold configured to ${threshold}`);
      setFeedbackType('success');
      
      // Clear feedback after 3 seconds
      setTimeout(() => setFeedback(''), 3000);
    } catch (err) {
      setFeedback('Failed to configure threshold');
      setFeedbackType('error');
    }
  };

  // Handle value check
  const handleCheckValue = async (e: React.FormEvent) => {
    e.preventDefault();

    const error = validateNumericInput(inputValue);
    if (error) {
      setErrors({ value: error });
      return;
    }

    try {
      const event = await onCheckValue({
        value: parseFloat(inputValue)
      });

      if (event) {
        setFeedback(`⚠️ Threshold exceeded! Value ${event.value} > Threshold ${event.threshold}`);
        setFeedbackType('error');
      } else {
        setFeedback(`✓ Value ${inputValue} is within threshold`);
        setFeedbackType('success');
      }

      // Clear input after check
      setInputValue('');
      setErrors({});
      
      // Clear feedback after 5 seconds
      setTimeout(() => setFeedback(''), 5000);
    } catch (err) {
      setFeedback('Failed to check value');
      setFeedbackType('error');
    }
  };

  const hasErrors = Object.keys(errors).length > 0;

  return (
    <div style={{ padding: '20px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: '#fff' }}>
      <h3 style={{ marginTop: 0, marginBottom: '20px' }}>Trigger Monitor</h3>
      
      {/* Threshold Configuration Section */}
      <div style={{ marginBottom: '24px', padding: '16px', backgroundColor: '#f5f5f5', borderRadius: '4px' }}>
        <h4 style={{ marginTop: 0, marginBottom: '12px' }}>Configure Threshold</h4>
        
        <div style={{ marginBottom: '12px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Threshold Value
          </label>
          <input
            type="number"
            value={threshold}
            onChange={(e) => handleThresholdChange(e.target.value)}
            disabled={isLoading}
            step="0.1"
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: errors.threshold ? '1px solid #d32f2f' : '1px solid #ccc'
            }}
          />
          {errors.threshold && (
            <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {errors.threshold}
            </span>
          )}
        </div>

        <div style={{ marginBottom: '12px' }}>
          <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }}>
            <input
              type="checkbox"
              checked={thresholdEnabled}
              onChange={(e) => setThresholdEnabled(e.target.checked)}
              disabled={isLoading}
              style={{ marginRight: '8px' }}
            />
            <span>Enable threshold monitoring</span>
          </label>
        </div>

        <button
          type="button"
          onClick={handleConfigureThreshold}
          disabled={isLoading || !!errors.threshold}
          style={{
            width: '100%',
            padding: '10px',
            backgroundColor: isLoading || errors.threshold ? '#ccc' : '#4caf50',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            fontSize: '14px',
            fontWeight: 'bold',
            cursor: isLoading || errors.threshold ? 'not-allowed' : 'pointer'
          }}
        >
          {isLoading ? 'Configuring...' : 'Set Threshold'}
        </button>
      </div>

      {/* Value Check Section */}
      <form onSubmit={handleCheckValue}>
        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
            Enter Value to Check
          </label>
          <input
            type="number"
            value={inputValue}
            onChange={(e) => handleInputValueChange(e.target.value)}
            disabled={isLoading}
            step="0.1"
            placeholder="Enter a numeric value"
            style={{
              width: '100%',
              padding: '8px',
              borderRadius: '4px',
              border: errors.value ? '1px solid #d32f2f' : '1px solid #ccc'
            }}
          />
          {errors.value && (
            <span style={{ color: '#d32f2f', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {errors.value}
            </span>
          )}
        </div>

        <button
          type="submit"
          disabled={isLoading || hasErrors || !inputValue}
          style={{
            width: '100%',
            padding: '12px',
            backgroundColor: isLoading || hasErrors || !inputValue ? '#ccc' : '#1976d2',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            fontSize: '16px',
            fontWeight: 'bold',
            cursor: isLoading || hasErrors || !inputValue ? 'not-allowed' : 'pointer'
          }}
        >
          {isLoading ? 'Checking...' : 'Check Value'}
        </button>
      </form>

      {/* Feedback Display */}
      {feedback && (
        <div
          style={{
            marginTop: '16px',
            padding: '12px',
            borderRadius: '4px',
            backgroundColor: 
              feedbackType === 'error' ? '#ffebee' : 
              feedbackType === 'success' ? '#e8f5e9' : 
              '#e3f2fd',
            color: 
              feedbackType === 'error' ? '#c62828' : 
              feedbackType === 'success' ? '#2e7d32' : 
              '#1565c0',
            border: `1px solid ${
              feedbackType === 'error' ? '#ef5350' : 
              feedbackType === 'success' ? '#66bb6a' : 
              '#42a5f5'
            }`
          }}
        >
          {feedback}
        </div>
      )}
    </div>
  );
};

export default TriggerInput;
