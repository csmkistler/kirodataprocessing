import React, { useMemo, useState } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  Brush
} from 'recharts';

// ============================================================================
// Types
// ============================================================================

export interface SignalData {
  id: string;
  type: string;
  samples: number[];
  timestamps: number[];
  metadata: {
    frequency: number;
    amplitude: number;
    duration: number;
    sampleRate: number;
  };
}

export interface ProcessedSignalData extends SignalData {
  originalSignalId: string;
  processingParams: {
    operation: string;
    cutoffFrequency?: number;
    gain?: number;
  };
}

interface SignalChartProps {
  signalData: SignalData | null;
  processedData: ProcessedSignalData | null;
  height?: number;
}

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * Downsample signal data for efficient rendering when sample count exceeds threshold.
 * Uses simple decimation - takes every nth sample.
 */
function downsampleData(
  timestamps: number[],
  samples: number[],
  maxPoints: number = 10000
): { time: number; amplitude: number }[] {
  const dataLength = timestamps.length;
  
  if (dataLength <= maxPoints) {
    // No downsampling needed
    return timestamps.map((time, idx) => ({
      time,
      amplitude: samples[idx]
    }));
  }

  // Calculate decimation factor
  const decimationFactor = Math.ceil(dataLength / maxPoints);
  const result: { time: number; amplitude: number }[] = [];

  for (let i = 0; i < dataLength; i += decimationFactor) {
    result.push({
      time: timestamps[i],
      amplitude: samples[i]
    });
  }

  return result;
}

/**
 * Merge original and processed signal data for dual-line chart.
 */
function mergeSignalData(
  originalTimestamps: number[],
  originalSamples: number[],
  processedTimestamps: number[] | null,
  processedSamples: number[] | null,
  maxPoints: number = 10000
): { time: number; original: number; processed?: number }[] {
  // Downsample original signal
  const originalData = downsampleData(originalTimestamps, originalSamples, maxPoints);
  
  if (!processedTimestamps || !processedSamples) {
    // Only original signal
    return originalData.map(d => ({
      time: d.time,
      original: d.amplitude
    }));
  }

  // Downsample processed signal
  const processedData = downsampleData(processedTimestamps, processedSamples, maxPoints);
  
  // Create a map for quick lookup
  const processedMap = new Map(
    processedData.map(d => [d.time.toFixed(6), d.amplitude])
  );

  // Merge data
  return originalData.map(d => ({
    time: d.time,
    original: d.amplitude,
    processed: processedMap.get(d.time.toFixed(6))
  }));
}

// ============================================================================
// SignalChart Component
// ============================================================================

export const SignalChart: React.FC<SignalChartProps> = ({
  signalData,
  processedData,
  height = 400
}) => {
  const [zoomDomain, setZoomDomain] = useState<[number, number] | null>(null);

  // Prepare chart data with downsampling
  const chartData = useMemo(() => {
    if (!signalData) return [];

    const originalTimestamps = signalData.timestamps;
    const originalSamples = signalData.samples;
    const processedTimestamps = processedData?.timestamps || null;
    const processedSamples = processedData?.samples || null;

    return mergeSignalData(
      originalTimestamps,
      originalSamples,
      processedTimestamps,
      processedSamples
    );
  }, [signalData, processedData]);

  // Calculate axis domains
  const { timeDomain, amplitudeDomain } = useMemo(() => {
    if (chartData.length === 0) {
      return { timeDomain: [0, 1], amplitudeDomain: [-1, 1] };
    }

    const times = chartData.map(d => d.time);
    const allAmplitudes = chartData.flatMap(d => 
      [d.original, d.processed].filter((v): v is number => v !== undefined)
    );

    const minTime = Math.min(...times);
    const maxTime = Math.max(...times);
    const minAmp = Math.min(...allAmplitudes);
    const maxAmp = Math.max(...allAmplitudes);

    // Add 10% padding to amplitude domain
    const ampPadding = (maxAmp - minAmp) * 0.1;

    return {
      timeDomain: [minTime, maxTime] as [number, number],
      amplitudeDomain: [minAmp - ampPadding, maxAmp + ampPadding] as [number, number]
    };
  }, [chartData]);

  // Handle zoom/pan via brush
  const handleBrushChange = (domain: any) => {
    if (domain && domain.startIndex !== undefined && domain.endIndex !== undefined) {
      const startTime = chartData[domain.startIndex]?.time;
      const endTime = chartData[domain.endIndex]?.time;
      if (startTime !== undefined && endTime !== undefined) {
        setZoomDomain([startTime, endTime]);
      }
    }
  };

  const displayDomain = zoomDomain || timeDomain;

  if (!signalData) {
    return (
      <div
        style={{
          height,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          border: '1px solid #ddd',
          borderRadius: '4px',
          backgroundColor: '#f9f9f9'
        }}
      >
        <p style={{ color: '#666' }}>No signal data to display</p>
      </div>
    );
  }

  return (
    <div style={{ width: '100%' }}>
      <ResponsiveContainer width="100%" height={height}>
        <LineChart
          data={chartData}
          margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="time"
            type="number"
            domain={displayDomain}
            label={{ value: 'Time (s)', position: 'insideBottom', offset: -5 }}
            tickFormatter={(value) => value.toFixed(3)}
          />
          <YAxis
            domain={amplitudeDomain}
            label={{ value: 'Amplitude', angle: -90, position: 'insideLeft' }}
            tickFormatter={(value) => value.toFixed(2)}
          />
          <Tooltip
            formatter={(value: number) => value.toFixed(4)}
            labelFormatter={(label) => `Time: ${Number(label).toFixed(4)}s`}
          />
          <Legend />
          <Line
            type="monotone"
            dataKey="original"
            stroke="#8884d8"
            name="Original Signal"
            dot={false}
            strokeWidth={2}
            isAnimationActive={false}
          />
          {processedData && (
            <Line
              type="monotone"
              dataKey="processed"
              stroke="#82ca9d"
              name="Processed Signal"
              dot={false}
              strokeWidth={2}
              isAnimationActive={false}
            />
          )}
          <Brush
            dataKey="time"
            height={30}
            stroke="#8884d8"
            onChange={handleBrushChange}
          />
        </LineChart>
      </ResponsiveContainer>
      
      {chartData.length > 10000 && (
        <p style={{ fontSize: '12px', color: '#666', marginTop: '8px' }}>
          Note: Data downsampled for performance ({chartData.length.toLocaleString()} points displayed)
        </p>
      )}
    </div>
  );
};

export default SignalChart;
