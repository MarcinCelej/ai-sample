import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import '@/lib/i18n'
import { server } from './server'

// MSW intercepts requests at the network level; unknown requests fail the test.
beforeAll(() => {
  server.listen({ onUnhandledFrame: 'error' })
})
afterEach(() => {
  cleanup()
  server.resetHandlers()
  document.cookie.split('; ').forEach((cookie) => {
    document.cookie = `${cookie.split('=')[0] ?? ''}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`
  })
})
afterAll(() => {
  server.close()
})
