import { defineConfig, devices } from '@playwright/test'

/**
 * E2E tests run against a running app: the API (with a database) and the SPA.
 * Locally: start the API and `npm run dev`, then `npm run e2e`. Override the URL with E2E_BASE_URL.
 */
export default defineConfig({
  testDir: 'e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: process.env.CI ? 'github' : 'list',
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5173',
    locale: 'pl-PL',
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
})
