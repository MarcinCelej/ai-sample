import { isProblemDetails, type ProblemDetails } from '@/lib/problem-details'

/** The only place that calls `fetch`; used by the generated client (orval mutator). */

const ANTIFORGERY_COOKIE = 'XSRF-TOKEN'
const ANTIFORGERY_HEADER = 'X-XSRF-TOKEN'
const SAFE_METHODS = new Set(['GET', 'HEAD', 'OPTIONS'])

/** A non-2xx API response, carrying the parsed ProblemDetails. */
export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `Request failed with status ${String(status)}`)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }

  /** The stable error code to translate, if the API sent one. */
  get code(): string | undefined {
    return this.problem.code
  }
}

/** Error type used by the generated hooks (orval reads this export). */
export type ErrorType<_Error> = ApiError

export const http = async <T>(url: string, init: RequestInit = {}): Promise<T> => {
  const method = (init.method ?? 'GET').toUpperCase()
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (!SAFE_METHODS.has(method)) {
    const token = readCookie(ANTIFORGERY_COOKIE)
    if (token) {
      headers.set(ANTIFORGERY_HEADER, token)
    }
  }

  const response = await fetch(new URL(url, window.location.origin), {
    ...init,
    method,
    headers,
    credentials: 'same-origin',
  })
  const body = await readBody(response)

  if (!response.ok) {
    throw new ApiError(response.status, isProblemDetails(body) ? body : { status: response.status })
  }

  return body as T
}

async function readBody(response: Response): Promise<unknown> {
  const text = await response.text()
  if (!text) {
    return undefined
  }

  const contentType = response.headers.get('Content-Type') ?? ''
  return contentType.includes('json') ? (JSON.parse(text) as unknown) : text
}

function readCookie(name: string): string | undefined {
  const prefix = `${name}=`
  const cookie = document.cookie.split('; ').find((c) => c.startsWith(prefix))
  return cookie ? decodeURIComponent(cookie.slice(prefix.length)) : undefined
}
