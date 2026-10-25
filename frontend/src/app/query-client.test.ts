import { ApiError } from '@/api/http'
import { createQueryClient } from './query-client'

describe('createQueryClient', () => {
  const retry = createQueryClient().getDefaultOptions().queries?.retry as (count: number, error: unknown) => boolean

  it('does not retry client errors', () => {
    expect(retry(0, new ApiError(404, { status: 404 }))).toBe(false)
  })

  it('retries server and network errors twice', () => {
    expect(retry(0, new ApiError(503, { status: 503 }))).toBe(true)
    expect(retry(1, new TypeError('Failed to fetch'))).toBe(true)
    expect(retry(2, new TypeError('Failed to fetch'))).toBe(false)
  })
})
