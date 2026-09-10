# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: smoke.spec.ts >> login page is displayed
- Location: tests\e2e\smoke.spec.ts:3:1

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: getByTestId('login-page')
Expected: visible
Timeout: 5000ms
Error: element(s) not found

Call log:
  - Expect "toBeVisible" getByTestId('login-page') with timeout 5000ms
  - waiting for getByTestId('login-page')

```

# Test source

```ts
  1  | import { test, expect } from '@playwright/test';
  2  | 
  3  | test('login page is displayed', async ({ page }) => {
  4  |   await page.goto('/');
  5  | 
> 6  |   await expect(page.getByTestId('login-page')).toBeVisible();
     |                                                ^ Error: expect(locator).toBeVisible() failed
  7  |   await expect(page.getByRole('heading', { name: 'Login' })).toBeVisible();
  8  | });
  9  | 
  10 | // Intentionally minimal.
  11 | // Students will use Claude to generate the remaining automation and E2E tests.
  12 | 
```