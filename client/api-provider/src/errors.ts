import type { ErrorItemResponse, ErrorResponse } from './types';

export class ApiError extends Error {
  public readonly status: number;
  public readonly errors: ErrorItemResponse[];
  public readonly response: Response;

  public constructor(status: number, errors: ErrorItemResponse[], response: Response) {
    super(errors.map((error) => error.message).join('; ') || `Request failed with status ${status}.`);
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
    this.response = response;
  }
}

export class NetworkError extends Error {
  public readonly cause: unknown;

  public constructor(cause: unknown) {
    super('Network request failed.');
    this.name = 'NetworkError';
    this.cause = cause;
  }
}

export function isErrorResponse(value: unknown): value is ErrorResponse {
  if (typeof value !== 'object' || value === null || !('errors' in value)) {
    return false;
  }

  const errors = (value as { errors: unknown }).errors;

  return Array.isArray(errors);
}
