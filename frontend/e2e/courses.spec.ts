import AxeBuilder from '@axe-core/playwright'
import { expect, test } from '@playwright/test'

test('course list opens from the start page and has no serious accessibility violations', async ({ page }) => {
  await page.goto('/')

  await expect(page).toHaveURL(/\/courses$/)
  await expect(page.getByRole('heading', { name: 'Kursy' })).toBeVisible()

  const results = await new AxeBuilder({ page }).analyze()
  const serious = results.violations.filter((v) => v.impact === 'serious' || v.impact === 'critical')
  expect(serious).toEqual([])
})
