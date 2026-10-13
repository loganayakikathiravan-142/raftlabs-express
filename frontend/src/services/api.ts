import { ProblemDetails } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

export class ApiError extends Error {
  public status: number;
  public problemDetails?: ProblemDetails;

  constructor(status: number, message: string, problemDetails?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problemDetails = problemDetails;
  }
}

export async function apiClient<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...(options?.headers || {}),
  };

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let errorDetail: ProblemDetails | undefined;
    let errorMessage = `HTTP request failed with status ${response.status}`;

    try {
      errorDetail = await response.json();
      if (errorDetail?.detail) {
        errorMessage = errorDetail.detail;
      } else if (errorDetail?.title) {
        errorMessage = errorDetail.title;
      }
    } catch {
      // Body not JSON
    }

    throw new ApiError(response.status, errorMessage, errorDetail);
  }

  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}
