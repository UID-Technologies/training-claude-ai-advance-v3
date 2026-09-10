import { test, expect } from '@playwright/test';

test('login page is displayed', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByTestId('login-page')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Login' })).toBeVisible();
});

// Intentionally minimal.
// Students will use Claude to generate the remaining automation and E2E tests.
