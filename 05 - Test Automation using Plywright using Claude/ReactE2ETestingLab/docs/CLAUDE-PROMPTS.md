# Claude Prompts

## Understand the Application

```text
Act as a senior React automation test engineer.

Analyze this React application.

Do not modify files.

Identify:
- major user workflows
- pages/states
- important user actions
- validations
- stable selectors
- automation test scenarios
- end-to-end test scenarios

Create a concise test strategy.
```

## Generate Login Tests

```text
Analyze the login workflow.

Create a compact test matrix covering:
- successful login
- empty credentials
- invalid credentials

Then generate Playwright tests.

Prefer accessible selectors or data-testid.
Do not modify application code unless a stable selector is genuinely missing.
```

## Generate Cart Tests

```text
Create Playwright automation tests for:
- search product
- no search result
- add product to cart
- increase quantity
- verify cart total
- remove product by setting quantity to zero

Keep each test focused.
```

## Generate Full E2E Test

```text
Generate one end-to-end Playwright test covering:

login
→ search product
→ add product
→ open cart
→ checkout
→ fill shipping details
→ place order
→ verify order confirmation

Use readable locators.
Avoid arbitrary waits.
```

## Failure Analysis

```text
This Playwright test failed.

Analyze the failure before changing code.

Classify it as:
- application defect
- selector issue
- timing/synchronization issue
- wrong test expectation
- test-data problem

Recommend the minimum fix.
```

## Final Review

```text
Review all Playwright tests as a principal QA automation engineer.

Check:
- brittle selectors
- unnecessary waits
- duplicated flows
- poor assertions
- test independence
- hidden dependencies
- missing negative cases
- maintainability

Return the 5 highest-value improvements.
```
