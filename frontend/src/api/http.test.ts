import { http as mswHttp, HttpResponse } from 'msw'
import { server } from '@/test/server'
import { ApiError, http } from './http'

describe('http', () => {
  it('returns the parsed JSON body of a successful response', async () => {
    server.use(mswHttp.get('*/api/things', () => HttpResponse.json({ id: 1 })))

    await expect(http('/api/things')).resolves.toEqual({ id: 1 })
  })

  it('returns undefined for an empty response and text for non-JSON content', async () => {
    server.use(
      mswHttp.delete('*/api/things/1', () => new HttpResponse(null, { status: 204 })),
      mswHttp.get('*/api/text', () => HttpResponse.text('plain')),
    )

    await expect(http('/api/things/1', { method: 'DELETE' })).resolves.toBeUndefined()
    await expect(http('/api/text')).resolves.toBe('plain')
  })

  it('sends the CSRF token from the cookie on unsafe requests only', async () => {
    document.cookie = 'XSRF-TOKEN=token%2Fvalue'
    const seen: Record<string, string | null> = {}
    server.use(
      mswHttp.all('*/api/things', ({ request }) => {
        seen[request.method] = request.headers.get('X-XSRF-TOKEN')
        return HttpResponse.json({})
      }),
    )

    await http('/api/things', { method: 'POST', body: '{}' })
    await http('/api/things')

    expect(seen).toEqual({ POST: 'token/value', GET: null })
  })

  it('sends no CSRF header when there is no token cookie', async () => {
    let header: string | null = 'not called'
    server.use(
      mswHttp.post('*/api/things', ({ request }) => {
        header = request.headers.get('X-XSRF-TOKEN')
        return HttpResponse.json({})
      }),
    )

    await http('/api/things', { method: 'post' })

    expect(header).toBeNull()
  })

  it('throws ApiError with the ProblemDetails of a failed response', async () => {
    server.use(
      mswHttp.post('*/api/courses', () =>
        HttpResponse.json(
          { status: 400, title: 'Invalid', code: 'validation.failed', errorCodes: { name: ['course.name.required'] } },
          { status: 400, headers: { 'Content-Type': 'application/problem+json' } },
        ),
      ),
    )

    const error = await http('/api/courses', { method: 'POST' }).catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    const apiError = error as ApiError
    expect(apiError.status).toBe(400)
    expect(apiError.code).toBe('validation.failed')
    expect(apiError.problem.errorCodes).toEqual({ name: ['course.name.required'] })
    expect(apiError.message).toBe('Invalid')
  })

  it('throws ApiError with only the status when the error body is not ProblemDetails', async () => {
    server.use(mswHttp.get('*/api/broken', () => HttpResponse.text('Bad gateway', { status: 502 })))

    const error = (await http('/api/broken').catch((e: unknown) => e)) as ApiError

    expect(error.status).toBe(502)
    expect(error.problem).toEqual({ status: 502 })
    expect(error.code).toBeUndefined()
    expect(error.message).toBe('Request failed with status 502')
  })
})
