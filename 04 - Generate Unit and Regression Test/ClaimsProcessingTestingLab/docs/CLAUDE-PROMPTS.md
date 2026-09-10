# Claude Prompts for the Testing Demo

## 1. Understand Before Testing

```text
Act as a senior .NET test engineer.

Analyze this repository.
Do not modify code.

Identify:
1. business rules
2. classes containing business logic
3. external dependencies
4. unit-test candidates
5. integration-test candidates
6. important edge cases
7. code that is difficult to test

Return a test strategy before writing tests.
```

## 2. Generate Unit-Test Matrix

```text
Analyze ClaimCalculator and ClaimValidator.

Do not write tests yet.

Create a compact test matrix with:

Method
Scenario
Input
Expected result
Why important

Include:
happy path
boundary
invalid input
edge cases

Highlight any business rule that appears incorrectly implemented.
```

## 3. Generate Calculator Tests

```text
Generate xUnit tests for ClaimCalculator.

Requirements:
- follow Arrange / Act / Assert
- use descriptive test names
- avoid duplicate tests
- cover all reimbursement rules
- include boundary tests
- include the preventive ₹10,000 cap business rule

Do not modify production code even if a test fails.
```

## 4. Analyze Failing Test

```text
A generated test is failing.

Do not change anything yet.

Compare:
- business requirement
- production implementation
- test expectation

Classify the failure as:
1. production defect
2. incorrect test
3. ambiguous requirement

Explain the root cause and recommend the minimum correction.
```

## 5. Generate Service Tests with Mocks

```text
Analyze ClaimService.

Create unit tests using xUnit and Moq.

Mock only external collaborators:
- IClaimRepository
- INotificationService

Use real:
- ClaimValidator
- ClaimCalculator

Cover:
- successful submission
- manual review threshold
- repository save
- notification invocation
- validation failure
- repository failure
- notification failure

Before generating code, explain why each dependency is mocked or real.
```

## 6. Generate Integration-Test Plan

```text
Analyze Claims.Api.

Create an integration-test plan using WebApplicationFactory.

Do not generate code yet.

Cover:
- health endpoint
- POST successful claim
- GET created claim
- invalid amount returns 400
- unsupported treatment returns 400
- missing claim returns 404
- manual review response

Explain what is being tested end-to-end versus mocked.
```

## 7. Generate Integration Tests

```text
Implement the integration tests from our approved plan.

Use:
- WebApplicationFactory<Program>
- HttpClient
- System.Net.Http.Json
- xUnit

Do not call external services.
Keep the tests independent and readable.
```

## 8. Test Review

```text
Act as a principal test engineer.

Review all generated unit and integration tests.

Check:
- tests that assert implementation details
- fragile tests
- missing boundary cases
- false positives
- excessive mocking
- duplicated coverage
- tests that modify production behavior
- integration-test isolation

Classify findings:
Major
Minor

Do not modify files yet.
```
