import React, { useState, useCallback, useEffect } from 'react';
import './index.css';
import { ParameterPanel } from './components/ParameterPanel';
import { ProcessingPanel } from './components/ProcessingPanel';
import { SignalChart, SignalData, ProcessedSignalData } from './components/SignalChart';
import { TriggerInput } from './components/TriggerInput';
import { EventList } from './components/EventList';
import { SignalSelector } from './components/SignalSelector';
import api, { 
  SignalGenerationRequest, 
  ProcessingRequest, 
  TriggerCheckRequest, 
  TriggerConfigRequest,
  SignalDto,
  ProcessedSignalDto
} from './services/api';

// ============================================================================
// App Component
// ============================================================================

function App() {
  // State for signals
  const [currentSignal, setCurrentSignal] = useState<SignalData | null>(null);
  const [processedSignal, setProcessedSignal] = useState<ProcessedSignalData | null>(null);
  const [signalHistory, setSignalHistory] = useState<SignalDto[]>([]);
  const [selectedSignalId, setSelectedSignalId] = useState<string | null>(null);

  // Loading states
  const [isGenerating, setIsGenerating] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [isTriggerLoading, setIsTriggerLoading] = useState(false);

  // Error state
  const [error, setError] = useState<string>('');

  // Track last update time for chart update performance
  const [lastChartUpdate, setLastChartUpdate] = useState<number>(0);

  // ============================================================================
  // Signal Generation Handler
  // ============================================================================

  const handleGenerateSignal = useCallback(async (params: SignalGenerationRequest) => {
    setIsGenerating(true);
    setError('');
    
    try {
      const startTime = performance.now();
      
      // Generate signal via API
      const signalDto = await api.signals.generateSignal(params);
      
      // Fetch complete signal data with samples
      const completeSignal = await api.signals.getSignal(signalDto.id);
      
      // Convert DTO to SignalData format for chart
      const signalData: SignalData = {
        id: completeSignal.id,
        type: completeSignal.type,
        samples: [], // Will be populated by fetching samples
        timestamps: [],
        metadata: {
          frequency: completeSignal.metadata.frequency,
          amplitude: completeSignal.metadata.amplitude,
          duration: completeSignal.metadata.duration,
          sampleRate: completeSignal.metadata.sampleRate
        }
      };

      // Generate timestamps and samples locally for visualization
      // (In production, these would come from the API)
      const sampleCount = Math.floor(params.duration * params.sampleRate);
      const timestamps = Array.from({ length: sampleCount }, (_, i) => i / params.sampleRate);
      const samples = generateSamplesLocally(params, sampleCount);
      
      signalData.timestamps = timestamps;
      signalData.samples = samples;

      setCurrentSignal(signalData);
      setProcessedSignal(null); // Clear processed signal when new signal is generated
      setSelectedSignalId(signalDto.id);
      
      // Update chart within 100ms requirement
      const endTime = performance.now();
      setLastChartUpdate(endTime - startTime);
      
      // Refresh signal history
      await refreshSignalHistory();
      
    } catch (err: any) {
      setError(err.message || 'Failed to generate signal');
      console.error('Signal generation error:', err);
    } finally {
      setIsGenerating(false);
    }
  }, []);

  // ============================================================================
  // Signal Processing Handler
  // ============================================================================

  const handleProcessSignal = useCallback(async (params: ProcessingRequest) => {
    if (!currentSignal) {
      setError('No signal to process');
      return;
    }

    setIsProcessing(true);
    setError('');
    
    try {
      const startTime = performance.now();
      
      // Process signal via API
      const processedDto = await api.processing.processSignal(params);
      
      // Fetch complete processed signal data
      const completeProcessed = await api.processing.getProcessedSignal(processedDto.id);
      
      // Convert DTO to ProcessedSignalData format
      const processedData: ProcessedSignalData = {
        id: completeProcessed.id,
        type: completeProcessed.type,
        samples: [],
        timestamps: [],
        metadata: {
          frequency: completeProcessed.metadata.frequency,
          amplitude: completeProcessed.metadata.amplitude,
          duration: completeProcessed.metadata.duration,
          sampleRate: completeProcessed.metadata.sampleRate
        },
        originalSignalId: completeProcessed.originalSignalId,
        processingParams: {
          operation: completeProcessed.processingParams.operation,
          cutoffFrequency: completeProcessed.processingParams.cutoffFrequency,
          gain: completeProcessed.processingParams.gain
        }
      };

      // Generate processed samples locally for visualization
      const samples = applyProcessingLocally(currentSignal.samples, params);
      processedData.timestamps = currentSignal.timestamps;
      processedData.samples = samples;

      setProcessedSignal(processedData);
      
      // Update chart within 100ms requirement
      const endTime = performance.now();
      setLastChartUpdate(endTime - startTime);
      
    } catch (err: any) {
      setError(err.message || 'Failed to process signal');
      console.error('Signal processing error:', err);
    } finally {
      setIsProcessing(false);
    }
  }, [currentSignal]);

  // ============================================================================
  // Trigger Handlers
  // ============================================================================

  const handleConfigureThreshold = useCallback(async (request: TriggerConfigRequest) => {
    setIsTriggerLoading(true);
    try {
      await api.triggers.configure(request);
    } catch (err: any) {
      setError(err.message || 'Failed to configure threshold');
      throw err;
    } finally {
      setIsTriggerLoading(false);
    }
  }, []);

  const handleCheckValue = useCallback(async (request: TriggerCheckRequest) => {
    setIsTriggerLoading(true);
    try {
      const event = await api.triggers.checkValue(request);
      return event;
    } catch (err: any) {
      setError(err.message || 'Failed to check value');
      throw err;
    } finally {
      setIsTriggerLoading(false);
    }
  }, []);

  const handleFetchEvents = useCallback(async () => {
    try {
      const events = await api.triggers.getEvents({ pageSize: 50 });
      return events;
    } catch (err: any) {
      console.error('Failed to fetch events:', err);
      return [];
    }
  }, []);

  // ============================================================================
  // Signal History Handlers
  // ============================================================================

  const refreshSignalHistory = useCallback(async () => {
    try {
      const signals = await api.signals.getRecentSignals({ pageSize: 20 });
      setSignalHistory(signals);
    } catch (err: any) {
      console.error('Failed to fetch signal history:', err);
    }
  }, []);

  const handleSelectSignal = useCallback(async (signalId: string) => {
    try {
      const signalDto = await api.signals.getSignal(signalId);
      
      // Convert to SignalData format
      const signalData: SignalData = {
        id: signalDto.id,
        type: signalDto.type,
        samples: [],
        timestamps: [],
        metadata: {
          frequency: signalDto.metadata.frequency,
          amplitude: signalDto.metadata.amplitude,
          duration: signalDto.metadata.duration,
          sampleRate: signalDto.metadata.sampleRate
        }
      };

      // Generate samples locally for visualization
      const sampleCount = Math.floor(signalDto.metadata.duration * signalDto.metadata.sampleRate);
      const timestamps = Array.from({ length: sampleCount }, (_, i) => i / signalDto.metadata.sampleRate);
      const samples = generateSamplesLocally({
        type: signalDto.type as any,
        frequency: signalDto.metadata.frequency,
        amplitude: signalDto.metadata.amplitude,
        phase: signalDto.metadata.phase,
        duration: signalDto.metadata.duration,
        sampleRate: signalDto.metadata.sampleRate
      }, sampleCount);
      
      signalData.timestamps = timestamps;
      signalData.samples = samples;

      setCurrentSignal(signalData);
      setProcessedSignal(null);
      setSelectedSignalId(signalId);
    } catch (err: any) {
      setError(err.message || 'Failed to load signal');
    }
  }, []);

  // Load initial signal history
  useEffect(() => {
    refreshSignalHistory();
  }, [refreshSignalHistory]);

  // ============================================================================
  // Helper Functions for Local Sample Generation
  // ============================================================================

  function generateSamplesLocally(params: SignalGenerationRequest, sampleCount: number): number[] {
    const samples: number[] = [];
    const { type, frequency, amplitude, phase, sampleRate } = params;

    for (let i = 0; i < sampleCount; i++) {
      const t = i / sampleRate;
      let value = 0;

      switch (type) {
        case 'Sine':
          value = amplitude * Math.sin(2 * Math.PI * frequency * t + phase);
          break;
        case 'Square':
          value = amplitude * Math.sign(Math.sin(2 * Math.PI * frequency * t + phase));
          break;
        case 'Sawtooth':
          value = amplitude * (2 * ((frequency * t + phase / (2 * Math.PI)) % 1) - 1);
          break;
        case 'Noise':
          value = amplitude * (Math.random() * 2 - 1);
          break;
      }

      samples.push(value);
    }

    return samples;
  }

  function applyProcessingLocally(samples: number[], params: ProcessingRequest): number[] {
    // Simple processing simulation for visualization
    // In production, this would come from the backend
    if (params.operation === 'Gain' && params.gain) {
      return samples.map(s => s * params.gain!);
    }
    
    // For filters, just return a slightly modified version for demo
    // Real filtering would be done by the backend
    return samples.map(s => s * 0.8);
  }

  // ============================================================================
  // Render
  // ============================================================================

  return (
    <div style={{ padding: '20px', fontFamily: 'Arial, sans-serif', maxWidth: '1600px', margin: '0 auto' }}>
      <header style={{ marginBottom: '30px', textAlign: 'center' }}>
        <h1 style={{ margin: '0 0 10px 0', color: '#1976d2' }}>Signal Processing Visualization</h1>
        <p style={{ color: '#666', margin: 0 }}>
          Interactive signal generation, processing, and visualization tool
        </p>
      </header>

      {/* Error Display */}
      {error && (
        <div
          style={{
            padding: '12px',
            marginBottom: '20px',
            backgroundColor: '#ffebee',
            color: '#c62828',
            borderRadius: '4px',
            border: '1px solid #ef5350'
          }}
        >
          {error}
          <button
            onClick={() => setError('')}
            style={{
              marginLeft: '12px',
              padding: '4px 8px',
              backgroundColor: '#c62828',
              color: '#fff',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Dismiss
          </button>
        </div>
      )}

      {/* Performance Indicator */}
      {lastChartUpdate > 0 && (
        <div
          style={{
            padding: '8px',
            marginBottom: '20px',
            backgroundColor: lastChartUpdate < 100 ? '#e8f5e9' : '#fff3e0',
            color: lastChartUpdate < 100 ? '#2e7d32' : '#e65100',
            borderRadius: '4px',
            fontSize: '12px',
            textAlign: 'center'
          }}
        >
          Chart updated in {lastChartUpdate.toFixed(2)}ms 
          {lastChartUpdate < 100 ? ' ✓ (within 100ms requirement)' : ' ⚠ (exceeds 100ms requirement)'}
        </div>
      )}

      <main>
        {/* Top Section: Signal Chart */}
        <section style={{ marginBottom: '20px' }}>
          <SignalChart
            signalData={currentSignal}
            processedData={processedSignal}
            height={450}
          />
        </section>

        {/* Middle Section: Signal Selector */}
        <section style={{ marginBottom: '20px' }}>
          <SignalSelector
            signals={signalHistory}
            selectedSignalId={selectedSignalId}
            onSelectSignal={handleSelectSignal}
            onRefresh={refreshSignalHistory}
            isLoading={isGenerating}
          />
        </section>

        {/* Bottom Section: Control Panels */}
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px', marginBottom: '20px' }}>
          {/* Left Column: Signal Generation */}
          <ParameterPanel
            onGenerateSignal={handleGenerateSignal}
            isLoading={isGenerating}
          />

          {/* Right Column: Signal Processing */}
          <ProcessingPanel
            currentSignalId={selectedSignalId}
            onProcessSignal={handleProcessSignal}
            isLoading={isProcessing}
          />
        </div>

        {/* Trigger Section */}
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px' }}>
          {/* Left Column: Trigger Input */}
          <TriggerInput
            onCheckValue={handleCheckValue}
            onConfigureThreshold={handleConfigureThreshold}
            isLoading={isTriggerLoading}
          />

          {/* Right Column: Event List */}
          <EventList
            onFetchEvents={handleFetchEvents}
            autoRefreshInterval={5000}
            maxEvents={50}
          />
        </div>
      </main>
    </div>
  );
}

export default App;
