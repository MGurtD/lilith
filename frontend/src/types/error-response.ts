/**
 * RFC 7807 Problem Details response from backend ErrorHandlerMiddleware
 * Matches the ErrorResponse model from lilith-backend
 */
export interface ErrorResponse {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance: string;
  traceId: string;
  errors: string[];
  timestamp: string;
}

/**
 * Legacy GenericResponse format (maintained for backward compatibility)
 */
export interface GenericResponse<T = any> {
  result: boolean;
  errors: string[];
  content?: T;
}

/**
 * Unified error information extracted from API responses
 */
export interface ParsedErrorInfo {
  message: string;
  errors: string[];
  statusCode: number;
  traceId?: string;
  path?: string;
  severity: "error" | "warn" | "info";
  isNetworkError: boolean;
}
