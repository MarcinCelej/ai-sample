/** ProblemDetails (RFC 9457) as returned by the ClassBook API. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  /** Stable error code, translated by the frontend, e.g. `course.full`. */
  code?: string
  /** Validation messages per camelCase field (English, for developers). */
  errors?: Record<string, string[]>
  /** Validation error codes per camelCase field, translated by the frontend. */
  errorCodes?: Record<string, string[]>
  traceId?: string
}

export function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === 'object' && value !== null && ('status' in value || 'title' in value || 'code' in value)
}
