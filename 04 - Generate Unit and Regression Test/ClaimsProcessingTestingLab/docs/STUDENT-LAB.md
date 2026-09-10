# Student Lab – AI-Assisted Unit and Integration Testing with Claude

## Objective

Use Claude in VS Code to understand an existing .NET application and progressively build a useful automated test suite.

Do not ask Claude to generate every test in one step.

Use:

```text
Understand
  ↓
Test Strategy
  ↓
Test Matrix
  ↓
Generate Small Test Set
  ↓
Run
  ↓
Analyze Failure
  ↓
Fix Correct Layer
  ↓
Review
```

---

# LAB 1 – Use Claude to Discover Unit Tests

Open the repository in VS Code.

Run:

```bash
dotnet restore
dotnet build
dotnet test
```

Ask Claude:

```text
Act as a senior .NET test engineer.

Analyze this repository.

Do not modify code.

Identify:
- business rules
- business-logic classes
- testable units
- dependencies
- edge cases
- existing test gaps

Create a concise testing strategy.
```

Focus on:

```text
ClaimCalculator
ClaimValidator
ClaimService
```

Then ask:

```text
Create a unit-test matrix for ClaimCalculator and ClaimValidator.

Do not generate code yet.

For each test show:
Scenario
Input
Expected result
Boundary/edge case
```

Review the matrix.

Now ask Claude to generate only the tests for `ClaimCalculator`.

Run:

```bash
dotnet test tests/Claims.UnitTests
```

One preventive-treatment test should expose an intentional defect.

Do not tell Claude to make tests pass.

Ask:

```text
The preventive reimbursement test failed.

Compare the documented business rule, production code and test.

Determine whether the failure is a production defect or test defect.
Do not change anything yet.
```

After confirming the defect, ask Claude for the smallest production fix.

Expected rule:

```text
Preventive payable = min(claimed amount, 10000)
```

Run tests again.

---

# LAB 2 – Generate Service Tests Using Mocks

Now focus on:

```text
ClaimService
```

Ask Claude:

```text
Analyze ClaimService and identify its collaborators.

Tell me:
- which dependencies should be mocked
- which should use real implementation
- which behavior belongs in ClaimService tests
- which behavior is already covered by calculator/validator tests

Do not create tests yet.
```

Expected:

```text
Mock:
IClaimRepository
INotificationService

Real:
ClaimValidator
ClaimCalculator
```

Then ask Claude:

```text
Generate xUnit + Moq tests for ClaimService.

Cover:
1. successful submission
2. claim >= 100000 goes to ManualReview
3. repository AddAsync is called once
4. notification is called once
5. invalid request is not persisted
6. notification failure is propagated

Keep the tests focused on ClaimService behavior.
```

Run:

```bash
dotnet test tests/Claims.UnitTests
```

Review any failure with Claude before changing code.

---

# LAB 3 – Generate API Integration Tests

Explain to Claude:

```text
We now want integration tests for Claims.Api.

We want to verify the API, dependency injection, application service and persistence working together.

Use WebApplicationFactory.

Do not generate tests yet.

First create an integration-test matrix.
```

Target scenarios:

```text
GET /health                    → 200

POST valid claim               → 201

POST invalid amount            → 400

POST unsupported treatment     → 400

GET existing claim             → 200

GET unknown claim              → 404

POST >= 100000                 → ManualReview
```

Then ask:

```text
Implement the approved API integration tests using:

WebApplicationFactory<Program>
HttpClient
System.Net.Http.Json
xUnit

Keep the tests readable.
Do not mock the application service or repository.
```

Run:

```bash
dotnet test tests/Claims.IntegrationTests
```

Explain the difference:

```text
Unit Test

ClaimCalculator
      ↓
one isolated business unit


Integration Test

HTTP
 ↓
Controller
 ↓
ClaimService
 ↓
Repository
 ↓
In-memory database
```

---

# LAB 4 – Use Claude to Review and Improve the Test Suite

Ask Claude:

```text
Review all tests in:

tests/Claims.UnitTests
tests/Claims.IntegrationTests

Do not modify anything.

Find:
- missing edge cases
- duplicated tests
- weak assertions
- excessive mocks
- tests coupled to implementation
- test isolation issues
- important business risks without coverage

Return only the 10 highest-value improvements.
```

One important gap should be duplicate submission/idempotency.

Discuss:

```text
Same member
+
same treatment
+
same amount
+
same treatment date
+
retry
```

Could result in duplicate claims.

Ask Claude:

```text
Design tests that would expose duplicate claim submission.

Do not implement idempotency yet.

Explain:
1. unit-test scenario
2. integration-test scenario
3. expected business behavior
4. possible implementation strategies
```

The key lesson:

> Write the test that exposes the missing behavior before implementing the fix.

---

# Student Deliverables

Students should produce:

1. unit-test matrix
2. ClaimCalculator tests
3. ClaimValidator tests
4. ClaimService tests using Moq
5. API integration-test matrix
6. WebApplicationFactory integration tests
7. one failing test that exposed a real production defect
8. root-cause explanation
9. final test-review report from Claude
10. short explanation of unit vs integration testing

---

# Main Learning

Do not use Claude like this:

```text
Write all tests for my project.
```

Use Claude like this:

```text
Understand Code
     ↓
Discover Behavior
     ↓
Identify Boundaries
     ↓
Design Test Matrix
     ↓
Generate Focused Tests
     ↓
Execute
     ↓
Analyze Failures
     ↓
Fix Correct Layer
     ↓
Review Coverage
```
