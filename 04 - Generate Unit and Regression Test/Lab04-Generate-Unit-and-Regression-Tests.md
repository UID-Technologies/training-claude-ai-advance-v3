# Lab 04 — Generate Unit and Regression Tests with Claude

**Hands-on lab · VS Code + Claude Code · Application: `ClaimsProcessingTestingLab` (.NET 8)**

| | |
|---|---|
| **Duration** | 3 hours · 11 tasks (4 exercises) |
| **Level** | Intermediate |
| **Stack** | .NET 8, ASP.NET Core, xUnit, Moq, WebApplicationFactory |
| **Tools** | VS Code 1.94+, Claude Code, Git |

## Description

You inherited a **Claims Processing API** with a handful of stub tests. You do not know whether the
important business rules are protected. Use Claude to **understand rules → design a test matrix →
generate tests → run → analyze failures → review coverage** — without blindly changing production code.

## Business scenario

> A health insurer processes member reimbursement claims. Each claim has a member ID, treatment type,
> amount, treatment date and emergency flag. The platform validates, calculates payable amount, stores
> the claim and exposes REST APIs. QA says "we have tests" but nobody trusts the coverage.

## Business rules (from requirements)

| # | Rule |
|---|---|
| 1 | Member ID required |
| 2 | Amount > 0 |
| 3 | Amount ≤ ₹500,000 |
| 4 | Treatment date cannot be in the future |
| 5 | Outpatient → 80% reimbursement |
| 6 | Emergency → 90% (overrides treatment type) |
| 7 | Preventive → 100%, **capped at ₹10,000** |
| 8 | Dental → 60% |
| 9 | Unsupported treatment type → rejected |
| 10 | Claims ≥ ₹100,000 → `ManualReview` status |

## What you will produce

| # | Deliverable | Task |
|---|---|---|
| 1 | Unit-test matrix for `ClaimCalculator` + `ClaimValidator` | 2 |
| 2 | xUnit tests (calculator + validator) | 3–4 |
| 3 | Moq tests for `ClaimService` | 6 |
| 4 | API integration tests (`WebApplicationFactory`) | 8 |
| 5 | One failed-test root-cause analysis | 4 |
| 6 | Final Claude test-review summary | 10 |

---

## Prerequisites

```powershell
dotnet --list-sdks    # 8.0.x or 9.0.x
git --version
```

Install **Claude Code** in VS Code. Open with Spark icon (✱) or `Ctrl+Shift+Esc`.

**Permission modes:** **Plan** for analysis/matrix/review. **Manual** for generating test code and production fixes.

---

# Task 0 — Setup (15 min)

### 0.1 Remove trainer spoilers

```powershell
cd "F:\#Training\training-claude-ai-advance-v3\04 - Generate Unit and Regression Test\ClaimsProcessingTestingLab"
Move-Item .\docs "$env:USERPROFILE\Desktop\lab04-trainer-docs"
```

> Read `README.md` **Business Rules** section only. **Do not read** "Intentional Training Characteristics" until after Task 4.

The repo ships with **stub tests** — one test per class, marked incomplete. You will expand them with Claude.

### 0.2 Fix the build

```powershell
dotnet build .\ClaimsProcessingTestingLab.sln
```

Same trap as Labs 01–03: `Unable to find a project to restore!` then fake success. Fix:

```powershell
Remove-Item .\ClaimsProcessingTestingLab.sln -Force
dotnet new sln -n ClaimsProcessingTestingLab
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
dotnet build .\ClaimsProcessingTestingLab.sln
```

Add to **both** test projects (`tests\Claims.UnitTests\Claims.UnitTests.csproj` and `tests\Claims.IntegrationTests\Claims.IntegrationTests.csproj`):

```xml
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
```

Verify baseline (2 unit + 1 integration test):

```powershell
dotnet test .\ClaimsProcessingTestingLab.sln
```

Expected: `Claims.UnitTests: Passed 2` · `Claims.IntegrationTests: Passed 1`

### 0.3 Git + engineering contract

```powershell
code .
git init
git add .
git commit -m "baseline: Claims API with stub tests"
```

Run `/init`, replace `CLAUDE.md` with [Appendix A](#appendix-a--claudemd). Confirm with `/context`.

```powershell
git add . ; git commit -m "build: repair solution and test projects"
```

**Check:** 3 tests pass, stub files still have `// Intentionally incomplete` comments.

---

# EXERCISE 1 — Unit Tests: Calculator & Validator

## Task 1 — Analyze business logic (10 min · Plan)

Open:

```text
src/Claims.Application/ClaimCalculator.cs
src/Claims.Application/ClaimValidator.cs
src/Claims.Domain/TreatmentTypes.cs
```

**Prompt:**

```text
Act as a senior .NET test engineer.

Analyze @src/Claims.Application/ClaimCalculator.cs and @src/Claims.Application/ClaimValidator.cs.
Do not modify code.

Identify:
- business rules implemented in each class
- happy-path scenarios
- boundary conditions
- invalid inputs
- possible defects (compare implementation to the README business rules)

Create a concise unit-test matrix with columns:
| Class | Scenario | Input | Expected outcome |

Do not generate test code yet.
```

**Expected matrix highlights:**

| Scenario | Expected |
|---|---|
| Outpatient ₹1,000 | ₹800 |
| Dental ₹1,000 | ₹600 |
| Emergency ₹1,000 | ₹900 |
| Preventive ₹5,000 | ₹5,000 |
| Preventive ₹15,000 | **₹10,000 (cap)** |
| Zero amount | `ArgumentException` |
| Above ₹500,000 | `ArgumentException` |
| Future treatment date | `ArgumentException` |
| Unsupported treatment type | `ArgumentException` |

---

## Task 2 — Confirm the matrix yourself (5 min)

Skim `README.md` Business Rules and tick off each row in your matrix. Add any scenario Claude missed.

**Deliverable 1.**

---

## Task 3 — Generate unit tests (25 min · Manual)

**Prompt:**

```text
Generate xUnit tests for ClaimCalculator and ClaimValidator using the test matrix.

Requirements:
- add tests to the existing files:
  tests/Claims.UnitTests/ClaimCalculatorTests.cs
  tests/Claims.UnitTests/ClaimValidatorTests.cs
- keep the existing passing tests
- use Arrange / Act / Assert
- use descriptive test method names
- use [Theory] / [InlineData] where inputs repeat the same rule
- cover boundaries (0, max amount, cap, future date, unsupported type)
- do NOT modify production code

For ClaimValidator, pass a fixed DateTime.UtcNow as the second parameter to Validate().
```

Run unit tests only:

```powershell
dotnet test .\tests\Claims.UnitTests\Claims.UnitTests.csproj
```

**Expected:** at least one test **fails** — the Preventive cap (₹15,000 → should be ₹10,000, code returns ₹15,000).

If all tests pass, your matrix missed the cap rule. Add:

```csharp
[Fact]
public void Calculate_PreventiveAboveCap_ReturnsMaximumTenThousand()
{
    var calculator = new ClaimCalculator();
    var result = calculator.Calculate(TreatmentTypes.Preventive, 15_000m, false);
    Assert.Equal(10_000m, result);
}
```

Re-run — it should fail: `Expected: 10000  Actual: 15000`.

```powershell
git add tests/
git commit -m "test: unit tests for ClaimCalculator and ClaimValidator"
```

---

## Task 4 — Analyze failure and fix the defect (20 min)

Copy the failing output. **Plan mode first:**

```text
This test failed:

<PASTE FULL FAILURE OUTPUT>

Do not change anything yet.

Compare:
- the business rule (Preventive 100% up to ₹10,000)
- the test expectation
- the current implementation in ClaimCalculator

Tell me whether this is:
1. production defect
2. test defect
3. unclear requirement

Explain the root cause with file and line numbers.
```

**Expected verdict:** production defect — `ClaimCalculator` returns full `claimedAmount` for Preventive with no cap.

Switch to **Manual** and fix with the smallest change:

```text
Apply the minimum fix to @src/Claims.Application/ClaimCalculator.cs so Preventive reimbursement
is capped at ₹10,000. Do not refactor anything else. Explain why the test now passes.
```

**Expected fix:**

```csharp
TreatmentTypes.Preventive =>
    decimal.Round(Math.Min(claimedAmount, 10_000m), 2),
```

```powershell
dotnet test .\tests\Claims.UnitTests\Claims.UnitTests.csproj
git add .
git commit -m "fix: cap preventive reimbursement at 10000 (found by unit test)"
```

**Deliverable 2 + 5.** **Check:** all unit tests green.

---

# EXERCISE 2 — Service Tests with Moq

## Task 5 — Analyze what to mock (10 min · Plan)

Open `src/Claims.Application/ClaimService.cs`.

**Prompt:**

```text
Analyze @src/Claims.Application/ClaimService.cs from a unit-testing perspective.

Identify:
- dependencies injected via constructor
- which should be mocked and which should be real implementations
- why mocking everything would be a bad idea here
- 5 important service-level test scenarios

Do not generate tests yet.
```

**Expected:**

```text
Mock:     IClaimRepository, INotificationService
Real:     ClaimValidator, ClaimCalculator

Scenarios:
1. successful submission → Submitted status, correct payable
2. amount >= 100,000 → ManualReview status
3. invalid request → nothing saved, no notification
4. repository AddAsync called exactly once on success
5. notification SendClaimSubmittedAsync called exactly once on success
```

---

## Task 6 — Generate ClaimService tests (25 min · Manual)

**Prompt:**

```text
Create tests/Claims.UnitTests/ClaimServiceTests.cs using xUnit and Moq.

Cover only:
1. successful claim submission (Submitted status, payable calculated correctly)
2. claim >= ₹100,000 becomes ManualReview
3. IClaimRepository.AddAsync called exactly once on success
4. INotificationService.SendClaimSubmittedAsync called exactly once on success
5. invalid claim (zero amount) — AddAsync and SendClaimSubmittedAsync are NEVER called

Use real ClaimValidator and ClaimCalculator instances.
Mock IClaimRepository and INotificationService.
Do not modify production code.

Before creating the file, show the mock setup for each scenario.
```

Run:

```powershell
dotnet test .\tests\Claims.UnitTests\Claims.UnitTests.csproj
```

**Common Moq patterns to verify in the generated code:**

```csharp
_repoMock.Verify(r => r.AddAsync(It.IsAny<Claim>(), It.IsAny<CancellationToken>()), Times.Once);
_repoMock.Verify(r => r.AddAsync(It.IsAny<Claim>(), It.IsAny<CancellationToken>()), Times.Never);
_notificationMock.Verify(n => n.SendClaimSubmittedAsync(It.IsAny<Claim>(), It.IsAny<CancellationToken>()), Times.Once);
```

```powershell
git add tests/Claims.UnitTests/ClaimServiceTests.cs
git commit -m "test: ClaimService unit tests with Moq"
```

**Deliverable 3.** **Check:** all unit tests pass (expect ~10+ total).

---

# EXERCISE 3 — API Integration Tests

## Task 7 — Integration test matrix (10 min · Plan)

**Prompt:**

```text
Analyze the Claims API (@src/Claims.Api/Controllers/ClaimsController.cs and @src/Claims.Api/Program.cs).

Create a concise integration-test matrix using WebApplicationFactory<Program>.

Cover only:
1. GET /health
2. POST valid claim → 201 Created with body
3. POST invalid amount (zero) → 400 Bad Request
4. GET created claim by id → 200 OK
5. GET unknown claim id → 404 Not Found

For each row: endpoint, request body (if any), expected HTTP status, key assertion.
Do not generate code yet.
```

**Sample valid POST body** (from README):

```json
{
  "memberId": "MEM-1001",
  "treatmentType": "Outpatient",
  "claimedAmount": 5000,
  "treatmentDate": "2026-09-01",
  "isEmergency": false
}
```

---

## Task 8 — Generate integration tests (25 min · Manual)

**Prompt:**

```text
Extend tests/Claims.IntegrationTests/ClaimsApiTests.cs with the integration tests from the matrix.

Requirements:
- xUnit + WebApplicationFactory<Program> (already set up)
- HttpClient from factory.CreateClient()
- System.Net.Http.Json for PostAsJsonAsync / GetFromJsonAsync
- do NOT mock ClaimService or ClaimRepository — test the real stack including EF InMemory
- assert HTTP status codes and important response fields (id, status, payableAmount)
- keep the existing health test

Do not modify production code.
```

Run:

```powershell
dotnet test .\tests\Claims.IntegrationTests\Claims.IntegrationTests.csproj
```

**Expected flow:**

```text
GET /health           → 200
POST /api/claims      → 201 Created (Location header + body)
POST invalid amount   → 400 Bad Request
GET /api/claims/{id}  → 200 (same id, payableAmount = 4000 for outpatient 5000)
GET unknown id        → 404
```

If POST returns 400 unexpectedly, check JSON property names match the record (`memberId`, not `MemberId` — ASP.NET Core default is camelCase).

```powershell
dotnet test .\ClaimsProcessingTestingLab.sln
git add tests/Claims.IntegrationTests/
git commit -m "test: API integration tests with WebApplicationFactory"
```

**Deliverable 4.** **Check:** full solution test count significantly higher than 3.

---

# EXERCISE 4 — Review and Improve

## Task 9 — Gap analysis (15 min · Plan)

`/clear` or new tab, then:

```text
Act as a principal .NET test engineer.

Review all tests in:
- tests/Claims.UnitTests
- tests/Claims.IntegrationTests

Do not modify anything.

Identify only the 5 most important gaps. Check for:
- missing business rules
- missing boundaries
- weak assertions (status only, no body checks)
- excessive mocks
- duplicate tests
- missing failure paths

Rank by risk to the business, not by ease of implementation.
```

Discuss: duplicate submission has **no test and no fix** in this lab — note it as a gap Claude should find.

---

## Task 10 — Review the git diff (15 min · Plan)

```powershell
git diff main 2>$null; if (-not $?) { git log --oneline }
git diff HEAD~5..HEAD --stat
```

**Prompt:**

```text
Review the git diff of all test and production changes in this session.

Check whether any test:
- changed production behavior unnecessarily
- uses incorrect business assumptions
- is too tightly coupled to implementation details
- is fragile (depends on DateTime.Now without injection)
- duplicates another test

Also check the one production fix (Preventive cap):
- was it the minimum change?
- could a test have been wrong instead?

Recommend: ACCEPT / ACCEPT WITH CHANGES / REJECT
```

**Deliverable 6.**

---

## Task 11 — Final validation (5 min)

```powershell
dotnet build .\ClaimsProcessingTestingLab.sln
dotnet test  .\ClaimsProcessingTestingLab.sln --logger "console;verbosity=normal"
```

Record your final counts:

```text
Unit tests:         __ passed
Integration tests:  __ passed
Production fixes:   1 (Preventive cap)
Defects found:      1 confirmed, 1 noted (duplicate submission — no fix required)
```

---

# Appendix A — `CLAUDE.md`

```markdown
# Claims Processing — Testing AI Contract

Act as a senior .NET test engineer helping automate tests for this repository.

## Project
- .NET 8 ASP.NET Core API: src/Claims.Api
- Business logic: src/Claims.Application (ClaimCalculator, ClaimValidator, ClaimService)
- Unit tests: tests/Claims.UnitTests (xUnit + Moq)
- Integration tests: tests/Claims.IntegrationTests (WebApplicationFactory<Program>)
- Health: GET /health
- Claims API: POST/GET /api/claims

## Commands
- Build: dotnet build ClaimsProcessingTestingLab.sln
- Test:  dotnet test ClaimsProcessingTestingLab.sln
- Unit:  dotnet test tests/Claims.UnitTests/Claims.UnitTests.csproj
- Int:   dotnet test tests/Claims.IntegrationTests/Claims.IntegrationTests.csproj

## Rules
1. Inspect code and business rules before creating tests.
2. Create a test matrix before generating test code.
3. Keep tests focused, readable, Arrange-Act-Assert.
4. Mock only external collaborators (repository, notification). Use real validator and calculator.
5. Do not modify production code just because a test fails — diagnose first.
6. When a test fails, determine: production defect, test defect, mock misconfiguration, or unclear requirement.
7. Integration tests verify observable HTTP behavior — no mocking the service layer.
8. Avoid duplicate or trivial tests.
9. Apply the minimum production fix when a real defect is confirmed.
10. Review the final git diff critically.
```

---

# Appendix B — Troubleshooting

| Symptom | Fix |
|---|---|
| `Unable to find a project to restore!` then build "succeeds" | Rebuild `.sln` (Task 0.2) |
| `error CS0246: 'Fact' could not be found` | Add `<Using Include="Xunit" />` to both test `.csproj` files |
| All calculator tests pass, no failure | Add Preventive ₹15,000 cap test — defect is intentional |
| Moq `Times.Never` fails on invalid claim | Validation throws before repository call — use `Assert.ThrowsAsync` + `Verify Never` |
| Integration POST returns 400 | Check JSON camelCase property names match `SubmitClaimRequest` record |
| Integration GET 404 for created claim | Parse `id` from POST response body, not a random Guid |
| `WebApplicationFactory<Program>` not found | Ensure `public partial class Program { }` exists in Claims.Api/Program.cs |
| Claude generates tests for everything at once | Use smaller prompts per class; stay in **Plan** until matrix is approved |
| Claude "fixes" production code to make tests pass | Reject — run Task 4 diagnosis prompt first |

---

# Appendix C — Command summary

```powershell
# setup
cd ClaimsProcessingTestingLab
Move-Item .\docs "$env:USERPROFILE\Desktop\lab04-trainer-docs"

# fix build
Remove-Item .\ClaimsProcessingTestingLab.sln -Force
dotnet new sln -n ClaimsProcessingTestingLab
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
# add <Using Include="Xunit" /> to both test csproj files

dotnet build .\ClaimsProcessingTestingLab.sln
dotnet test  .\ClaimsProcessingTestingLab.sln

# per-exercise loop
dotnet test .\tests\Claims.UnitTests\Claims.UnitTests.csproj
dotnet test .\tests\Claims.IntegrationTests\Claims.IntegrationTests.csproj
git diff
git add . ; git commit -m "..."
```

> **The rule:** Claude helps you understand and design tests. A failing test that exposes a real defect
> is more valuable than fifty tests that only pass because production code was changed to match wrong assumptions.
