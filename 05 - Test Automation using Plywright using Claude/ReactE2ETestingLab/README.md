# ShopSmart React Automation & E2E Testing Lab

A small React application designed for demonstrating automation and end-to-end testing with Claude in VS Code and Playwright.

## Scenario

ShopSmart is a simple e-commerce storefront.

User journey:

```text
Login
  ↓
Browse/Search Products
  ↓
Add to Cart
  ↓
Update Quantity
  ↓
Checkout
  ↓
Validate Address
  ↓
Place Order
  ↓
Order Confirmation
```

## Demo Credentials

```text
student@example.com
Password123!
```

## Run

```bash
npm install
npx playwright install
npm run dev
```

Open:

```text
http://localhost:5173
```

## Run Tests

```bash
npm run test:e2e
```

UI mode:

```bash
npm run test:e2e:ui
```

Headed mode:

```bash
npm run test:e2e:headed
```

## Structure

```text
src/
  App.tsx
  data.ts
  types.ts

tests/e2e/
  smoke.spec.ts

docs/
  STUDENT-LAB.md
  CLAUDE-PROMPTS.md
  TRAINER-NOTES.md
```

## Testing Opportunities

- successful login
- empty login
- invalid login
- product search
- no search result
- add to cart
- multiple items
- quantity changes
- total calculation
- remove item
- checkout validation
- invalid pincode
- successful order
- complete end-to-end purchase flow

The test suite is intentionally incomplete so students can use Claude to design and generate the missing Playwright tests.
