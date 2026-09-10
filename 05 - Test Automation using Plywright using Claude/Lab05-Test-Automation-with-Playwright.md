# Lab 05 — Test Automation with Playwright and Claude

**Hands-on lab · VS Code + Claude Code · Application: `ReactE2ETestingLab` (ShopSmart)**

| | |
|---|---|
| **Duration** | 3–4 hours · 13 tasks (4 exercises) |
| **Level** | Intermediate |
| **Stack** | React 18, TypeScript, Vite, Vitest, React Testing Library, Playwright |
| **Tools** | VS Code 1.94+, Claude Code, Node.js 18+, Git |

## Description

Use Claude to build a **test pyramid** for a React e-commerce app: fast unit/component tests with Vitest,
browser automation with Playwright, and one high-value end-to-end purchase journey. Claude generates —
you review, run, debug and decide.

## Business scenario

> **ShopSmart** is a small e-commerce storefront. Users log in, search products, add to cart, adjust
> quantity, checkout with delivery details, and receive an order confirmation. QA has one smoke test.
> Your job: design and implement the missing automated test coverage with Claude as your pair.

## User journey

```text
Login → Browse/Search → Add to Cart → Change Quantity → Checkout → Place Order → Order Confirmed
```

**Demo credentials:** `student@example.com` / `Password123!`

**Key product:** Wireless Mouse — ₹899 (product id `1`, test id `add-1`)

## What you will produce

| # | Deliverable | Task |
|---|---|---|
| 1 | Test strategy (unit vs Playwright split) | 1 |
| 2 | Vitest + RTL login component tests | 3–4 |
| 3 | Playwright login + search tests | 6–7 |
| 4 | Cart + checkout automation tests | 9–10 |
| 5 | Complete purchase-flow E2E test | 11 |
| 6 | One failure root-cause analysis | 12 |
| 7 | Final Claude test-review summary | 13 |

---

## Prerequisites

```powershell
node --version    # 18+
npm --version
git --version
```

Install **Claude Code** in VS Code. **Plan** mode for analysis; **Manual** for generating test files.

---

# Task 0 — Setup (20 min)

### 0.1 Copy to a safe path (important)

Vite **breaks when the project path contains `#`**. If your training folder is under `F:\#Training\...`,
copy the lab first:

```powershell
Copy-Item -Recurse `
  "F:\#Training\training-claude-ai-advance-v3\05 - Test Automation using Plywright using Claude\ReactE2ETestingLab" `
  "C:\labs\ReactE2ETestingLab"

cd C:\labs\ReactE2ETestingLab
code .
```

> Work from `C:\labs\ReactE2ETestingLab` for the rest of this lab. Adjust if you use another path — just
> **no `#` characters** in the full path.

### 0.2 Remove trainer spoilers

```powershell
Move-Item .\docs "$env:USERPROFILE\Desktop\lab05-trainer-docs"
```

### 0.3 Install and verify manually

```powershell
npm install
npx playwright install chromium
npm run dev
```

Open `http://localhost:5173` and verify manually:

1. Login with demo credentials → Products page
2. Search "Mouse" → Wireless Mouse appears
3. Add to cart → Cart count increases
4. Checkout → form appears
5. Fill valid details + 6-digit pincode → Order Confirmed with `ORD-...`

Stop the dev server (`Ctrl+C`) — Playwright will start it automatically via `webServer` in
`playwright.config.ts`.

Run the existing smoke test:

```powershell
npm run test:e2e
```

Expected: `1 passed` — `login page is displayed`.

If it fails with `Failed to load url /src/main.tsx`, your path still contains `#`. Copy to `C:\labs\`.

### 0.4 Git + engineering contract

```powershell
git init
git add .
git commit -m "baseline: ShopSmart with smoke e2e test"
```

Run `/init`, replace `CLAUDE.md` with [Appendix A](#appendix-a--claudemd). Confirm with `/context`.

**Check:** smoke test passes, app works in browser, `docs/` removed.

---

# EXERCISE 1 — Unit & Component Tests (Vitest + RTL)

> The project has Playwright but **no Vitest yet**. You configure it in Task 2.

## Task 1 — Test strategy (10 min · Plan)

**Prompt:**

```text
Act as a senior React test automation engineer.

Analyze @src/App.tsx and the overall application structure. Do not modify files.

Identify:
1. major UI behaviors and business rules
2. validation logic (login, checkout, pincode)
3. behaviors suitable for Vitest + React Testing Library (fast, isolated)
4. behaviors better suited for Playwright (browser workflows)
5. important edge cases

Create a short test strategy: what gets unit tested vs what gets E2E tested, and why.
Do not generate test code yet.
```

**Expected split:**

| Vitest (fast) | Playwright (browser) |
|---|---|
| Login validation messages | Full login → shop navigation |
| Empty credentials error | Product search + results |
| Invalid credentials error | Add to cart + cart count in header |
| (optional) pincode regex logic | Quantity change + total display |
| | Checkout form + order confirmation |

---

## Task 2 — Configure Vitest (15 min · Manual)

**Prompt:**

```text
Configure this React + Vite project for unit/component testing.

Use: Vitest, React Testing Library, @testing-library/jest-dom, @testing-library/user-event, jsdom.

Make only the minimum required changes:
- add devDependencies to package.json
- add "test" and "test:watch" scripts
- update vite.config.ts with a test block (environment: jsdom, globals if needed)
- create src/test/setup.ts importing @testing-library/jest-dom

After changes, explain packages added, scripts, and how to run tests.
Do not write test cases yet.
```

```powershell
npm install
npm test
```

Expected: Vitest runs with **0 tests** (or only if you added a placeholder) — no errors.

```powershell
git add . ; git commit -m "chore: configure Vitest and React Testing Library"
```

---

## Task 3 — Login test matrix + unit tests (25 min · Manual)

**Prompt 1 — matrix:**

```text
Analyze login behavior in @src/App.tsx.

Create a test matrix covering:
1. login page renders (heading visible)
2. empty credentials → "Email and password are required"
3. wrong credentials → "Invalid credentials"
4. correct credentials → shop page visible (data-testid="shop-page")

Do not generate code yet.
```

**Prompt 2 — generate tests:**

```text
Create src/App.login.test.tsx with Vitest + React Testing Library tests for the approved login matrix.

Requirements:
- render <App /> for each test
- use userEvent for typing and clicking
- prefer getByRole, getByLabelText, or getByTestId — match what exists in App.tsx
- assert user-visible outcomes, not internal React state
- do not modify production code
- keep tests independent (each renders fresh App)
```

```powershell
npm test
```

Expected: **4 passing** unit tests.

```powershell
git add src/App.login.test.tsx
git commit -m "test: Vitest login component tests"
```

**Deliverable 2.**

---

## Task 4 — Review unit tests (10 min · Plan)

```text
Review the Vitest login tests in src/App.login.test.tsx.

Check for: implementation-detail testing, weak assertions, unnecessary mocks,
brittle selectors, duplicated setup.

Recommend only important improvements. Do not modify anything.
```

---

# EXERCISE 2 — Playwright: Login & Search

Playwright is pre-configured in `playwright.config.ts` with `baseURL: http://127.0.0.1:5173` and auto
`webServer`. You do not start `npm run dev` manually for e2e runs.

## Task 5 — Login automation matrix (10 min · Plan)

**Prompt:**

```text
Analyze the ShopSmart login workflow for Playwright browser automation.

Create a test matrix covering:
1. successful login → shop page visible
2. empty login → validation error visible
3. invalid credentials → "Invalid credentials"

For each: user actions, preferred selector (role/label/test-id), expected visible result.
Do not generate code yet.
```

---

## Task 6 — Generate login Playwright tests (20 min · Manual)

**Prompt:**

```text
Create tests/e2e/login.spec.ts with Playwright tests for the approved login scenarios.

Requirements:
- use @playwright/test
- prefer page.getByTestId, getByRole, getByLabel — avoid CSS selectors
- no waitForTimeout or arbitrary sleeps
- verify visible business outcomes
- each test starts at / and is fully independent
- keep the existing smoke.spec.ts unchanged
```

```powershell
npx playwright test tests/e2e/login.spec.ts
npx playwright test tests/e2e/login.spec.ts --headed
```

Expected: 3 passing login tests (+ smoke still passes).

```powershell
git add tests/e2e/login.spec.ts
git commit -m "test: Playwright login automation"
```

---

## Task 7 — Product search tests (20 min · Manual)

**Prompt:**

```text
Create tests/e2e/search.spec.ts for product search.

Cover:
1. login with valid credentials
2. search for "Mouse" → Wireless Mouse is visible
3. search for "NOTFOUND999" → "No products found" (data-testid="no-products")

Use a helper function login(page) at the top of the file to avoid duplicating login steps in every test.
No waitForTimeout.
```

```powershell
npx playwright test tests/e2e/search.spec.ts
npm run test:e2e
```

**Deliverable 3 (partial).** **Check:** all e2e tests green.

---

# EXERCISE 3 — Cart & Checkout Automation

## Task 8 — Cart test plan (10 min · Plan)

**Prompt:**

```text
Analyze the cart flow in @src/App.tsx.

Create a compact Playwright test plan:
1. login → search "Mouse" → add Wireless Mouse
2. verify cart count in header shows 1
3. open cart page
4. change quantity to 2
5. verify total is ₹1,798 (899 × 2)
6. proceed to checkout → checkout page visible

Do not generate code yet.
```

---

## Task 9 — Generate cart test (20 min · Manual)

**Prompt:**

```text
Create tests/e2e/cart.spec.ts for the approved cart flow.

Use data-testid values from App.tsx:
- add-1 (Wireless Mouse), cart-total, checkout-button, search, etc.

Expected total: ₹1,798 for quantity 2.
Reuse a login(page) helper.
No waitForTimeout.
```

```powershell
npx playwright test tests/e2e/cart.spec.ts --headed
```

---

## Task 10 — Checkout validation test (20 min · Manual)

**Prompt:**

```text
Create tests/e2e/checkout.spec.ts for checkout validation.

Flow:
1. login → add Wireless Mouse → open cart → proceed to checkout
2. submit empty form → verify "All checkout fields are required"
3. fill Full Name, Address, City; enter pincode "ABC123" → submit
4. verify "Pincode must be exactly 6 digits"

Use getByTestId for form fields: fullName, address, city, pincode, place-order.
```

```powershell
npx playwright test tests/e2e/checkout.spec.ts
npm run test:e2e
git add tests/e2e/
git commit -m "test: Playwright cart and checkout automation"
```

**Deliverable 4.**

---

# EXERCISE 4 — End-to-End Purchase & Failure Analysis

## Task 11 — Complete E2E purchase test (25 min · Manual)

**Prompt 1 — design:**

```text
Design one end-to-end Playwright scenario for ShopSmart:

1. login (student@example.com / Password123!)
2. search Wireless Mouse → add to cart
3. open cart → verify item and ₹899 total (qty 1)
4. proceed to checkout
5. enter: Test Student / 10 Test Street / Delhi / 110001
6. place order
7. verify "Order Confirmed" and order number matching /^ORD-\d{6}$/

List the assertion at each business step. Do not generate code yet.
```

**Prompt 2 — implement:**

```text
Create tests/e2e/purchase-flow.spec.ts for the approved E2E scenario.

Requirements:
- descriptive test name
- stable selectors from data-testid
- business-focused assertions at each step
- no waitForTimeout, no test dependencies
- single test method covering the full journey
```

```powershell
npx playwright test tests/e2e/purchase-flow.spec.ts --headed
```

**Deliverable 5.**

---

## Task 12 — Failure analysis exercise (15 min)

Intentionally break an assertion in `cart.spec.ts`:

```typescript
await expect(page.getByTestId('cart-total')).toContainText('₹1,999');  // wrong — should be ₹1,798
```

```powershell
npx playwright test tests/e2e/cart.spec.ts
```

Copy the failure output. **Plan mode:**

```text
This Playwright test failed:

<PASTE FULL FAILURE>

Do not modify anything yet.

Classify as:
1. application defect
2. incorrect test expectation
3. selector/locator problem
4. timing/synchronization problem
5. test-data issue

Explain root cause and recommend the minimum correction.
```

Fix the assertion, re-run, confirm green.

```powershell
git add . ; git commit -m "test: complete E2E purchase flow"
```

**Deliverable 6.**

---

## Task 13 — Final review (15 min · Plan)

`/clear` or new tab:

```text
Act as a principal QA automation engineer.

Review all tests under tests/e2e/ and src/*.test.tsx.

Do not modify anything.

Identify the 5 most important issues. Check for:
- brittle selectors
- waitForTimeout usage
- duplicated login steps without a helper
- weak assertions
- test dependencies
- unnecessary E2E coverage that belongs in unit tests
- missing negative tests

For each: Severity, Problem, Recommended improvement.

Then review the git diff and recommend: ACCEPT / ACCEPT WITH CHANGES / REJECT.
```

Final run:

```powershell
npm test
npm run test:e2e
```

**Deliverable 7.**

---

# Appendix A — `CLAUDE.md`

```markdown
# ShopSmart — Test Automation AI Contract

Act as a senior React and Playwright automation engineer.

## Project
- React 18 + TypeScript + Vite
- Single-page app: src/App.tsx (login, shop, cart, checkout, success views)
- Demo user: student@example.com / Password123!
- Wireless Mouse: ₹899, product id 1, add button data-testid="add-1"
- Vitest + RTL: npm test
- Playwright e2e: npm run test:e2e (webServer auto-starts Vite on port 5173)

## Rules
1. Inspect App.tsx before generating tests.
2. Create test scenarios/matrix before test code.
3. Prefer getByRole, getByLabel, getByTestId — avoid CSS/XPath.
4. Never use waitForTimeout unless absolutely necessary.
5. Keep Playwright tests independent — each can start from /.
6. Use unit tests for validation logic; Playwright for browser workflows.
7. Only a few high-value full E2E tests — not everything end-to-end.
8. Do not modify production code because a test fails — diagnose first.
9. Reuse a login(page) helper in Playwright specs to reduce duplication.
10. Review generated tests critically before considering them done.
```

---

# Appendix B — Selector reference (from App.tsx)

| Element | Selector |
|---|---|
| Login page | `getByTestId('login-page')` |
| Email / Password | `getByTestId('email')` / `getByTestId('password')` |
| Login button | `getByTestId('login-button')` |
| Shop page | `getByTestId('shop-page')` |
| Search | `getByTestId('search')` |
| No products | `getByTestId('no-products')` |
| Add Wireless Mouse | `getByTestId('add-1')` |
| Cart total | `getByTestId('cart-total')` |
| Checkout button | `getByTestId('checkout-button')` |
| Checkout fields | `fullName`, `address`, `city`, `pincode` |
| Place order | `getByTestId('place-order')` |
| Success page | `getByTestId('success-page')` |
| Order number | `getByTestId('order-number')` |

**Login helper pattern:**

```typescript
async function login(page: import('@playwright/test').Page) {
  await page.goto('/');
  await page.getByTestId('email').fill('student@example.com');
  await page.getByTestId('password').fill('Password123!');
  await page.getByTestId('login-button').click();
  await expect(page.getByTestId('shop-page')).toBeVisible();
}
```

---

# Appendix C — Troubleshooting

| Symptom | Fix |
|---|---|
| Vite warning about `#` in path; blank page; e2e fails | Copy project to `C:\labs\ReactE2ETestingLab` (no `#` in path) |
| `Failed to load url /src/main.tsx` | Same — path issue with `#` |
| Playwright timeout, login-page not found | Fix path first; then check `npm run dev` works manually |
| `npx playwright install` not run | Run `npx playwright install chromium` |
| Vitest: `document is not defined` | Set `environment: 'jsdom'` in vite.config.ts test block |
| Vitest: cannot find `@testing-library/jest-dom` | Run `npm install` after Claude adds packages |
| E2E passes locally but selector fails | Use `npx playwright test --headed` and inspect with `--debug` |
| Cart total assertion wrong | Wireless Mouse = ₹899; qty 2 = **₹1,798** not ₹1,999 |
| Order number assertion fails | Format is `ORD-` + last 6 digits of timestamp — use regex `/^ORD-\d{6}$/` |
| Claude generates `waitForTimeout(3000)` | Reject — use `expect(locator).toBeVisible()` auto-wait instead |

---

# Appendix D — Command summary

```powershell
# setup (use path WITHOUT #)
Copy-Item -Recurse "<source>\ReactE2ETestingLab" "C:\labs\ReactE2ETestingLab"
cd C:\labs\ReactE2ETestingLab
Move-Item .\docs "$env:USERPROFILE\Desktop\lab05-trainer-docs"
npm install
npx playwright install chromium

# manual check
npm run dev          # http://localhost:5173

# unit tests (after Vitest configured)
npm test
npm run test:watch

# e2e (webServer starts Vite automatically)
npm run test:e2e
npm run test:e2e:headed
npm run test:e2e:ui
npx playwright test tests/e2e/login.spec.ts
npx playwright show-report

# loop
git diff
git add . ; git commit -m "..."
```

> **The rule:** Claude accelerates test design and generation. You own what gets tested, whether the test
> is correct, and whether a failure means a bug in the app or a bug in the test.
