import { QueryClient } from '@tanstack/react-query'
import { ApiError } from '@/api/http'

/** Client errors (4xx) are not retried: repeating the same request gives the same answer. */
export function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        retry: (failureCount, error) =>
          !(error instanceof ApiError && error.status >= 400 && error.status < 500) && failureCount < 2,
      },
    },
  })
}
