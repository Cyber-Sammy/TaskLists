import { ApiError, NetworkError, isErrorResponse } from './errors';
import type { ErrorItemResponse } from './types';

export interface ApiClientConfig {
  baseUrl: string;
  currentUserId?: string;
  correlationIdFactory?: () => string;
  defaultHeaders?: HeadersInit;
  fetch?: typeof fetch;
}

export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown | undefined;
  query?: Record<string, string | number | boolean | null | undefined> | undefined;
  headers?: HeadersInit | undefined;
  currentUserId?: string | undefined;
  signal?: AbortSignal | undefined;
}

export class ApiClient {
  private readonly baseUrl: string;
  private readonly defaultHeaders: HeadersInit | undefined;
  private readonly currentUserId: string | undefined;
  private readonly correlationIdFactory: (() => string) | undefined;
  private readonly fetchImpl: typeof fetch;

  public constructor(config: ApiClientConfig) {
    this.baseUrl = config.baseUrl.replace(/\/+$/, '');
    this.defaultHeaders = config.defaultHeaders;
    this.currentUserId = config.currentUserId;
    this.correlationIdFactory = config.correlationIdFactory;
    this.fetchImpl = config.fetch ?? fetch;
  }

  public async request<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse> {
    const response = await this.send(path, options);

    if (response.status === 204) {
      return undefined as TResponse;
    }

    return await response.json() as TResponse;
  }

  public async requestEmpty(path: string, options: RequestOptions = {}): Promise<void> {
    await this.send(path, options);
  }

  private async send(path: string, options: RequestOptions): Promise<Response> {
    const url = this.buildUrl(path, options.query);
    const headers = this.buildHeaders(options);

    let response: Response;

    try {
      const requestInit: RequestInit = {
        method: options.method ?? 'GET',
        headers,
      };

      if (options.body !== undefined) {
        requestInit.body = JSON.stringify(options.body);
      }

      if (options.signal !== undefined) {
        requestInit.signal = options.signal;
      }

      response = await this.fetchImpl(url, {
        ...requestInit,
      });
    } catch (error) {
      throw new NetworkError(error);
    }

    if (!response.ok) {
      throw await this.createApiError(response);
    }

    return response;
  }

  private buildUrl(path: string, query?: RequestOptions['query']): string {
    const normalizedPath = path.startsWith('/') ? path : `/${path}`;
    const url = new URL(`${this.baseUrl}${normalizedPath}`);

    if (query === undefined) {
      return url.toString();
    }

    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        url.searchParams.set(key, String(value));
      }
    });

    return url.toString();
  }

  private buildHeaders(options: RequestOptions): Headers {
    const headers = new Headers(this.defaultHeaders);

    headers.set('Accept', 'application/json');

    if (options.body !== undefined) {
      headers.set('Content-Type', 'application/json');
    }

    const currentUserId = options.currentUserId ?? this.currentUserId;

    if (currentUserId !== undefined) {
      headers.set('X-User-Id', currentUserId);
    }

    const correlationId = this.correlationIdFactory?.();

    if (correlationId !== undefined) {
      headers.set('X-Correlation-Id', correlationId);
    }

    new Headers(options.headers).forEach((value, key) => headers.set(key, value));

    return headers;
  }

  private async createApiError(response: Response): Promise<ApiError> {
    const body = await this.readJson(response);
    const errors: ErrorItemResponse[] = isErrorResponse(body)
      ? body.errors
      : [{ message: `Request failed with status ${response.status}.` }];

    return new ApiError(response.status, errors, response);
  }

  private async readJson(response: Response): Promise<unknown> {
    try {
      return await response.json();
    } catch {
      return undefined;
    }
  }
}
