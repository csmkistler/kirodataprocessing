import axios, { AxiosError } from 'axios';

// ============================================================================
// TypeScript Interfaces - Request Types
// ============================================================================

export interface SignalGenerationRequest {
  type: 'Sine' | 'Square' | 'Sawtooth' | 'Noise';
  frequency: number;
  amplitude: number;
  phase: number;
  duration: number;
  sampleRate: number;
}

export interface ProcessingRequest {
  signalId: string;
  operation: 'LowPass' | 'HighPass' | 'BandPass' | 'Gain';
  cutoffFrequency?: number;
  lowCutoff?: number;
  highCutoff?: number;
  gain?: number;
  order?: number;
}

export interface TriggerConfigRequest {
  threshold: number;
  enabled: boolean;
}

export interface TriggerCheckRequest {
  value: number;
}

// ============================================================================
// TypeScript Interfaces - Response Types
// ============================================================================

export interface SignalMetadataDto {
  frequency: number;
  amplitude: number;
  phase: number;
  duration: number;
  sampleRate: number;
}

export interface ProcessingParamsDto {
  operation: string;
  cutoffFrequency?: number;
  lowCutoff?: number;
  highCutoff?: number;
  gain?: number;
  order?: number;
}

export interface SignalDto {
  id: string;
  type: string;
  metadata: SignalMetadataDto;
  createdAt: string;
  sampleCount: number;
}

export interface ProcessedSignalDto {
  id: string;
  type: string;
  metadata: SignalMetadataDto;
  createdAt: string;
  sampleCount: number;
  originalSignalId: string;
  processingParams: ProcessingParamsDto;
}

export interface TriggerEventDto {
  id: string;
  value: number;
  threshold: number;
  timestamp: string;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
}

// ============================================================================
// Error Handling
// ============================================================================

export interface ApiError {
  message: string;
  statusCode?: number;
  validationErrors?: string[];
}

class ApiErrorHandler {
  static handle(error: unknown): ApiError {
    if (axios.isAxiosError(error)) {
      const axiosError = error as AxiosError<any>;
      
      if (axiosError.response) {
        // Server responded with error status
        const data = axiosError.response.data;
        return {
          message: data?.error || data?.message || 'An error occurred',
          statusCode: axiosError.response.status,
          validationErrors: data?.validationErrors || []
        };
      } else if (axiosError.request) {
        // Request made but no response received
        return {
          message: 'No response from server. Please check your connection.',
          statusCode: 0
        };
      }
    }
    
    // Unknown error
    return {
      message: error instanceof Error ? error.message : 'An unknown error occurred',
      statusCode: 500
    };
  }
}

// ============================================================================
// Axios Client Configuration
// ============================================================================

const apiClient = axios.create({
  baseURL: '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response: any) => response,
  (error: any) => {
    const apiError = ApiErrorHandler.handle(error);
    console.error('API Error:', apiError);
    return Promise.reject(apiError);
  }
);

// ============================================================================
// API Service Methods
// ============================================================================

export const signalApi = {
  /**
   * Generate a new signal with specified parameters.
   * @param request Signal generation parameters
   * @returns Generated signal data
   */
  async generateSignal(request: SignalGenerationRequest): Promise<SignalDto> {
    const response = await apiClient.post<SignalDto>('/signals/generate', request);
    return response.data;
  },

  /**
   * Get a signal by ID.
   * @param id Signal ID
   * @returns Signal data
   */
  async getSignal(id: string): Promise<SignalDto> {
    const response = await apiClient.get<SignalDto>(`/signals/${id}`);
    return response.data;
  },

  /**
   * Get recent signals with pagination.
   * @param params Pagination parameters
   * @returns List of signals
   */
  async getRecentSignals(params?: PaginationParams): Promise<SignalDto[]> {
    const response = await apiClient.get<SignalDto[]>('/signals', { params });
    return response.data;
  }
};

export const processingApi = {
  /**
   * Process a signal with specified parameters.
   * @param request Processing parameters
   * @returns Processed signal data
   */
  async processSignal(request: ProcessingRequest): Promise<ProcessedSignalDto> {
    const response = await apiClient.post<ProcessedSignalDto>('/processing/process', request);
    return response.data;
  },

  /**
   * Get a processed signal by ID.
   * @param id Processed signal ID
   * @returns Processed signal data
   */
  async getProcessedSignal(id: string): Promise<ProcessedSignalDto> {
    const response = await apiClient.get<ProcessedSignalDto>(`/processing/${id}`);
    return response.data;
  }
};

export const triggerApi = {
  /**
   * Configure trigger threshold.
   * @param request Trigger configuration
   */
  async configure(request: TriggerConfigRequest): Promise<void> {
    await apiClient.post('/triggers/configure', request);
  },

  /**
   * Check a value against the configured threshold.
   * @param request Value to check
   * @returns Trigger event if threshold exceeded, null otherwise
   */
  async checkValue(request: TriggerCheckRequest): Promise<TriggerEventDto | null> {
    const response = await apiClient.post<TriggerEventDto | null>('/triggers/check', request);
    return response.data;
  },

  /**
   * Get trigger events with pagination.
   * @param params Pagination parameters
   * @returns List of trigger events
   */
  async getEvents(params?: PaginationParams): Promise<TriggerEventDto[]> {
    const response = await apiClient.get<TriggerEventDto[]>('/triggers/events', { params });
    return response.data;
  }
};

// ============================================================================
// Export default API object
// ============================================================================

const api = {
  signals: signalApi,
  processing: processingApi,
  triggers: triggerApi
};

export default api;
