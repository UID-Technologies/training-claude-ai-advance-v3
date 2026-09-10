# Lab 02 — Code Quality, Bug Detection & Secure Coding with Claude

**Hands-on lab · VS Code + Claude Code · Application: `SecureCommerceLegacyLab` (.NET 8)**

| | |
|---|---|
| **Duration** | 3 hours · 15 tasks |
| **Level** | Intermediate |
| **Stack** | .NET 8, ASP.NET Core Web API, EF Core 8, SQL Server, xUnit |
| **Tools** | VS Code 1.94+, Claude Code extension, .NET SDK 8 or 9, Git |
| **Database** | Not required — all tasks run with EF Core InMemory |

## Description

You have joined the team maintaining **SecureCommerce**, a B2B commerce API. It works, but the team
reports inconsistent standards, unexplained production defects, security concerns and almost no tests.

You will use Claude to **review, detect, diagnose, fix and validate** — in that order. Claude proposes;
you decide. Every fix is proved by a test that failed before it and passes after.

## Business scenario

> A customer-success ticket says a buyer placed an order and the warehouse stock **went up**.
> Separately, finance found an order with a **negative total**. The security team's pre-audit flagged the
> admin reporting endpoint and the login flow. You have one sprint to review the codebase, fix the
> confirmed defects with regression tests, and close the top security gaps — without a rewrite.

## What you will produce

| # | Deliverable | Task |
|---|---|---|
| 1 | Top 5 code-quality findings | 2 |
| 2 | One quality refactoring (`IPaymentGateway`) | 3 |
| 3 | Two bug reports: symptom → root cause → fix → regression test | 5–8 |
| 4 | Top 5 security risks with severity | 9 |
| 5 | Two security improvements | 10–12 |
| 6 | Passing regression test suite | 6–8 |
| 7 | Independent AI review report + PASS/FAIL verdict | 14–15 |

---

## Prerequisites

```powershell
dotnet --list-sdks        # 8.0.x or 9.0.x
git --version
code --version            # 1.94.0+
```

Install the **Claude Code** extension (`Ctrl+Shift+X` → "Claude Code" → Install → reload → sign in).
Open Claude with the Spark icon (✱) in the editor toolbar or `Ctrl+Shift+Esc`.

**Permission modes** — click the mode indicator at the bottom of the prompt box:

- **Plan** for every review/analysis task (Tasks 1, 2, 4, 5, 9, 14) — Claude cannot edit files.
- **Manual** for every code change (Tasks 3, 6–8, 10–12) — you approve each diff.

---

# Task 0 — Setup (15 min)

### 0.1 Remove the spoilers

The repository ships with a trainer answer sheet. If you leave it in place, Claude will read it and hand
you every finding — the lab becomes pointless.

```powershell
cd "F:\#Training\training-claude-ai-advance-v3\02 - Code Quality, Bug Detection & Secure Coding"
Move-Item .\SecureCommerceLegacyLab\docs "$env:USERPROFILE\Desktop\lab02-trainer-docs"
```

> Trainers: keep `docs\TRAINER-ANSWER-SHEET.md` for grading. Students: do not open it.

### 0.2 Baseline

```powershell
cd .\SecureCommerceLegacyLab
code .
git init
git add .
git commit -m "baseline: SecureCommerce as shipped"
git checkout -b review-and-fix
```

### 0.3 Fix the build (the shipped solution builds nothing)

Run this first and read the output carefully:

```powershell
dotnet build .\SecureCommerceLegacyLab.sln
```

You get `Unable to find a project to restore!` followed by `Build succeeded` — the `.sln` lists five
projects but has no configuration sections, so **nothing is compiled**. Repair it:

```powershell
Remove-Item .\SecureCommerceLegacyLab.sln -Force
dotnet new sln -n SecureCommerceLegacyLab
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
dotnet build .\SecureCommerceLegacyLab.sln
```

Now you get a real error:

```text
tests\SecureCommerce.Tests\DiscountServiceTests.cs(5,6): error CS0246:
    The type or namespace name 'Fact' could not be found
```

Fix it by adding the missing global using to `tests\SecureCommerce.Tests\SecureCommerce.Tests.csproj`:

```xml
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
```

Verify green:

```powershell
dotnet build .\SecureCommerceLegacyLab.sln
dotnet test  .\SecureCommerceLegacyLab.sln
```

Expected: `Build succeeded. 0 Error(s)` and `Passed! - Failed: 0, Passed: 1, Total: 1`.

### 0.4 Create the engineering contract

In Claude, run `/init`, then replace the generated `CLAUDE.md` with the block in
[Appendix A](#appendix-a--claudemd). Confirm it loaded:

```text
/context
```

`CLAUDE.md` must appear under **Memory files**.

```powershell
git add . ; git commit -m "build: repair solution and test project"
```

**Check:** build green, 1 test passing, `docs/` removed, `CLAUDE.md` loaded.

---

# PART A — Code Quality

## Task 1 — Understand the application (10 min · Plan mode)

**Prompt:**

```text
Act as a senior .NET engineer. Analyze this repository without changing any code.

Explain concisely:
1. Business purpose
2. Projects and their responsibilities
3. The important services
4. Database access approach
5. External integrations
6. The end-to-end order-placement workflow, controller to database to payment
7. The three components that look highest-risk and why

Cite file paths for every claim. Then draw a simple text architecture diagram.
```

**Expected map:**

```text
Controllers (Auth, Order, Product, Admin, Upload)
      ↓
Services (OrderService, DiscountService, AuthService, FileStorageService, PaymentGateway, AuditLogger)
      ↓                                   ↓
Data (EF Core repositories)        https://payments.example
      ↓
SQL Server
```

**Check:** you can name which controller reaches the database without passing through a service.
(Answer: `ProductController` and `AdminController`.)

---

## Task 2 — AI-assisted code-quality review (20 min · Plan mode)

**Prompt 1 — repository sweep:**

```text
Act as a senior C# code reviewer. Review this repository for code-quality problems.

Focus on: readability, method complexity, oversized responsibilities, tight coupling, duplication,
magic strings, exception handling, blocking/synchronous code, naming, and testability.

For each finding give a table row:
| File:line | Problem | Why it matters | Severity | Recommended improvement |

Cite real file:line for every row. Report the 12 most important findings only. Do not modify code.
```

**Prompt 2 — focus on the god service:**

```text
Review @src/SecureCommerce.Services/OrderService.cs against clean-code and SOLID principles.

Identify: code smells, the distinct responsibilities in PlaceOrder, hidden dependencies,
complex business logic, database responsibilities, external-service responsibilities, logging.

Then propose a minimal refactoring plan — maximum 5 changes, ordered by
maintainability-and-testability benefit per unit of risk. Do not generate code yet.
```

**What `PlaceOrder` actually does** — validation + product lookup + stock mutation + price calculation +
discount + persistence + payment + audit logging, in 45 lines. Confirm these specific smells yourself:

| Finding | Evidence |
|---|---|
| Hidden dependency | `new PaymentGateway()` inside `PlaceOrder` |
| Static, unmockable logger | `AuditLogger.Log(...)`, a `static` class writing to a file |
| Generic exceptions | `throw new Exception("Product missing")` — 5 occurrences |
| Magic strings | `"NEW"`, `"PAID"`, `"PAYMENT_FAILED"`, `"User"` |
| `SaveChanges` in a loop | `_products.Update(product)` inside `foreach` |
| Order persisted before payment | `_db.SaveChanges()` then `Charge(...)` |
| Blocking async | `.Result` in `PaymentGateway.Charge` |
| `HttpClient` per call | `new HttpClient()` in `PaymentGateway` |
| Filter in memory | `_db.Products.Where(...).ToList().Where(...)` in `ProductRepository.Search` |
| Non-deterministic | `DateTime.Now` for order timestamps and payment references |

**Deliverable 1:** your top 5 findings, with `file:line` and severity.

---

## Task 3 — Implement one quality improvement (20 min · Manual mode)

Make the payment gateway injectable — this also unblocks every test in Part B.

**Prompt:**

```text
Refactor so OrderService depends on an abstraction instead of constructing PaymentGateway.

1. Create IPaymentGateway in SecureCommerce.Services with a Charge(decimal amount, string reference) method.
2. Make PaymentGateway implement it.
3. Inject IPaymentGateway into OrderService via the constructor.
4. Register it in @src/SecureCommerce.Web/Program.cs so the API still starts.

Do not change any other business behavior. Do not touch DiscountService, AuthService or the controllers.

Before editing, list every file you will change and what you will change in it.
```

**Expected diff — 4 files:**

| File | Change |
|---|---|
| `src/SecureCommerce.Services/IPaymentGateway.cs` | new interface |
| `src/SecureCommerce.Services/PaymentGateway.cs` | `: IPaymentGateway` |
| `src/SecureCommerce.Services/OrderService.cs` | constructor parameter + `_payments.Charge(...)` |
| `src/SecureCommerce.Web/Program.cs` | `builder.Services.AddScoped<IPaymentGateway, PaymentGateway>();` |

> **Do not skip the `Program.cs` registration.** Without it the project still compiles and the API fails
> only at runtime, with an unresolved-service error on the first order.

```powershell
git diff
dotnet build .\SecureCommerceLegacyLab.sln
dotnet test  .\SecureCommerceLegacyLab.sln
```

**Then make Claude review itself:**

```text
Review your own git diff. Check whether: any unrelated code changed, business behavior changed,
an unnecessary abstraction was introduced, error handling changed, or a new security issue appeared.
Answer each point explicitly with yes/no and evidence.
```

```powershell
git add . ; git commit -m "refactor: inject IPaymentGateway into OrderService"
```

**Deliverable 2.** **Check:** build green, existing test still passes, exactly 4 files changed.

---

# PART B — Bug Detection & Root-Cause Analysis

## Task 4 — Bug hunt (15 min · Plan mode)

**Prompt:**

```text
Act as a senior debugging engineer. Inspect this application for functional bugs and edge cases.

For each defect provide:
| File:line | Trigger condition | Expected behavior | Actual behavior | Business impact | Test that would expose it |

Prioritize defects that cause money or inventory to be wrong. Do not fix anything yet.
```

Claude should surface several. **We work only two of them** — negative quantity and the discount
overflow. Note the rest as backlog. Strong candidates it should find:

- zero/negative quantity accepted in `OrderService.PlaceOrder`
- `FLAT100` can make `NetAmount` negative
- coupon matching is case-sensitive (`save10` silently gets no discount)
- order is persisted before payment; a failed payment leaves stock decremented
- no transaction around the stock loop — a mid-loop failure leaves partial stock updates
- `AuthService.Login` dereferences `request` and `request.Password` with no null check

---

## Task 5 — Bug 1: negative quantity (15 min · Plan mode)

Open `src/SecureCommerce.Services/OrderService.cs` and read the stock check:

```csharp
if (product.Stock < item.Quantity) throw new Exception("Insufficient stock");
...
product.Stock -= item.Quantity;
```

**Prompt — trace it:**

```text
Analyze @src/SecureCommerce.Services/OrderService.cs, PlaceOrder, when item.Quantity is 0, -1 and -10,
for a product with Stock = 10 and Price = 100.

Trace the value of: the stock guard result, LineTotal, gross, NetAmount, and product.Stock after the loop.

State clearly whether each outcome is correct. Do not change code.
```

**The defect:** with `Quantity = -5`, the guard `10 < -5` is false, so it passes; then
`product.Stock -= -5` **increases** stock to 15, and `LineTotal` is negative, reducing the order total.
A buyer can inflate inventory and shrink their bill in one request.

**Prompt — root cause:**

```text
Perform root-cause analysis for the negative-quantity defect in this exact format:
Symptom / Trigger / Immediate cause / Root cause / Business impact / Correct remediation / Regression tests required.

The remediation must state the correct business rule, not just "check for negative".
Explain why checking `Quantity < 0` alone would still be wrong. Do not change code yet.
```

**Key teaching point:** the rule is `Quantity > 0`. Fixing only `< 0` leaves `Quantity = 0` valid, which
creates zero-value order lines.

---

## Task 6 — Write the regression tests FIRST (25 min · Manual mode)

Add the in-memory EF provider to the test project:

```powershell
dotnet add .\tests\SecureCommerce.Tests\SecureCommerce.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.8
```

**Prompt:**

```text
Create tests/SecureCommerce.Tests/OrderServiceQuantityTests.cs — xUnit regression tests that express the
CORRECT business behavior for order quantity and discounts.

Test harness requirements:
- Build CommerceDbContext with UseInMemoryDatabase and a fresh Guid name per test.
- Seed one active product: Id 1, Price 100, Stock 10.
- Use a hand-written FakePaymentGateway implementing IPaymentGateway that returns true. No mocking library.
- Construct OrderService with the in-memory context, a real ProductRepository, a real DiscountService and the fake gateway.

Cover:
1. Quantity -10, -1 and 0 are rejected (Theory)
2. A negative quantity never increases product stock
3. Quantity 2 still succeeds: GrossAmount 200 and Stock falls to 8
4. Gross 50 with coupon FLAT100 never produces a negative NetAmount

Do NOT modify any production code in this step. These tests are expected to fail right now.
```

```powershell
dotnet test .\SecureCommerceLegacyLab.sln
```

**Expected — red, and that is correct:**

```text
Failed!  - Failed: 5, Passed: 2, Total: 7
```

The two passing ones are the pre-existing `SAVE10` test and the positive-quantity happy path. Failing
tests prove the defects are real and that your tests actually detect them.

```powershell
git add . ; git commit -m "test: failing regression tests for quantity and discount defects"
```

**Check:** if all tests pass at this point, your tests are wrong — not the code.

---

## Task 7 — Fix Bug 1 (10 min · Manual mode)

**Prompt:**

```text
Implement the minimum production-code change in @src/SecureCommerce.Services/OrderService.cs to reject
zero and negative quantities. Validate before the product lookup so no stock is touched.

Do not refactor unrelated code. Do not change the exception type used elsewhere in this method.

After the change, explain exactly why each failing test now passes.
```

**Expected change — one line:**

```csharp
if (item.Quantity <= 0) throw new Exception("Quantity must be greater than zero");
```

```powershell
dotnet test .\SecureCommerceLegacyLab.sln
```

---

## Task 8 — Bug 2: discount produces a negative total (15 min · Manual mode)

**Prompt — confirm:**

```text
Analyze @src/SecureCommerce.Services/DiscountService.cs together with PlaceOrder in
@src/SecureCommerce.Services/OrderService.cs.

Can any coupon make NetAmount negative? Give one reproducible example with concrete numbers,
and explain the business impact of a negative order total reaching payment and accounting.
```

**The defect:** `FLAT100` returns a flat `100`, so a ₹50 order becomes `50 - 100 = -50`.

**Prompt — fix, with a design decision:**

```text
Fix it with the smallest change that satisfies the rule "a discount can never exceed the order gross,
and NetAmount can never be negative".

First tell me whether the cap belongs in DiscountService or in OrderService, and justify the choice
in two sentences. Then implement it in that one place.
```

**Expected change — one line in `PlaceOrder`:**

```csharp
var discount = Math.Min(_discounts.CalculateDiscount(request.CouponCode, gross), gross);
```

> Capping in `OrderService` covers every current and future coupon; capping inside `DiscountService`
> only fixes `FLAT100`. Either is defensible if the student can argue it — the reasoning is the exercise.

```powershell
dotnet test .\SecureCommerceLegacyLab.sln
```

**Expected — green:**

```text
Passed!  - Failed: 0, Passed: 7, Total: 7
```

```powershell
git diff HEAD~1
git add . ; git commit -m "fix: reject non-positive quantities and cap discount at order gross"
```

**Deliverable 3 + 6.** **Check:** two one-line production changes turned 5 red tests green.

---

## Task 9 — Stack-trace analysis (10 min · Plan mode)

Not every defect is visible statically. Give Claude a production error and let it work backwards.

**Prompt:**

```text
A production log shows:

System.NullReferenceException: Object reference not set to an instance of an object.
   at SecureCommerce.Services.AuthService.Login(LoginRequest request)

Read @src/SecureCommerce.Services/AuthService.cs.

1. Identify every statement in Login that could throw this.
2. Give the exact request payloads that reproduce each one.
3. Explain which line executes first and therefore which cause is most likely.
4. State the invalid assumption the code makes.
5. Recommend the fix and where input validation belongs — controller or service. Justify it.

Do not fix it yet.
```

**Answer:** `Login` hashes `request.Password` on the very first line with no null check, so `request = null`
or `Password = null` both throw before any lookup. `Register` guards its input; `Login` does not.

**Optional (5 min):** fix it with a regression test, using the same red-then-green loop.

---

# PART C — Secure Coding

## Task 10 — Security review (20 min · Plan mode)

**Prompt:**

```text
Act as a senior application security engineer. Review this .NET 8 application for vulnerabilities.

Cover only: input validation, SQL injection, secrets, authentication, authorization,
sensitive-data exposure, exception handling, logging, and file upload.

For each finding:
| File:line | Vulnerability | Severity (Critical/High/Medium/Low) | Attack scenario | Remediation |

Judge severity by exploitability and business impact for a B2B commerce platform.
Do not modify code. Do not produce working exploit payloads.
```

**Confirm these yourself** — they are all really present:

| Vulnerability | Location |
|---|---|
| SQL injection via string concatenation into `FromSqlRaw` | `AdminReportRepository.SearchOrdersRaw` |
| Admin endpoint with no authentication or authorization | `AdminController.Orders` |
| Fake token: `email + ":" + role`, unsigned and forgeable | `AuthController.Login` |
| Fast unsalted SHA-256 password hashing | `AuthService.Register` / `Login` |
| Account-enumeration messages ("User does not exist" vs "Password is incorrect") | `AuthService.Login` |
| Stack trace returned to the client | `AuthController.Register` |
| Hard-coded DB password and JWT secret | `appsettings.json` |
| Path traversal — user-supplied filename passed to `Path.Combine` | `FileStorageService.SaveAsync` |
| No size, extension or content-type validation on upload | `UploadController.Upload` |
| Physical server path returned to the caller | `UploadController.Upload` |
| Full request payload serialized into the audit log | `OrderService` → `AuditLogger.Log` |
| No authentication, HTTPS redirection or exception handler middleware | `Program.cs` |

### Prioritize

```text
Rank your findings as Critical / High / Medium / Low and recommend the first 5 the team should fix.
Justify the order using likelihood × business impact, not by how easy each fix is.
```

**Deliverable 4:** top 5 security risks with severity.

---

## Task 11 — Fix the SQL injection (15 min · Manual mode)

Open `src/SecureCommerce.Data/AdminReportRepository.cs`:

```csharp
var sql = "SELECT * FROM Orders WHERE Status='" + status +
          "' AND CreatedOn >= '" + fromDate + "'";
return _db.Orders.FromSqlRaw(sql).ToListAsync();
```

**Prompt — analyze:**

```text
Analyze @src/SecureCommerce.Data/AdminReportRepository.cs for SQL injection.

Trace how untrusted HTTP input reaches the SQL statement, naming every hop from the route to the query.
Explain what authentication protects this path today.

Then recommend the safest EF Core implementation. Compare parameterized FromSqlInterpolated against a
plain LINQ query, and note the `fromDate` type problem — it is a string, not a DateTime.

Do not produce exploit payloads.
```

**The path:** `GET /api/admin/orders?status=...&fromDate=...` → `AdminController.Orders` (no `[Authorize]`)
→ `SearchOrdersRaw` → concatenated SQL. **Unauthenticated and directly reachable** — that is why it ranks
Critical rather than High.

**Prompt — fix:**

```text
Replace the raw SQL with a parameterized LINQ query. Parse fromDate to DateTime and reject an
unparseable value with a clear error instead of passing it through.

Keep the method signature and its async return type. Do not change the API contract.
```

**Prompt — prove it:**

```text
Add xUnit tests using the in-memory provider proving that a status value containing SQL metacharacters
(quotes, a semicolon, a comment marker) is treated as literal data and simply matches nothing,
rather than altering the query. Also test that an invalid fromDate is rejected.
```

```powershell
dotnet test .\SecureCommerceLegacyLab.sln
git add . ; git commit -m "security: parameterize admin order search (SQL injection)"
```

**Deliverable 5a.**

---

## Task 12 — Choose one more security fix (20 min · Manual mode)

Pick **one** of the three below. Discuss the other two rather than implementing them.

### Option A — Authorization on the admin endpoint

```text
Review @src/SecureCommerce.Web/Controllers/AdminController.cs and @src/SecureCommerce.Web/Program.cs
from an authorization perspective.

Who can call GET /api/admin/orders today? What is the business risk?

Design an ASP.NET Core authorization approach for admin-only endpoints: authentication scheme,
a named policy, the role/claim it requires, and where each piece is registered.

Then implement the minimum: register authentication and authorization services and middleware,
add the policy, and apply it to AdminController. Tell me exactly what still has to be true for this
to be real security given that the current login issues a fake token.
```

The honest answer matters more than the code: `[Authorize]` over a forgeable `email:role` token is
theatre. Note the dependency on Option B in your report.

### Option B — Secrets and password storage

```text
Review @src/SecureCommerce.Web/appsettings.json.

1. Classify each value as configuration or secret.
2. Recommend where each belongs for local development, CI/CD and production.
3. Give me the exact commands to move the connection-string password and the JWT secret to User Secrets,
   and show the resulting appsettings.json with placeholders only.
4. State the remediation runbook for secrets already committed to Git — including why rotation matters
   more than deleting the line.

Then review password storage in @src/SecureCommerce.Services/AuthService.cs.
Explain why SHA-256 is the wrong algorithm for passwords, what "unsalted" and "fast" cost us concretely,
and recommend the ASP.NET Core replacement. Do not write a real secret into any file.
```

Reference commands:

```powershell
dotnet user-secrets init --project .\src\SecureCommerce.Web
dotnet user-secrets set "ConnectionStrings:CommerceDb" "Server=localhost;Database=SecureCommerce;Trusted_Connection=True;TrustServerCertificate=True" --project .\src\SecureCommerce.Web
dotnet user-secrets set "Jwt:Secret" "<generated-value>" --project .\src\SecureCommerce.Web
```

### Option C — Secure file upload

```text
Perform a threat review of @src/SecureCommerce.Web/Controllers/UploadController.cs and
@src/SecureCommerce.Services/FileStorageService.cs.

Cover: user-controlled file name, path traversal, file size, extension, content type, storage location,
executable content, and the physical path returned in the response.

Then implement a safer SaveAsync: generate a server-side file name, validate the extension against an
allowlist, enforce a maximum size, confirm the resolved path stays inside the upload root,
and return an opaque identifier instead of a filesystem path.
```

Also fix the error leak while you are here:

```text
Recommend a secure ASP.NET Core exception-handling approach for this API: a global exception handler,
ProblemDetails responses, a correlation ID returned to the client and logged server-side, and no stack
traces in responses.

Give me a table: for each exception type, what the CLIENT sees versus what is LOGGED.

Then remove the stack trace from @src/SecureCommerce.Web/Controllers/AuthController.cs.
Also review AuditLogger usage in OrderService: list the fields that must never be logged and show the
structured alternative.
```

```powershell
dotnet build .\SecureCommerceLegacyLab.sln
dotnet test  .\SecureCommerceLegacyLab.sln
git add . ; git commit -m "security: <describe the fix you implemented>"
```

**Deliverable 5b.**

---

# PART D — Validate the AI-Generated Changes

## Task 13 — Review the diff yourself (10 min)

```powershell
git diff main...review-and-fix --stat
git diff main...review-and-fix
```

```text
□ Only the files you expected changed
□ No behavior changed that you did not ask for
□ No secret, key or connection string added
□ No new NuGet package you did not approve
□ No abstraction with one implementation and no test using it
□ No commented-out code or leftover TODO
□ Every production fix has a test that failed before it
```

---

## Task 14 — Claude critiques its own work (10 min)

```text
Act as a principal .NET reviewer. Review the current git diff as if another developer wrote it.
Do not assume it is correct.

Check for: code-quality regressions, unintended behavior changes, missing edge cases, security
vulnerabilities, authentication/authorization gaps, sensitive-data exposure, concurrency problems,
performance regressions, missing tests, and over-engineering.

Report findings before making any recommendation.
```

---

## Task 15 — Independent review in a fresh context (15 min)

A session that wrote the code will defend it. Start clean with `/clear` or a new tab (`Ctrl+Shift+Esc`) —
`CLAUDE.md` still loads. Try the bundled skill first:

```text
/code-review
```

Then the explicit adversarial pass:

```text
You are reviewing an AI-generated pull request. Analyze the diff between `main` and `review-and-fix`
independently, by reading the code — not the commit messages.

Your objective is to find reasons this change should NOT be merged.

Review: correctness, security, maintainability, testing, performance.
Classify every finding as Blocker / Major / Minor with file:line and a recommended fix.

Finish with a release-readiness verdict: PASS, PASS WITH CONDITIONS or FAIL,
and explain any remaining blocker in three sentences.
```

Fix every **Blocker**, re-run the gates, and record Majors/Minors as backlog:

```powershell
dotnet build .\SecureCommerceLegacyLab.sln
dotnet test  .\SecureCommerceLegacyLab.sln
```

**Deliverable 7:** the review report plus your own judgement on where the review was right or wrong.

---

# Appendix A — `CLAUDE.md`

```markdown
# SecureCommerce — AI Engineering Contract

Act as a senior .NET engineer, code reviewer and application security reviewer.

## Project
- .NET 8, ASP.NET Core Web API, EF Core 8, SQL Server, xUnit
- Training repository with intentional bugs and vulnerabilities. Never treat existing code as a good example.
- Build: dotnet build .\SecureCommerceLegacyLab.sln
- Test:  dotnet test  .\SecureCommerceLegacyLab.sln
- Tests use the EF Core InMemory provider. No SQL Server is required.

## Rules
1. Inspect the code before concluding. Cite file:line for every claim.
2. Review and fix one issue at a time. Never batch unrelated changes.
3. Before editing, state: problem, root cause, proposed change, affected files, regression risk.
4. Write the failing regression test before the production fix.
5. Never change a test just to make it pass. Diagnose the root cause and explain it first.
6. Make the smallest change that satisfies the business rule. No opportunistic refactoring.
7. No new NuGet package or abstraction without a justification I have accepted.
8. Never write a real secret into any file. Use placeholders.
9. Do not produce working exploit payloads; explain the vulnerability and the fix.
10. After changes, review your own diff critically and report what you are unsure about.
```

---

# Appendix B — Verified reference test (Task 6)

Try to have Claude generate this first. Verified: fails 5 of 7 before the fixes, passes 7 of 7 after.

```csharp
using Microsoft.EntityFrameworkCore;
using SecureCommerce.Data;
using SecureCommerce.Domain;
using SecureCommerce.Services;

namespace SecureCommerce.Tests;

public class FakePaymentGateway : IPaymentGateway {
    public bool Result = true;
    public bool Charge(decimal amount, string reference) => Result;
}

public class OrderServiceQuantityTests {
    private static (OrderService sut, CommerceDbContext db) Build(int stock = 10, decimal price = 100m) {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new CommerceDbContext(options);
        db.Products.Add(new Product { Id = 1, Sku = "SKU-1", Name = "Widget", Price = price, Stock = stock, IsActive = true });
        db.SaveChanges();

        var sut = new OrderService(db, new ProductRepository(db), new DiscountService(), new FakePaymentGateway());
        return (sut, db);
    }

    private static PlaceOrderRequest Request(int quantity, string coupon = null) => new() {
        UserId = 1,
        CouponCode = coupon,
        Items = new List<PlaceOrderItemRequest> { new() { ProductId = 1, Quantity = quantity } }
    };

    [Theory]
    [InlineData(-10)]
    [InlineData(-1)]
    [InlineData(0)]
    public void NonPositiveQuantity_IsRejected(int quantity) {
        var (sut, _) = Build();
        Assert.ThrowsAny<Exception>(() => sut.PlaceOrder(Request(quantity)));
    }

    [Fact]
    public void NegativeQuantity_DoesNotIncreaseStock() {
        var (sut, db) = Build(stock: 10);
        try { sut.PlaceOrder(Request(-5)); } catch { /* expected once fixed */ }
        Assert.Equal(10, db.Products.Single(p => p.Id == 1).Stock);
    }

    [Fact]
    public void PositiveQuantity_StillWorks() {
        var (sut, db) = Build(stock: 10, price: 100m);
        var orderId = sut.PlaceOrder(Request(2));
        Assert.Equal(200m, db.Orders.Single(o => o.Id == orderId).GrossAmount);
        Assert.Equal(8, db.Products.Single(p => p.Id == 1).Stock);
    }

    [Fact]
    public void Flat100Coupon_CannotProduceNegativeNetAmount() {
        var (sut, db) = Build(stock: 10, price: 50m);
        var orderId = sut.PlaceOrder(Request(1, "FLAT100"));
        var order = db.Orders.Single(o => o.Id == orderId);
        Assert.True(order.NetAmount >= 0, $"NetAmount was {order.NetAmount}");
    }
}
```

---

# Appendix C — Troubleshooting

| Symptom | Fix |
|---|---|
| `dotnet build` says "Unable to find a project to restore!" then succeeds | The shipped `.sln` has no configurations — rebuild it (Task 0.3) |
| `error CS0246: 'Fact' could not be found` | Add `<Using Include="Xunit" />` to the test `.csproj` |
| `UseInMemoryDatabase` not found | `dotnet add ... package Microsoft.EntityFrameworkCore.InMemory --version 8.0.8` |
| Order tests hang or fail with an HTTP error | `OrderService` still calls `new PaymentGateway()` — finish Task 3 |
| API starts but ordering fails at runtime | `IPaymentGateway` not registered in `Program.cs` |
| Claude instantly "finds" every bug | `docs\TRAINER-ANSWER-SHEET.md` is still in the repo — remove it (Task 0.1) |
| Claude edits files you did not approve | Switch the mode indicator to **Manual** |
| Claude forgets the rules mid-session | `/context`, then `/compact`; use `/clear` between parts |
| A change broke something and you cannot tell what | Hover the message → **Rewind code to here**, or `git checkout -- <file>` |

---

# Appendix D — Command summary

```powershell
# setup
Move-Item .\SecureCommerceLegacyLab\docs "$env:USERPROFILE\Desktop\lab02-trainer-docs"
cd .\SecureCommerceLegacyLab ; code .
git init ; git add . ; git commit -m "baseline" ; git checkout -b review-and-fix

# repair the build
Remove-Item .\SecureCommerceLegacyLab.sln -Force
dotnet new sln -n SecureCommerceLegacyLab
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
# add <Using Include="Xunit" /> to tests\SecureCommerce.Tests\SecureCommerce.Tests.csproj

# test tooling
dotnet add .\tests\SecureCommerce.Tests\SecureCommerce.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.8

# the loop you repeat all lab
dotnet build .\SecureCommerceLegacyLab.sln
dotnet test  .\SecureCommerceLegacyLab.sln
git diff
git add . ; git commit -m "..."
```

> **The rule that makes this lab work:** Claude reviews, diagnoses and proposes. A failing test proves the
> bug is real, a passing test proves the fix works, and you remain accountable for accepting the change.
