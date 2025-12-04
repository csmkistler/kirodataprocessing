import React, { useEffect, useState } from 'react';
import { SignalDto } from '../services/api';

// ============================================================================
// Types
// ============================================================================

interface SignalSelectorProps {
  signals: SignalDto[];
  selectedSignalId: string | null;
  onSelectSignal: (signalId: string) => void;
  onRefresh: () => void;
  isLoading?: boolean;
}

// ============================================================================
// Helper Functions
// ============================================================================

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
}

function formatFrequency(frequency: number): string {
  if (frequency >= 1000) {
    return `${(frequency / 1000).toFixed(1)} kHz`;
  }
  return `${frequency.toFixed(1)} Hz`;
}

// ============================================================================
// SignalSelector Component
// ============================================================================

export const SignalSelector: React.FC<SignalSelectorProps> = ({
  signals,
  selectedSignalId,
  onSelectSignal,
  onRefresh,
  isLoading = false
}) => {
  const [isRefreshing, setIsRefreshing] = useState(false);

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await onRefresh();
    setIsRefreshing(false);
  };

  // Auto-select first signal if none selected and signals available
  useEffect(() => {
    if (!selectedSignalId && signals.length > 0) {
      onSelectSignal(signals[0].id);
    }
  }, [signals, selectedSignalId, onSelectSignal]);

  return (
    <div style={{ padding: '20px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: '#fff' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <h3 style={{ margin: 0 }}>Signal History</h3>
        <button
          onClick={handleRefresh}
          disabled={isRefreshing || isLoading}
          style={{
            padding: '6px 12px',
            backgroundColor: '#1976d2',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            cursor: isRefreshing || isLoading ? 'not-allowed' : 'pointer',
            fontSize: '14px'
          }}
        >
          {isRefreshing ? 'Refreshing...' : 'Refresh'}
        </button>
      </div>

      {signals.length === 0 ? (
        <p style={{ color: '#666', fontStyle: 'italic' }}>
          No signals available. Generate a signal to get started.
        </p>
      ) : (
        <>
          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>
              Select Signal
            </label>
            <select
              value={selectedSignalId || ''}
              onChange={(e) => onSelectSignal(e.target.value)}
              disabled={isLoading}
              style={{
                width: '100%',
                padding: '8px',
                borderRadius: '4px',
                border: '1px solid #ccc',
                fontSize: '14px'
              }}
            >
              {signals.map((signal) => (
                <option key={signal.id} value={signal.id}>
                  {signal.type} - {formatFrequency(signal.metadata.frequency)} - {formatDate(signal.createdAt)}
                </option>
              ))}
            </select>
          </div>

          {/* Display metadata for selected signal */}
          {selectedSignalId && (() => {
            const selectedSignal = signals.find(s => s.id === selectedSignalId);
            if (!selectedSignal) return null;

            return (
              <div style={{ 
                padding: '12px', 
                backgroundColor: '#f5f5f5', 
                borderRadius: '4px',
                fontSize: '14px'
              }}>
                <h4 style={{ marginTop: 0, marginBottom: '8px', fontSize: '14px' }}>Signal Details</h4>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
                  <div>
                    <strong>Type:</strong> {selectedSignal.type}
                  </div>
                  <div>
                    <strong>Frequency:</strong> {formatFrequency(selectedSignal.metadata.frequency)}
                  </div>
                  <div>
                    <strong>Amplitude:</strong> {selectedSignal.metadata.amplitude.toFixed(2)}
                  </div>
                  <div>
                    <strong>Phase:</strong> {selectedSignal.metadata.phase.toFixed(2)} rad
                  </div>
                  <div>
                    <strong>Duration:</strong> {selectedSignal.metadata.duration.toFixed(2)} s
                  </div>
                  <div>
                    <strong>Sample Rate:</strong> {selectedSignal.metadata.sampleRate.toLocaleString()} Hz
                  </div>
                  <div style={{ gridColumn: '1 / -1' }}>
                    <strong>Samples:</strong> {selectedSignal.sampleCount.toLocaleString()}
                  </div>
                  <div style={{ gridColumn: '1 / -1' }}>
                    <strong>Created:</strong> {formatDate(selectedSignal.createdAt)}
                  </div>
                </div>
              </div>
            );
          })()}
        </>
      )}
    </div>
  );
};

export default SignalSelector;
