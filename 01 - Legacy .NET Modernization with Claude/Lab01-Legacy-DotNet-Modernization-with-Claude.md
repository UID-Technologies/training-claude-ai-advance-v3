# Lab 01 — AI-Assisted Legacy .NET Modernization with Claude

**Hands-on lab · VS Code + Claude Code · Application:** `LegacyEnterpriseBanking`


|                        |                                                                                                         |
| ---------------------- | ------------------------------------------------------------------------------------------------------- |
| **Lab code**           | AI-MOD-01                                                                                               |
| **Duration**           | 4 hours (7 modules, each 20–45 min)                                                                     |
| **Level**              | Intermediate → Advanced                                                                                 |
| **Audience**           | .NET developers, tech leads, architects, modernization engineers                                        |
| **Sample application** | `LegacyEnterpriseBanking` (included in this folder)                                                     |
| **Current stack**      | .NET Framework 4.8, ASP.NET Web API 2, Entity Framework 6, raw ADO.NET, SQL Server                      |
| **Target stack**       | .NET 8, ASP.NET Core Web API, EF Core, DI, async/await, `IHttpClientFactory`, structured logging, xUnit |
| **Tooling**            | VS Code 1.94+, Claude Code extension (v2.1.2xx+), .NET SDK 9 (builds `net48` and `net8.0`), Git         |
| **Mode of work**       | Every exercise is driven by prompts you copy into Claude, followed by your own review                   |


---



## 1. Lab description

You will modernize a deliberately awful legacy banking application using Claude as an AI engineering
assistant inside VS Code — not as a code generator you trust blindly.

The lab is built around one rule that professional modernization work depends on:

> **You never ask Claude to rewrite the application. You ask Claude to help you understand, assess,
> plan, test, refactor, modernize and review it — one small, reviewable change at a time.**

You will practise the full loop on a single business feature (**Money Transfer**):

```text
Understand → Assess → Plan → Test existing behavior → Refactor
          → Modernize → Build & Test → Independent AI Review
```

By the end you will have a legacy application that still runs, a safety net of tests, a phased
roadmap, several concrete reliability/security/performance fixes, and one feature running on .NET 8 —
plus a review report where Claude critiques its own work.

### Learning outcomes

After completing this lab you can:

1. Use Claude to reverse-engineer an unfamiliar legacy .NET codebase and produce an architecture map.
2. Produce a prioritized technical-debt assessment (architecture, security, performance, reliability, testability).
3. Write an incremental modernization roadmap that keeps the application operational.
4. Capture legacy behavior in characterization tests **before** changing code.
5. Refactor hidden dependencies into constructor-injected abstractions without changing business rules.
6. Fix a real transactional-integrity defect and design an idempotency strategy.
7. Modernize integrations, configuration/secrets, data access and error handling to current .NET practice.
8. Migrate a single vertical slice to .NET 8 / ASP.NET Core.
9. Validate AI-generated changes with diff review, build, tests and an independent AI review pass.
10. Drive Claude Code in VS Code effectively: plan mode, permission modes, `@`-mentions, `CLAUDE.md`, checkpoints, subagents.

---



## 2. Business scenario

> **Client:** *Meridian Retail Bank* — a mid-size retail bank running a 12-year-old in-house core
> platform called **LegacyEnterpriseBanking**.
>
> **The situation.** The platform handles customer onboarding, accounts, money transfers, loan
> eligibility, transaction search, audit logging, notifications and a nightly interest batch. It runs on
> .NET Framework 4.8 on Windows Server 2016, which leaves extended support, and the bank's regulator has
> flagged three findings in the last audit:
>
> 1. A customer complaint where money left the source account but never arrived in the destination account.
> 2. Database credentials and third-party API keys committed to source control in `web.config`.
> 3. API error responses returning .NET stack traces to callers.
>
> **The pressure.** The team of four maintains this system while also delivering features. Two of the
> original developers have left. There is almost no automated test coverage. A previous "big bang rewrite"
> attempt was cancelled after nine months with nothing shipped.
>
> **Your mandate.** You have joined as the senior engineer leading modernization to .NET 8. Management's
> constraints are explicit:
>
> - **No big-bang rewrite.** The system must keep running throughout.
> - **Migrate feature by feature**, starting with **Money Transfer** (highest regulatory risk).
> - **Preserve business behavior** unless a behavior is proven to be a defect.
> - **No microservices.** The bank does not have the operational maturity for it.
> - **Every AI-assisted change must be reviewable** by a human and covered by tests.
>
> **Your first sprint deliverables** are exactly the ten artifacts listed in [Section 12](#12-student-deliverables).



### Why this application is a good training target

The sample repository intentionally contains: a god service, `new` dependencies hard-coded inside
business logic, SQL string concatenation, plaintext secrets, `WebClient`, `new HttpClient()`, `.Result`
blocking, a raw `Thread` with a swallowed `catch`, a missing transaction boundary around debit/credit,
N+1 queries, `SaveChanges()` in a loop, `ToList()` before `Where()`, static file logging, PII in logs,
stack-trace leakage, `dynamic` request models and no tests. Every finding you will make is real.

---



## 3. Prerequisites



### 3.1 Required software


| Requirement                       | Minimum                                                                           | Verify with                     |
| --------------------------------- | --------------------------------------------------------------------------------- | ------------------------------- |
| Windows 10/11 or Windows Server   | —                                                                                 | —                               |
| VS Code                           | 1.94.0+                                                                           | `code --version`                |
| Claude Code extension for VS Code | v2.1.257+ recommended                                                             | Extensions view → "Claude Code" |
| Claude account                    | Any paid plan (Pro/Max/Team/Enterprise) or Console account. **No API key needed** | Sign-in screen in the panel     |
| .NET SDK                          | 9.0.x (compiles both `net48` and `net8.0`)                                        | `dotnet --list-sdks`            |
| ASP.NET Core runtime 8.x          | for running the .NET 8 slice in Module 6                                          | `dotnet --list-runtimes`        |
| Git                               | 2.30+                                                                             | `git --version`                 |
| SQL Server (optional)             | Express/LocalDB/Developer                                                         | only needed for end-to-end runs |


> **SQL Server is optional.** Modules 0–6 are all completable without a database. The `database\*.sql`
> scripts are provided if you want to run the API end-to-end.



### 3.2 Verify your environment

Open **PowerShell** and run:

```powershell
dotnet --list-sdks
dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App 8"
git --version
code --version
```

Expected: at least one 9.0.x SDK, an `Microsoft.AspNetCore.App 8.0.x` runtime line, a Git version, and
a VS Code version ≥ 1.94.

### 3.3 Install the Claude Code extension

1. In VS Code press `Ctrl+Shift+X` to open the Extensions view.
2. Search for **Claude Code** and click **Install**.
3. Reload the window: `Ctrl+Shift+P` → **Developer: Reload Window**.
4. Open any file, then click the **Spark icon** (✱) in the top-right editor toolbar — or press
  `Ctrl+Shift+Esc` to open Claude in a new tab.
5. Click **Sign in** and complete authorization in the browser.

Optional but recommended — install the standalone CLI so you can also run `claude` in VS Code's
integrated terminal (the extension bundles its own private copy and does **not** put `claude` on your PATH):

```powershell
claude --version
```

---



## 4. Claude Code in VS Code — the 10-minute primer

Do this once before Module 0. Everything in this lab assumes the current extension experience.

### 4.1 Where Claude lives


| Action                             | How                                                                            |
| ---------------------------------- | ------------------------------------------------------------------------------ |
| Open Claude                        | Spark icon (✱) in editor toolbar, or `Ctrl+Shift+Esc` for a new tab            |
| Sessions list                      | Spark icon in the Activity Bar (left sidebar)                                  |
| Move the panel                     | Drag the tab to the secondary sidebar (right), primary sidebar, or editor area |
| Jump between editor and prompt box | `Ctrl+Esc`                                                                     |
| Second parallel conversation       | Command Palette → **Claude Code: Open in New Tab** / **Open in New Window**    |
| Resume earlier work                | **Session history** button at the top of the panel                             |
| Hide tool noise (Focus view)       | `Ctrl+Alt+F`                                                                   |




### 4.2 Permission modes — the single most important control

Click the **mode indicator at the bottom of the prompt box** to switch:


| Mode                   | Behavior                                                                                                                                                        | Use in this lab                                       |
| ---------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| **Plan**               | Claude describes what it will do and waits for approval. VS Code opens the plan as a full Markdown document you can **comment on inline** before Claude starts. | Modules 1, 2, 3 and the design step of every refactor |
| **Manual**             | Asks before each file edit and most shell commands; shows a side-by-side diff you can edit before accepting.                                                    | Modules 4, 5, 6 — the default for all code changes    |
| **Auto**               | A classifier reviews most actions instead of asking you.                                                                                                        | Only for bulk mechanical edits you will diff anyway   |
| **Edit automatically** | Edits without asking.                                                                                                                                           | Avoid during this lab                                 |


> **Lab rule:** Modules 1–3 run in **Plan** mode. Modules 4–6 run in **Manual** mode.
> If you find yourself accepting diffs without reading them, you are no longer doing the lab.



### 4.3 Giving Claude the right context

- `@`**-mention files and folders:** type `@BankingService` (fuzzy matching works), `@src/LegacyBanking.Data/` for a folder.
- **Line ranges:** select code in the editor and press `Alt+K` to insert a reference like `@BankingService.cs#59-95`.
- **Terminal output:** reference it with `@terminal:powershell` instead of pasting build errors.
- **Problems panel:** Claude can read language-server diagnostics (errors/warnings) directly.
- **Multi-line prompts:** `Shift+Enter` adds a newline without sending — paste the long prompts from this manual straight into the box.



### 4.4 Commands you will actually use


| Command            | Purpose                                                                                      |
| ------------------ | -------------------------------------------------------------------------------------------- |
| `/init`            | Generate a starter `CLAUDE.md` for the repo (Module 0)                                       |
| `/memory`          | Edit `CLAUDE.md` and view auto-memory                                                        |
| `/context`         | Show what is consuming the context window; confirm `CLAUDE.md` loaded under **Memory files** |
| `/compact`         | Summarize the conversation to free context and keep going                                    |
| `/clear`           | Start a fresh conversation (keeps `CLAUDE.md`) — required before the independent review      |
| `/rewind`          | Roll code and/or conversation back to a checkpoint (aliases `/undo`, `/checkpoint`)          |
| `/code-review`     | Bundled skill: reviews changes and reports findings                                          |
| `/usage`           | Your plan limits and what is consuming them                                                  |
| `/mcp`, `/plugins` | Manage MCP servers and plugins                                                               |
| `/model`           | Switch model mid-session (or click the model name at the bottom of the prompt box)           |


**Model choice for this lab:** use the strongest available reasoning model (Opus-class) for Modules 1–3
and all design/review steps; a faster Sonnet-class model is fine for mechanical edits in Modules 4–5.
Turn on **extended thinking** from the `/` command menu for the assessment and the transaction-integrity design.

### 4.5 Checkpoints — your undo button

The extension tracks Claude's file edits. **Hover any message** in the conversation to reveal the rewind
button, then choose:

- **Fork conversation from here** — new branch of the conversation, code untouched
- **Rewind code to here** — revert file changes, keep the conversation
- **Fork conversation and rewind code** — both

Checkpoints cover Claude's edits, not your manual ones — which is why Module 0 also creates a **Git**
baseline. Use `git diff` as the source of truth.

---



## 5. The sample application



### 5.1 Repository layout

```text
LegacyEnterpriseBanking/
├── LegacyEnterpriseBanking.sln          ← broken: contains no project configurations (Module 0 fixes it)
├── README.md
├── .gitignore
├── database/
│   ├── 001-create-database.sql          ← Customers, Accounts, Transactions, Loans, AuditLogs
│   └── 002-seed-data.sql                ← 2 customers, 3 accounts (ACC-10001/2/3), 2 transactions
└── src/
    ├── LegacyBanking.Domain/            ← entities + TransferRequest (no logic)
    ├── LegacyBanking.Data/              ← EF6 DbContext + 4 repositories (incl. raw ADO.NET)
    ├── LegacyBanking.Integrations/      ← CreditBureau, PaymentGateway, Notification clients
    ├── LegacyBanking.Business/          ← BankingService (god service), CustomerOnboardingService, LegacyLogger
    ├── LegacyBanking.Web/               ← ASP.NET Web API 2 controllers + web.config
    └── LegacyBanking.Batch/             ← nightly interest console app
```



### 5.2 Dependency direction (what you should discover in Module 1)

```text
        LegacyBanking.Web            LegacyBanking.Batch
      (Web API 2 controllers)        (nightly interest)
                 |                            |
                 v                            |
        LegacyBanking.Business                 |
     (BankingService, Onboarding)              |
            /            \                     |
           v              v                    v
  LegacyBanking.Data   LegacyBanking.Integrations
   (EF6 + ADO.NET)      (WebClient / HttpClient)
           |                     |
           v                     v
      SQL Server        Credit Bureau · Payment Gateway · Notifications
```

Note the smell already visible from the diagram: **Business depends directly on Data and Integrations
concrete classes**, and `Web` also references `Data` directly (`TransactionController`,
`CustomerController`) — bypassing the business layer entirely.

### 5.3 The Money Transfer feature — your working set

Keep these four files open in tabs for the whole lab:

```text
src/LegacyBanking.Web/Controllers/TransferController.cs
src/LegacyBanking.Business/BankingService.cs              (TransferMoney, lines ~19-133)
src/LegacyBanking.Data/Repositories/AccountRepository.cs
src/LegacyBanking.Data/Repositories/TransactionRepository.cs
```

---



## 6. Module 0 — Environment, baseline and a build that actually works

**Duration:** 30 min · **Mode:** Manual · **Goal:** a Git baseline, a project `CLAUDE.md`, and a solution that compiles.

> This module is not busywork. The repository as shipped **does not compile**, and its `.sln` builds
> nothing at all. Getting a green build *before* you change anything is the first discipline of legacy
> modernization: without it you cannot tell your breakage from pre-existing breakage.



### Step 0.1 — Open the project and create a Git baseline

```powershell
cd "F:\#Training\training-claude-ai-advance-v3\01 - Legacy .NET Modernization with Claude\LegacyEnterpriseBanking"
code .
```

> Adjust the path if your copy of the training material lives elsewhere.

In VS Code's integrated terminal (`Ctrl+` ```):

```powershell
git init
git add .
git commit -m "baseline: legacy application as shipped"
git checkout -b modernization
```

**Checkpoint:** `git status` reports a clean tree on branch `modernization`.

### Step 0.2 — Generate the project memory file

Open Claude (`Ctrl+Shift+Esc`) and run:

```text
/init
```

Claude scans the repository and proposes a `CLAUDE.md`. Accept it, then **replace its contents** with the
lab's engineering contract (paste the whole block from [Section 13](#13-the-claude-master-prompt-claudemd)).
Confirm it loaded:

```text
/context
```

`CLAUDE.md` must appear under **Memory files**.

**Why this matters:** these rules are re-read at the start of every session and every `/clear`. They are
what stop Claude from "helpfully" rewriting the whole application at 3 p.m. on day two.

### Step 0.3 — Observe that the solution file is a lie

```powershell
dotnet build .\LegacyEnterpriseBanking.sln
```

You will see this **passing** build:

```text
warning : Unable to find a project to restore!
Build succeeded.
    1 Warning(s)
    0 Error(s)
```

Nothing was compiled. The `.sln` lists six projects but has no `ProjectConfigurationPlatforms` section, so
MSBuild builds an empty set. **A green build that builds nothing is worse than a red build.**

Ask Claude to explain it in its own words (Plan mode, no edits):

```text
Read @LegacyEnterpriseBanking.sln.

`dotnet build LegacyEnterpriseBanking.sln` prints "Unable to find a project to restore!" and then
"Build succeeded" without compiling anything.

Explain exactly why, and what is missing from the solution file.

Do not modify any files yet.
```



### Step 0.4 — Rebuild the solution file

```powershell
Remove-Item .\LegacyEnterpriseBanking.sln -Force
dotnet new sln -n LegacyEnterpriseBanking
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
dotnet build .\LegacyEnterpriseBanking.sln
```

Now you get a **real** result — three compile errors:

```text
src\LegacyBanking.Data\Repositories\TransactionRepository.cs(15,17): error CS0103:
    The name 'ConfigurationManager' does not exist in the current context
src\LegacyBanking.Data\Repositories\AccountRepository.cs(33,43): error CS0103:
    The name 'EntityState' does not exist in the current context
src\LegacyBanking.Data\Repositories\CustomerRepository.cs(37,48): error CS0103:
    The name 'EntityState' does not exist in the current context
```



### Step 0.5 — Fix the build with Claude (minimal changes only)

Switch to **Manual** mode and send:

```text
The solution now compiles the six projects and reports three errors:

- src/LegacyBanking.Data/Repositories/TransactionRepository.cs(15,17) CS0103 'ConfigurationManager'
- src/LegacyBanking.Data/Repositories/AccountRepository.cs(33,43) CS0103 'EntityState'
- src/LegacyBanking.Data/Repositories/CustomerRepository.cs(37,48) CS0103 'EntityState'

Read @src/LegacyBanking.Data/ and @src/LegacyBanking.Data/LegacyBanking.Data.csproj.

Diagnose the root cause of each error and fix it with the smallest possible change.

Constraints:
- Do not change any behavior, query, SQL string or business rule.
- Do not reformat, rename or "clean up" anything.
- Do not add packages.
- Missing using directives or missing assembly references only.

Before editing, list each file you will touch and the exact line you will add.
```

Expected fix set (four one-line changes in total):


| File                                                        | Change                                             |
| ----------------------------------------------------------- | -------------------------------------------------- |
| `src/LegacyBanking.Data/LegacyBanking.Data.csproj`          | add `<Reference Include="System.Configuration" />` |
| `src/LegacyBanking.Data/Repositories/AccountRepository.cs`  | add `using System.Data.Entity;`                    |
| `src/LegacyBanking.Data/Repositories/CustomerRepository.cs` | add `using System.Data.Entity;`                    |
| `src/LegacyBanking.Web/LegacyBanking.Web.csproj`            | add `<Reference Include="Microsoft.CSharp" />`     |


The fourth appears only after the first three are fixed, when the `Web` project finally compiles:

```text
src\LegacyBanking.Web\Controllers\CustomerController.cs(36,29): error CS0656:
    Missing compiler required member 'Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create'
```

Feed it back with `@terminal:powershell` and let Claude connect it to the `dynamic request` parameter in
`CustomerController.Register` — a legacy smell you will revisit in Module 2.

```
@terminal:powershell

The build is now showing a new error.

Analyze this terminal output and determine:
1. What is causing the error?
2. Which source file/code is responsible?
3. What is the minimum change required to make the project compile?
4. Do not change application behavior.
```

Fix will be
```
<Reference Include="Microsoft.CSharp" />
```

### Step 0.6 — Verify and commit the baseline

```powershell
dotnet build .\LegacyEnterpriseBanking.sln
git diff
git add .
git commit -m "build: restore solution configurations and fix baseline compile errors"
```

**Checkpoint — do not continue until all four are true:**

- [ ] `Build succeeded. 0 Error(s)` **and** the log shows six `.csproj` files compiling
- [ ] `git diff HEAD~1` shows only the four minimal changes above
- [ ] `CLAUDE.md` exists and appears under `/context` → Memory files
- [ ] You can explain, unprompted, why the original `dotnet build` "succeeded"

**Deliverable 0:** the commit hash of your green baseline.

---



## 7. Module 1 — Understand the legacy application

**Duration:** 30 min · **Mode:** Plan (no edits) · **Goal:** an architecture map and a traced business flow.

### Step 1.1 — Repository-level architecture analysis

Switch the mode indicator to **Plan**, then send:

```text
Act as a senior .NET architect.

Analyze this legacy .NET repository. Do not modify any code.

Provide:
1. Business purpose of the system
2. Main projects/modules
3. Responsibility of each project
4. Frameworks and technologies in use, with the file that proves each one
5. Database approach (ORM vs raw SQL — where each is used)
6. External integrations
7. Project dependency direction, and any dependency that violates layering
8. The important business flows

Then create a simple text architecture diagram.

Keep the analysis concise. Cite file paths for every claim.
```

**What good output looks like:** Claude should name six projects, identify EF6 *and* raw ADO.NET coexisting
in `LegacyBanking.Data`, spot that `LegacyBanking.Web` references `LegacyBanking.Data` directly, and
produce something close to the diagram in [Section 5.2](#52-dependency-direction-what-you-should-discover-in-module-1).

**Verify, don't trust.** Pick any three claims and confirm them yourself with `Ctrl+P`. Note any
hallucination in your notes — it is a deliverable.

### Step 1.2 — Trace the Money Transfer flow

Open the four working-set files, then send:

```text
Trace the complete Money Transfer flow.

Start at @src/LegacyBanking.Web/Controllers/TransferController.cs and follow the code through:
controller → business service → account repository → transaction persistence → audit → notification.

Use @src/LegacyBanking.Business/BankingService.cs and @src/LegacyBanking.Data/Repositories/.

For every step, give me a table row with:
- step name
- file and line range
- what it does
- dependency it calls and how that dependency is obtained
- database change (which table, which operation, which DbContext instance)
- possible failure point and what state the system is left in if it fails there

Do not modify code.
```

Expected conceptual flow:

```text
Transfer Request → TransferController.Transfer
                 → BankingService.TransferMoney
                 → validate request (6 guard clauses)
                 → load source + destination accounts
                 → debit source   → SaveChanges (DbContext #1)
                 → credit target  → SaveChanges (DbContext #2)
                 → insert DEBIT + CREDIT rows → SaveChanges (DbContext #3)
                 → static file log (serializes the whole request)
                 → new Thread → audit insert + email notification (exceptions swallowed)
                 → return reference "TXN-<ticks>"
```



### Step 1.3 — Find the money-loss window

This is the regulator's finding #1. Ask specifically:

```text
In @src/LegacyBanking.Business/BankingService.cs, TransferMoney performs the debit, the credit and the
ledger inserts through three separate DbContext instances, each with its own SaveChanges.

Walk me through the exact system state after each of these failure points:
1. debit saved, credit throws
2. debit and credit saved, transaction insert throws
3. everything saved, the background Thread throws

For each: is money lost, duplicated, or intact? Is there a ledger record? Is there an audit record?
Which of these would a customer notice, and which would only an auditor notice?

Do not modify code.
```



### Checkpoint

Write two or three sentences each — you must be able to answer without re-reading the code:

- [ ] Where do transfer business rules live, and what else lives in the same class?
- [ ] How many separate database connections does one transfer open, and why does that matter?
- [ ] Which external systems does a transfer touch, and are they inside or outside the failure path?
- [ ] Name the precise line after which a failure leaves the bank's ledger inconsistent.

**Deliverable 1:** current-state architecture diagram + Money Transfer flow table (export with `/export module1-understand.md`).

---



## 8. Module 2 — Technical debt and modernization assessment

**Duration:** 45 min · **Mode:** Plan (no edits) · **Goal:** a prioritized, evidence-backed findings list.

Rather than four separate reviews, run **one consolidated assessment** — it produces better prioritization
because Claude can weigh a security finding against a reliability finding in the same pass.

### Step 2.1 — The consolidated assessment prompt

Turn on **extended thinking** from the `/` command menu first, then send:

```text
Act as a senior .NET modernization architect.

Analyze this repository for modernization risks. Do not modify code.

Classify every finding into exactly one category:
1. Architecture & maintainability
2. Outdated .NET patterns
3. Security
4. Performance
5. Reliability
6. Testability

For each finding provide a table row:
| # | Category | File:line | Current pattern | Problem | Business/technical impact | Severity (Critical/High/Medium/Low) | Recommended modernization |

Rules:
- Cite a real file and line for every finding. If you cannot cite one, drop the finding.
- Limit the result to the 15 most important findings, ordered by severity then blast radius.
- Judge severity from the perspective of a regulated retail bank.
- Do not propose a rewrite, a microservice split, or a new framework.
```



### Step 2.2 — Validate the findings against the code

**Do not accept the table as-is.** For each Critical/High finding, open the cited file and confirm it.
Below is the categorised set of patterns that genuinely exist in this repository — use it to score
Claude's recall (what it found) and precision (what it invented).

**Architecture & maintainability**

- `BankingService` mixes validation, persistence, business rules, integration, auditing, logging and notification (god service).
- Controllers construct services directly: `new BankingService()` in `TransferController` and `CustomerController`.
- Services construct repositories directly: `new AccountRepository()`, `new CustomerRepository()`, `new AuditRepository()`.
- `LegacyBanking.Web` bypasses the business layer and calls `TransactionRepository` / `CustomerRepository` directly.
- Magic strings everywhere: `"ACTIVE"`, `"DEBIT"`, `"CREDIT"`, `"POSTED"`, `"SAVINGS"`, `"APPROVED"`, `"PENDING"`.
- Business policy hard-coded as magic numbers in `CalculateLoanEligibility` (600/680/720/760, 0.40/0.60/0.80).

**Outdated .NET patterns**

- `WebClient` + `DownloadString` in `CreditBureauClient`.
- `new HttpClient()` per call in `PaymentGatewayClient` (socket exhaustion).
- `.Result` blocking on `PostAsync` in `PaymentGatewayClient`.
- Raw `new Thread(...)` fire-and-forget in `TransferMoney`.
- `ConfigurationManager` / `web.config` instead of `IConfiguration` + `appsettings.json`.
- `static LegacyLogger` writing to a file with `File.AppendAllText` instead of `ILogger<T>`.
- Hand-built JSON string in `PaymentGatewayClient.Pay`.
- `DateTime.Now` (not `UtcNow`, not abstracted) for ledger timestamps and generated references.
- `dynamic` request model in `CustomerController.Register`.
- Entirely synchronous I/O — no `async`/`await` anywhere in the codebase.

**Security**

- SQL injection: string-concatenated query in `TransactionRepository.Search`, reachable unauthenticated from `GET api/transactions/search`.
- Plaintext secrets committed in `web.config`: DB password, `CreditBureauApiKey`, `PaymentGatewayToken`.
- PII in logs: `JsonConvert.SerializeObject(request)` in `TransferMoney`; `NationalId` written to the log in `CustomerOnboardingService`.
- National ID and API key passed in a **query string** in `CreditBureauClient`.
- Stack-trace leakage: `return BadRequest(ex.ToString())` in `TransferController`.
- `<customErrors mode="Off"/>` and `debug="true"` in `web.config`.
- No authentication or authorization anywhere — no `[Authorize]`, no ownership check that the caller owns the source account.

**Performance**

- `db.Customers.ToList()` then `.Where(...)` in `CustomerRepository.SearchByName` — full table into memory, plus `ToLower()` defeating any index.
- N+1 queries in `GetCustomerDashboard`: a new `BankingDbContext` **and** a `COUNT` query per account.
- `SaveChanges()` inside the loop in `LegacyBanking.Batch/Program.cs` — one round trip per account.
- Blocking HTTP calls on request threads (`.Result`, `DownloadString`).
- `Include("Customer")` eager-loads the customer on every account lookup, needed or not.
- Three separate `DbContext` instances (three connections) for one transfer.

**Reliability**

- Debit and credit are not atomic — no transaction, no `TransactionScope`, three `SaveChanges` calls.
- Background `Thread` with an empty `catch {}` — audit and notification failures vanish silently.
- No idempotency: a client retry posts the transfer twice.
- No concurrency control — `db.Entry(account).State = Modified` writes the whole row, so two concurrent transfers on one account silently lose an update (no rowversion / optimistic concurrency).
- `CustomerRepository.GetById` never disposes its `DbContext`.
- External integration inside the onboarding write path in `CustomerOnboardingService.Register`, with no timeout, retry or circuit breaker.
- Generic `throw new Exception(...)` for every business-rule violation — callers cannot distinguish validation from failure.

**Testability**

- Dependencies created with `new` inside methods: `AccountRepository`, `CustomerRepository`, `AuditRepository`, `CreditBureauClient`, `LegacyNotificationClient`.
- `new BankingDbContext()` inline in business logic.
- `DateTime.Now` and `DateTime.Now.Ticks` — non-deterministic reference generation.
- `static LegacyLogger` — unmockable, writes to the filesystem.
- `Console.WriteLine` notifications.
- Zero test projects in the repository.

> **Scoring exercise (do this):** count how many of the above Claude found, and how many findings it
> reported that you could not confirm in the code. Record both numbers. This calibrates how much you
> should trust its later output.



### Step 2.3 — Inspect three findings in depth



#### Example A — SQL injection

Open `src/LegacyBanking.Data/Repositories/TransactionRepository.cs` and read lines 21–29.

```text
Read @src/LegacyBanking.Data/Repositories/TransactionRepository.cs.

1. Explain precisely why the Search query is unsafe.
2. Give me a concrete malicious value for the `status` parameter that would return every transaction
   in the database, and one that would be destructive.
3. Trace which public HTTP endpoint reaches this method and what authentication protects it.
4. Show two safe alternatives: parameterized SqlCommand, and an EF-based query. Compare them.

Do not modify the code yet.
```

**Discussion point:** `TransactionController.Search` exposes both parameters straight from the query string
with no authentication. Note in your findings that severity is driven by *reachability*, not just by the pattern.

#### Example B — Performance

Read `CustomerRepository.SearchByName` (lines 17–28).

```text
Read @src/LegacyBanking.Data/Repositories/CustomerRepository.cs, method SearchByName.

Explain the database, network and memory impact if the Customers table contains 2,000,000 rows.

Cover:
- what SQL is actually sent to the server
- how many rows cross the wire
- managed heap and GC impact per request
- what happens under 50 concurrent requests
- why ToLower() would still hurt even if the Where clause were translated to SQL

Then show the corrected EF query and explain what changes in the generated SQL.
```

The shape of the difference:

```text
CURRENT   Database → 2,000,000 rows → Application → filter in memory → a few rows
CORRECT   Database → filter (indexed) → a few rows → Application
```



#### Example C — Testability

Read `BankingService.TransferMoney` (lines 19–133).

```text
Read @src/LegacyBanking.Business/BankingService.cs, method TransferMoney.

1. List every dependency created inside the method, with its line number.
2. For each, explain what would have to exist on my machine for a unit test to run.
3. Explain which assertions are impossible today (e.g. "notification was sent", "audit was written").
4. Recommend the MINIMUM set of abstractions to make this method unit-testable.
   Justify each one. If an abstraction is not needed for a test assertion, do not propose it.
5. Tell me which parts of this method are already testable today with no refactoring at all.

Do not modify code.
```

Point 5 matters — the six guard clauses run before any dependency is touched, which is exactly what you
will exploit in Module 3.

### Checkpoint

- [ ] A findings table with ≥ 12 confirmed findings, each with a real `file:line`
- [ ] Your recall/precision score for Claude's assessment
- [ ] You can explain why the SQL injection is Critical rather than High
- [ ] You can name the three parts of `TransferMoney` that are testable without refactoring

**Deliverable 2:** technical assessment — your top 10 findings, with severity and evidence.

---



## 9. Module 3 — Modernization plan and safety net

**Duration:** 40 min · **Mode:** Plan, then Manual for the test project · **Goal:** a phased roadmap and executable characterization tests.

### Step 3.1 — Generate the roadmap

```text
Create an incremental modernization plan for this application.

Current: .NET Framework 4.8, ASP.NET Web API 2, EF6, raw ADO.NET, SQL Server, no tests.
Target:  .NET 8, ASP.NET Core Web API, EF Core, DI, async/await, structured logging, xUnit.

Constraints:
- no big-bang rewrite
- preserve business behavior
- the application must remain operational at every step
- migrate feature by feature, starting with Money Transfer
- no microservices
- a team of 4 developers who are also delivering features

Group the work into 5-6 phases. For each phase give me:
- objective
- major changes (referencing real files in this repo)
- what is explicitly OUT of scope for that phase
- risk, and the specific rollback story
- validation approach (what proves the phase is done)
- rough effort in developer-weeks

Order the phases so that the highest regulatory risk is addressed earliest, and so that no phase
depends on a later phase.
```

A defensible sequence for this repository:

```text
Phase 1  Baseline & safety net      build, Git, CLAUDE.md, characterization tests
Phase 2  Security & critical risks  SQL injection, secrets, stack traces, authorization
Phase 3  Testability & DI           interfaces + constructor injection, Transfer slice first
Phase 4  Reliability & async        transaction boundary, idempotency, async I/O, remove raw Thread
Phase 5  Data & performance         ToList/N+1/SaveChanges-in-loop, AsNoTracking, projections
Phase 6  .NET 8 vertical slice      Money Transfer on ASP.NET Core, side-by-side with legacy
```

**Discussion:** why is Security *before* DI, when DI would make security fixes easier? (Answer: the SQL
injection is unauthenticated and reachable in production today. Fix exploitable holes before pretty code.)

### Step 3.2 — Design the characterization tests

Characterization tests capture what the code **does today**, not what it *should* do. Bugs and all — the
missing transaction boundary gets captured as current behavior, then fixed deliberately in Module 4.

```text
Analyze @src/LegacyBanking.Business/BankingService.cs, method TransferMoney.

Create characterization test scenarios that capture its CURRENT observable behavior, covering:
- successful transfer
- null request
- missing/blank source account
- missing/blank target account
- same source and target
- zero amount and negative amount
- source account not found
- target account not found
- inactive source account
- inactive target account
- insufficient balance
- exact-balance transfer (boundary)
- the shape of the returned reference string
- notification failure

For each scenario give me a table row:
| Scenario | Input | Current observable behavior (exact exception type and message, or return value) | Requires a database? | Requires the Module 4 refactoring? |

Be precise about exception types and messages — copy them from the code, do not guess.

Then tell me which scenarios I can automate TODAY against the unrefactored code, and why the rest cannot
be automated yet.
```

Claude should conclude that the guard clauses (null request, blank accounts, same account, non-positive
amount) are reachable with **no database**, because they execute before `new AccountRepository()` on line 36.
Everything else needs either SQL Server or the Module 4 seams.

### Step 3.3 — Create the test project and make it green

Switch to **Manual** mode. Run:

```powershell
dotnet new xunit -n LegacyBanking.Tests -o tests\LegacyBanking.Tests
Remove-Item .\tests\LegacyBanking.Tests\UnitTest1.cs
```

> The `xunit` template does not accept `-f net48`, so it generates a `net9.0` project. You must retarget
> it — a `net9.0` test project cannot reference `net48` projects.

Replace `tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <LangVersion>latest</LangVersion>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\LegacyBanking.Business\LegacyBanking.Business.csproj" />
    <ProjectReference Include="..\..\src\LegacyBanking.Domain\LegacyBanking.Domain.csproj" />
  </ItemGroup>

</Project>
```

Now let Claude write the tests that need no database:

```text
Create the characterization tests that require no database, in
tests/LegacyBanking.Tests/TransferValidationCharacterizationTests.cs.

Target: xUnit on net48, referencing @src/LegacyBanking.Business/BankingService.cs.

Cover only the guard clauses that execute before line 36 (`new AccountRepository()`):
null request, blank source account, blank target account, identical source and target,
zero amount, negative amount.

Requirements:
- Assert the exact exception type AND the exact message string from the current code.
- Name each test so it reads as documented behavior, e.g. NullRequest_Throws.
- Use [Theory]/[InlineData] where the scenarios differ only by input value.
- Add a file-header comment stating these are characterization tests that lock in CURRENT behavior,
  including behavior we consider defective.
- Do NOT change any production code to make testing easier. Not one line.
```

Add the test project to the solution and run:

```powershell
dotnet sln add .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
dotnet test .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
```

Expected:

```text
Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6 - LegacyBanking.Tests.dll (net48)
```

(The exact count depends on how Claude splits `[Theory]` cases — 4 to 8 passing tests is normal. **Zero
failures is not negotiable.**)

For the scenarios that cannot run yet, ask for documentation rather than fake tests:

```text
For the scenarios that need a database or the Module 4 refactoring, add them to the same test class as
[Fact(Skip = "...")] stubs. The Skip reason must state exactly what is missing, e.g.
"Requires IAccountRepository seam (Module 4)". Put the expected behavior in the test body as a comment.

Do not use a real database. Do not silently pass.
```

```powershell
dotnet test .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
git add .
git commit -m "test: characterization tests for TransferMoney validation rules"
```



### Checkpoint

- [ ] Roadmap of 5–6 phases with rollback and validation per phase
- [ ] All non-skipped tests pass on `net48`
- [ ] Skipped tests state precisely what unblocks them
- [ ] No production file was modified in this module (`git diff HEAD~1 --stat` shows only `tests/` and the `.sln`)

**Deliverables 3 & 5:** the roadmap and the characterization test suite.

---



## 10. Module 4 — Refactor the Transfer feature

**Duration:** 45 min · **Mode:** Manual · **Goal:** injectable dependencies, one atomic transaction, an idempotency design.

The rule for this whole module: **behavior-preserving refactoring first, deliberate behavior change second, and never both in the same commit.**

### Step 4.1 — Remove hidden dependencies

Current pattern (`BankingService.cs` line 36 and following):

```csharp
var accountRepository = new AccountRepository();
```

Prompt:

```text
Refactor ONLY the Money Transfer feature.

Introduce the minimum abstractions needed:
IAccountRepository, ITransactionRepository, IAuditService, INotificationService

Requirements:
- Use constructor injection.
- Keep the existing concrete classes as the implementations; extract interfaces from what they already do.
- Do NOT change transfer business rules, validation order, exception types or exception messages.
- Do NOT change the returned reference format.
- Keep the existing behavior of the background thread for now; we address it in Step 4.2.
- Leave Customers, Loans, Onboarding and Batch untouched.
- The existing characterization tests must still pass unchanged.

Before editing, show me:
1. every file you will create or modify
2. the proposed design, including where TransferController gets its dependencies now that Web API 2
   has no built-in DI container
3. the risks, and which of my characterization tests would catch each risk

Wait for my approval before writing any code.
```

Target design:

```text
TransferController
       │
       ▼
  ITransferService ──► TransferService
                          ├── IAccountRepository
                          ├── ITransactionRepository
                          ├── IAuditService
                          └── INotificationService
```

> **Expect a real design question here.** ASP.NET Web API 2 has no built-in DI container. Claude will
> likely offer: (a) a `IDependencyResolver` / composition root in `WebApiConfig`, (b) a
> `Microsoft.Extensions.DependencyInjection` container wired manually, or (c) a poor-man's default
> constructor that news up the concrete types while a second constructor takes the interfaces.
> **Option (c) is the smallest step that keeps the app running and unblocks tests** — which is often the
> right answer under a "must remain operational" constraint. Push back if Claude proposes replacing the
> hosting model.

After the change:

```powershell
git diff
dotnet build .\LegacyEnterpriseBanking.sln
dotnet test .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
```

Now un-skip what the seams unblocked:

```text
The Transfer feature now has interface seams.

Convert the [Fact(Skip=...)] stubs that only needed those seams into real tests using hand-written fakes
(no mocking library). Cover: source not found, target not found, inactive source, inactive target,
insufficient balance, successful transfer, notification failure.

For the successful transfer, assert the observable outcomes: source debited, target credited,
two ledger rows written with the same reference, one DEBIT and one CREDIT.

These tests must document CURRENT behavior, including the non-atomic save sequence.
Do not fix the transaction bug in this step.
```

```powershell
dotnet test .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
git add . ; git commit -m "refactor: constructor-injected dependencies for Money Transfer"
```

**Checkpoint:** tests that previously could not exist now pass, and `git diff` contains no business-rule change.

### Step 4.2 — Fix transactional integrity

You now have a safety net, so you can change behavior deliberately.

Current sequence — three connections, three commits:

```text
debit  → SaveChanges   (connection 1)
credit → SaveChanges   (connection 2)
ledger → SaveChanges   (connection 3)
```

Prompt:

```text
Analyze the transaction consistency of the refactored TransferMoney.

1. What happens if the debit is saved and the credit then fails? Be specific about the customer impact
   and the ledger state.
2. Redesign the operation so that debit, credit, debit-ledger-row and credit-ledger-row either all
   succeed or all fail.
3. Explain, for .NET Framework 4.8 + EF6 specifically:
   - where the transaction boundary belongs and which layer should own it
   - the trade-offs between a single DbContext + DbContextTransaction and TransactionScope
   - what happens to the background thread's audit write if it participates in the transaction
   - how rollback interacts with the notification that has already been sent
4. Explain how two concurrent transfers on the same account behave today, and what optimistic
   concurrency would change. Note what schema change that needs.

Show me the complete design and the affected files. Do not write code until I approve.

Then tell me which of my existing characterization tests will FAIL after this change, and why that
failure is correct.
```

That last question is the point of the exercise. Tests asserting the non-atomic behavior *should* fail —
you update them **because the behavior intentionally changed**, and you say so in the commit message.

Target:

```text
BEGIN TRANSACTION
    debit source
    credit destination
    insert DEBIT ledger row
    insert CREDIT ledger row
COMMIT                          ── any failure ⇒ ROLLBACK, no partial state
```

Notification and audit move **outside** the transaction, after commit.

```powershell
dotnet build .\LegacyEnterpriseBanking.sln
dotnet test .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
git diff
git add . ; git commit -m "fix: make debit, credit and ledger writes atomic (BEHAVIOR CHANGE)"
```



### Step 4.3 — Design an idempotency strategy

```text
A client posts the same 10,000 rupee transfer twice because of a network retry: the first request
succeeded but the response was lost.

1. What happens today, end to end?
2. Recommend an idempotency strategy for this API. Cover:
   - where the idempotency key comes from and who generates it
   - the storage model, including the exact table/columns and unique constraint
   - the state machine for an in-flight duplicate that arrives before the first completes
   - what the API returns for a duplicate, and what HTTP status
   - retention/expiry of keys
   - how this interacts with the transaction boundary from Step 4.2
3. Contrast it with the alternatives: client-side dedupe, a natural business key, a time-window
   duplicate check. Say when each is the right choice.
4. Give me the migration path that does not break existing callers who send no key.

Design only. Do not write code.
```

```text
Request → idempotency key
            ├── already processed  → return the stored result (no second transfer)
            ├── in flight          → 409 Conflict / retry-after
            └── new                → process, store result under the key
```

Record the design in your deliverables. Implementation is optional if time is short — the point is that
**modernization includes business reliability, not just syntax upgrades.**

### Checkpoint

- [ ] `TransferService` receives all four dependencies through its constructor
- [ ] Transfer is atomic; a forced mid-transfer failure leaves balances and ledger unchanged
- [ ] Every behavior change is in its own commit, labelled as such
- [ ] Written idempotency design with a storage model and duplicate-handling states

**Deliverables 4 & 6:** the Transfer refactoring and the reliability improvement.

---



## 11. Module 5 — Modernize integrations, data and cross-cutting concerns

**Duration:** 45 min · **Mode:** Manual · **Goal:** modern HTTP, safe configuration, faster queries, safe error handling.

Pick **at least three of the four** steps. Do not try to fix everything — deliberate scoping is part of the skill.

### Step 5.1 — Modernize external HTTP calls

Read `CreditBureauClient.cs` (`WebClient`, IDs and API key in the query string) and
`PaymentGatewayClient.cs` (`new HttpClient()` per call, hand-built JSON, `.Result`).

```text
Modernize @src/LegacyBanking.Integrations/CreditBureauClient.cs and
@src/LegacyBanking.Integrations/PaymentGatewayClient.cs.

Apply:
- async/await end to end, no .Result and no .Wait()
- an injected HttpClient suitable for IHttpClientFactory (do not new up HttpClient)
- CancellationToken on every async method
- strongly typed configuration objects instead of ConfigurationManager lookups
- explicit timeouts
- structured error handling: distinguish transport failure, non-success status and unparseable payload
- proper JSON serialization instead of string concatenation
- move the national ID and API key OUT of the query string (header/body), and explain why

Constraints:
- Do not change the external API contract (URL shape, payload fields, auth scheme).
- Keep the public method signatures compatible where you can; where async forces a change, list every
  caller you must update.
- Explain how the async change propagates through BankingService and the Web API 2 controllers before
  you edit anything.
```

> **Watch for this trap:** making `GetCreditScore` async forces `CalculateLoanEligibility` async, which
> forces `CustomerController` async. Claude may instead "solve" it with `.Result` at the boundary —
> reintroducing the exact deadlock risk you removed. Reject that. `async` all the way up, or leave the
> sync wrapper explicitly documented as legacy debt.



### Step 5.2 — Separate configuration from secrets

Read `src/LegacyBanking.Web/web.config`: a database password, `CreditBureauApiKey`,
`PaymentGatewayToken`, `debug="true"` and `customErrors mode="Off"` — all committed to source control.

```text
Read @src/LegacyBanking.Web/web.config.

1. List every value that is a secret and must never be in source control, and every value that is
   ordinary configuration.
2. Recommend how each should be supplied for: local development, CI/CD, and production.
3. Show the target for the .NET 8 slice: appsettings.json structure, the options classes, and where
   secrets come from (User Secrets locally, environment variables in CI, Key Vault or the platform
   secret store in production).
4. Give me the remediation runbook for secrets that are ALREADY committed — including the fact that
   rotating them matters more than deleting them from the file, and what to do about Git history.
5. Explain what debug="true" and customErrors mode="Off" leak in production.

Do not put any real secret in any file you create.
```

```text
Configuration ──► appsettings.json / appsettings.{Environment}.json
Secrets       ──► User Secrets (local) │ environment variables (CI) │ Key Vault (production)
```



### Step 5.3 — Improve data access and performance

```text
Review the data access code in @src/LegacyBanking.Data/ and @src/LegacyBanking.Business/BankingService.cs
and @src/LegacyBanking.Batch/Program.cs.

Identify the three highest-impact performance problems. For each:
- name the pattern and cite file:line
- explain the SQL actually executed and how many round trips it causes
- estimate the impact at production scale (2 million customers, 5 million transactions)
- give the modern EF Core equivalent
- state which fix is safe to apply to the legacy EF6 code today, and which should wait for the .NET 8 slice

Then fix EXACTLY ONE of them — the one with the best impact-to-risk ratio — and justify your choice.
```

Strong candidate: `CustomerRepository.SearchByName`. The target shape for read-only queries:

```csharp
return await db.Customers
    .AsNoTracking()
    .Where(c => c.FullName.Contains(searchText))
    .Select(c => new CustomerSearchResult { Id = c.Id, FullName = c.FullName })
    .ToListAsync(cancellationToken);
```

Discuss why `.AsNoTracking()`, why a projection instead of the full entity, and why `ToLower()` disappears
(collation handles case-insensitivity; `ToLower()` prevents index use).

> Do not optimize everything. One measured fix with a stated rationale beats ten speculative ones.



### Step 5.4 — Error handling and logging

Today: `BadRequest(ex.ToString())` returns stack traces to callers; `LegacyLogger` appends the fully
serialized transfer request — including PII — to a file; the background thread swallows exceptions entirely.

```text
Review exception handling and logging across @src/LegacyBanking.Web/Controllers/,
@src/LegacyBanking.Business/BankingService.cs and @src/LegacyBanking.Business/LegacyLogger.cs.

Recommend a modern ASP.NET Core approach using:
- centralized exception handling (middleware / IExceptionHandler)
- ProblemDetails responses
- structured logging with ILogger<T> and message templates, not string concatenation
- correlation / trace IDs that tie an API response to its log entries
- PII-safe logging: which fields must never be logged, and how to redact them

Give me a table: for each exception type, what the CLIENT sees (status, body, whether it says why)
versus what is LOGGED internally (level, fields, PII treatment).

Also address the empty catch block in the background thread: what should be logged, at what level,
and who gets alerted.

Explain what changes for the legacy Web API 2 app versus the new .NET 8 slice.
```

```text
Exception ──► global handler ──┬──► client:  safe ProblemDetails + correlation ID
                               └──► internal: structured log, full detail, PII redacted
```



### Checkpoint

- [ ] No `.Result`, `.Wait()` or `new HttpClient()` left in the code you modernized
- [ ] No secret in any file you created; runbook written for the already-committed ones
- [ ] One performance fix applied, with a stated before/after rationale
- [ ] No API response can leak a stack trace
- [ ] `dotnet build` and `dotnet test` both green

**Deliverables 7 & 8:** one performance improvement and one security improvement.

---



## 12. Module 6 — Migrate one vertical slice to .NET 8

**Duration:** 45 min · **Mode:** Manual · **Goal:** Money Transfer running on ASP.NET Core, side by side with the legacy app.

**Migrate Money Transfer only.** Customers, Loans and Batch stay on .NET Framework 4.8 and keep running.

### Step 6.1 — Create the new API

```powershell
cd "F:\#Training\training-claude-ai-advance-v3\01 - Legacy .NET Modernization with Claude\LegacyEnterpriseBanking"
dotnet new webapi -n LegacyBanking.ModernApi -o src\LegacyBanking.ModernApi -f net8.0
dotnet sln add .\src\LegacyBanking.ModernApi\LegacyBanking.ModernApi.csproj
dotnet build .\src\LegacyBanking.ModernApi\LegacyBanking.ModernApi.csproj
```

> The .NET 9 SDK compiles `net8.0` fine. Running it needs the ASP.NET Core 8 runtime — you verified that
> in [Section 3.2](#32-verify-your-environment).



### Step 6.2 — Plan the migration before writing it

Switch to **Plan** mode:

```text
Help me migrate ONLY the Money Transfer vertical slice to .NET 8.

Source of truth: the refactored @src/LegacyBanking.Business/ transfer code, its interfaces, and my
characterization tests in @tests/LegacyBanking.Tests/.

Use:
ASP.NET Core Web API, built-in dependency injection, EF Core, async/await, CancellationToken,
ILogger<T>, ProblemDetails, strongly typed configuration (IOptions), xUnit.

Explicitly do NOT migrate Customers, Loans, Onboarding or Batch.

Rules:
- Preserve existing transfer behavior, except behaviors we identified as defects
  (non-atomic writes, swallowed exceptions, stack-trace leakage, PII logging) — those get fixed.
- Keep the atomic transaction boundary from Module 4.
- Replace generic Exception throws with a validation/domain exception type mapped to ProblemDetails.
- Do NOT create microservices. One API project, layered folders.
- Do NOT introduce MediatR, AutoMapper, a new ORM or any other dependency without justifying it
  against a concrete requirement I have.

Deliver, before writing code:
1. the folder structure with every file you will create
2. the EF Core mapping strategy for the existing SQL Server schema (see @database/001-create-database.sql)
   — the schema cannot change except for additions
3. how the legacy app and the new API coexist: routing, database sharing, transaction isolation,
   and what happens if both are called simultaneously for the same account
4. how my existing characterization tests port over, and which assertions must change
5. the risks and what you are NOT confident about
```

Reasonable target structure:

```text
src/LegacyBanking.ModernApi/
├── Program.cs                          ── DI, options, middleware, ProblemDetails
├── appsettings.json                    ── configuration only, no secrets
├── Api/
│   └── TransfersEndpoints.cs           ── POST /api/transfers
├── Application/
│   └── Transfers/
│       ├── ITransferService.cs
│       ├── TransferService.cs          ── business rules, atomic boundary
│       └── TransferRequest.cs / TransferResult.cs
├── Domain/
│   ├── Account.cs  Transaction.cs
│   └── TransferValidationException.cs
└── Infrastructure/
    ├── Persistence/  BankingDbContext.cs, AccountRepository.cs, TransactionRepository.cs
    └── Integrations/ NotificationClient.cs (IHttpClientFactory + typed client)
```



### Step 6.3 — Implement, then port the tests

Approve the plan, switch to **Manual**, and let Claude implement it in small reviewable chunks. Review
every diff. Then:

```text
Create tests/LegacyBanking.ModernApi.Tests (xUnit, net8.0) and port my characterization tests to the
new slice.

For each ported test state whether the assertion is:
(a) unchanged — behavior preserved
(b) changed — because we deliberately fixed a defect (name the defect)

Add new tests that were impossible before:
- rollback: nothing is persisted when the ledger insert fails
- ProblemDetails shape for a validation failure, including that no stack trace appears anywhere in the body
- CancellationToken is honoured

Use EF Core's in-memory or SQLite provider for the persistence tests and say which you chose and why.
```

```powershell
dotnet build .\LegacyEnterpriseBanking.sln
dotnet test .\LegacyEnterpriseBanking.sln
```

The coexistence picture:

```text
Legacy .NET Framework 4.8 app                New .NET 8 API
├── Customers      (unchanged)               └── POST /api/transfers
├── Loans          (unchanged)                       │
├── Batch          (unchanged)                       │
└── Transfers ──── traffic shifts ──────────────────►┘
                                                     │
                          both talk to the SAME SQL Server schema
```



### Checkpoint

- [ ] `LegacyBanking.ModernApi` builds on `net8.0` and its tests pass
- [ ] Legacy solution still builds and its tests still pass
- [ ] Transfer is atomic in the new slice
- [ ] Validation failures return `ProblemDetails` with no stack trace
- [ ] No new dependency was added without a justification you accepted
- [ ] Customers, Loans and Batch are untouched (`git diff --stat` proves it)

**Deliverable 9:** the .NET 8 Money Transfer vertical slice.

---



## 13. Final module — Validate the AI-generated modernization

**Duration:** 30 min · **Goal:** prove the work is correct, not merely plausible.

> AI-generated code is not automatically correct. It is *confidently formatted*. Every significant change
> passes four gates before you would raise a PR.



### Gate 1 — Review the diff

```powershell
git diff main...modernization --stat
git diff main...modernization
```

Check every item:

```text
□ Only the files you expected changed
□ No accidental business-rule change (validation order, thresholds, messages, reference format)
□ No secret, connection string or token introduced
□ No new NuGet dependency you did not approve
□ No excessive abstraction (an interface with exactly one implementation and no test using it)
□ No commented-out or dead code left behind
□ No TODO left in place of a real implementation
□ Async changes did not reintroduce .Result / .Wait()
□ Customers, Loans, Onboarding and Batch untouched
```



### Gate 2 — Build

```powershell
dotnet build .\LegacyEnterpriseBanking.sln
```

Zero errors. Read the **warnings** too — new warnings are findings.

### Gate 3 — Test

```powershell
dotnet test .\LegacyEnterpriseBanking.sln
```

**When a test fails, never say this:**

```text
Make the tests pass.
```

**Say this:**

```text
Analyze the failing test: <paste the failure, or reference @terminal:powershell>.

Determine whether the root cause is:
- production code
- test code
- an incorrect assumption in the test
- a business behavior we deliberately changed

Explain the root cause and cite the lines that prove it, BEFORE modifying anything.

If the correct fix is to change the test, state explicitly which behavior changed and why that change
is intentional. If you cannot tell, say so and tell me what you would need to inspect.
```



### Gate 4 — Independent AI review

Context poisons review: a session that wrote the code will defend it. Start clean.

1. Run `/clear` (or open a **new tab** with `Ctrl+Shift+Esc`) — `CLAUDE.md` still loads.
2. Try the bundled review skill first:

```text
/code-review
```

1. Then run the explicit principal-engineer review:

```text
Act as a principal .NET architect reviewing an AI-generated modernization pull request.

Review the diff between the `main` and `modernization` branches independently. Assume nothing in the
implementation is correct. Read the actual code, not the commit messages.

Check:
1. Business behavior changes — intended or accidental? Compare validation order, thresholds, exception
   types, exception messages and the reference format against the original.
2. Transaction and concurrency correctness — is the boundary real, and does rollback cover everything?
3. Security regressions — secrets, injection, PII in logs, error detail leakage, missing authorization.
4. Performance regressions — new N+1s, lost AsNoTracking, sync-over-async, per-call HttpClient.
5. Test coverage — do the tests actually assert the behavior that matters, or just that nothing threw?
   Name anything that is now untested but was covered before.
6. Breaking API changes for existing callers.
7. Over-engineering — abstractions with one implementation and no test, patterns with no requirement.

Classify every finding as Blocker, Major or Minor, with file:line and a recommended fix.

Finally recommend exactly one: ACCEPT / ACCEPT WITH CHANGES / REJECT, and justify it in three sentences.
```

1. Fix every **Blocker** and re-run gates 1–3. Record Majors and Minors as follow-up work.

**Deliverable 10:** the final AI code-review report, plus your own verdict on where the review itself
was right or wrong.

---



## 14. Student deliverables

Submit these ten artifacts. Export any Claude conversation with `/export <filename>.md`.


| #   | Deliverable                                                               | Produced in  | Format                        |
| --- | ------------------------------------------------------------------------- | ------------ | ----------------------------- |
| 0   | Green baseline commit hash                                                | Module 0     | one line                      |
| 1   | Current-state architecture diagram + Transfer flow table                  | Module 1     | 1 page                        |
| 2   | Technical assessment — top 10 findings with severity and `file:line`      | Module 2     | table                         |
| 3   | Modernization roadmap — max 6 phases with risk, rollback, validation      | Module 3     | 1–2 pages                     |
| 4   | Transfer refactoring — DI and improved boundaries                         | Module 4     | code + `git diff`             |
| 5   | Characterization / unit tests                                             | Modules 3–4  | code + passing test output    |
| 6   | One reliability improvement — transaction boundary and idempotency design | Module 4     | code + design note            |
| 7   | One performance improvement                                               | Module 5     | code + before/after rationale |
| 8   | One security improvement                                                  | Module 5     | code + secrets runbook        |
| 9   | .NET 8 Transfer vertical slice                                            | Module 6     | code + passing test output    |
| 10  | Final AI code-review report + your verdict                                | Final module | 1 page                        |




### Assessment rubric


| Criterion             | Weight | What "excellent" looks like                                                              |
| --------------------- | ------ | ---------------------------------------------------------------------------------------- |
| Evidence discipline   | 20%    | Every finding cites a real `file:line`; you caught at least one Claude hallucination     |
| Incrementalism        | 20%    | Small commits, one concern each; behavior changes separated from refactorings            |
| Test safety net       | 20%    | Tests written before refactoring; failures diagnosed by root cause, never "made to pass" |
| Technical correctness | 20%    | Atomic transaction genuinely works; no secret, injection or stack-trace leak remains     |
| Scope control         | 10%    | No rewrite, no microservices, no unjustified dependency, no abstraction without a test   |
| Review quality        | 10%    | Independent review found something real; you judged the review rather than accepting it  |


---



## 15. The Claude master prompt (`CLAUDE.md`)

Paste this into `CLAUDE.md` at the repository root in Module 0. It is re-read at the start of every
session and survives `/clear`.

```markdown
# LegacyEnterpriseBanking — AI Engineering Contract

You are assisting with an enterprise .NET modernization exercise on a legacy retail-banking platform.
Act as a senior .NET modernization engineer.

## Project facts
- Current: .NET Framework 4.8, ASP.NET Web API 2, EF6, raw ADO.NET, SQL Server
- Target: .NET 8, ASP.NET Core, EF Core — migrated incrementally, feature by feature
- Focus feature: Money Transfer (BankingService.TransferMoney)
- This is a training repository with intentional defects. Never treat existing code as a good example.

## Commands
- Build:  dotnet build .\LegacyEnterpriseBanking.sln
- Test:   dotnet test .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
- Legacy projects target net48; the modern slice targets net8.0; tests for legacy code must target net48.

## Rules
1. Inspect the code before drawing conclusions. Cite file and line for every claim.
2. Never rewrite the entire application. Modernize incrementally.
3. Preserve business behavior unless we have explicitly identified a defect.
4. Before changing code, state: the problem, the proposed solution, the affected files, the risk.
   Wait for approval.
5. Keep changes small enough to review in one sitting. One concern per change.
6. Avoid unnecessary abstractions. No interface without a caller or a test that needs it.
7. Do not introduce microservices.
8. Do not add a NuGet dependency without justifying it against a stated requirement.
9. Add or update tests for every important change.
10. Never change a failing test just to make it pass. Diagnose the root cause first and explain it.
11. Always consider security, performance, reliability and testability — in that order for this project.
12. After making changes, review your own diff critically and report what you are unsure about.
13. Never write a real secret into any file. Use placeholders.
14. Keep behavior-preserving refactorings and deliberate behavior changes in separate commits.

## Out of scope unless I say otherwise
Customers, Loans, Onboarding, Batch — do not modify these while we work on Transfer.
```

---



## 16. Troubleshooting


| Symptom                                                                                         | Cause                                                                           | Fix                                                                                                                         |
| ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `dotnet build` on the `.sln` says "Unable to find a project to restore!" then "Build succeeded" | The shipped `.sln` has no `ProjectConfigurationPlatforms`                       | Recreate it — [Step 0.4](#step-04--rebuild-the-solution-file)                                                               |
| `CS0103: 'EntityState' does not exist`                                                          | Missing `using System.Data.Entity;`                                             | [Step 0.5](#step-05--fix-the-build-with-claude-minimal-changes-only)                                                        |
| `CS0103: 'ConfigurationManager' does not exist`                                                 | `LegacyBanking.Data.csproj` lacks the `System.Configuration` reference          | [Step 0.5](#step-05--fix-the-build-with-claude-minimal-changes-only)                                                        |
| `CS0656: Missing compiler required member 'CSharpArgumentInfo.Create'`                          | `dynamic` used in `CustomerController` without the `Microsoft.CSharp` reference | Add `<Reference Include="Microsoft.CSharp" />` to `LegacyBanking.Web.csproj`                                                |
| `dotnet new xunit -f net48` fails                                                               | The template does not offer `net48`                                             | Generate the default project, then retarget the `.csproj` — [Step 3.3](#step-33--create-the-test-project-and-make-it-green) |
| Test project won't reference a legacy project                                                   | A `net9.0` test project cannot reference `net48`                                | Set `<TargetFramework>net48</TargetFramework>` in the test project                                                          |
| `net48` projects won't restore/build                                                            | .NET Framework reference assemblies missing                                     | Install VS Build Tools with the 4.8 targeting pack, or add `Microsoft.NETFramework.ReferenceAssemblies`                     |
| The .NET 8 API builds but won't run                                                             | ASP.NET Core 8 runtime missing                                                  | Install it, or target `net9.0`, or set `<RollForward>LatestMajor</RollForward>`                                             |
| Spark icon not visible                                                                          | No file open, VS Code < 1.94, or Restricted Mode                                | Open a file, check `Help → About`, trust the workspace                                                                      |
| Claude edits files you didn't approve                                                           | Mode is Auto or Edit automatically                                              | Click the mode indicator, switch to **Manual**                                                                              |
| Claude drifts, forgets constraints, invents files                                               | Context window exhausted                                                        | `/context` to inspect, then `/compact`; `/clear` between modules                                                            |
| Claude broke something and you can't tell what                                                  | —                                                                               | Hover the message → **Rewind code to here**, or `git diff` / `git checkout -- <file>`                                       |
| Answers ignore your rules                                                                       | `CLAUDE.md` not loading                                                         | `/context` → check **Memory files**; keep it under ~200 lines                                                               |


---



## Appendix A — Verified command reference

Every command below was executed against this repository on Windows/PowerShell with .NET SDK 9.

```powershell
# --- Module 0: baseline ---------------------------------------------------
cd "F:\#Training\training-claude-ai-advance-v3\01 - Legacy .NET Modernization with Claude\LegacyEnterpriseBanking"
code .
git init
git add .
git commit -m "baseline: legacy application as shipped"
git checkout -b modernization

# repair the solution file (the shipped one builds nothing)
Remove-Item .\LegacyEnterpriseBanking.sln -Force
dotnet new sln -n LegacyEnterpriseBanking
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
dotnet build .\LegacyEnterpriseBanking.sln

# build one project at a time while fixing errors
dotnet build .\src\LegacyBanking.Data\LegacyBanking.Data.csproj
dotnet build .\src\LegacyBanking.Web\LegacyBanking.Web.csproj

# --- Module 3: characterization tests ------------------------------------
dotnet new xunit -n LegacyBanking.Tests -o tests\LegacyBanking.Tests
Remove-Item .\tests\LegacyBanking.Tests\UnitTest1.cs
# retarget the test project to net48 (see Step 3.3), then:
dotnet sln add .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj
dotnet test .\tests\LegacyBanking.Tests\LegacyBanking.Tests.csproj

# --- Module 6: .NET 8 vertical slice -------------------------------------
dotnet new webapi -n LegacyBanking.ModernApi -o src\LegacyBanking.ModernApi -f net8.0
dotnet sln add .\src\LegacyBanking.ModernApi\LegacyBanking.ModernApi.csproj
dotnet build .\src\LegacyBanking.ModernApi\LegacyBanking.ModernApi.csproj

# --- Validation gates ----------------------------------------------------
git diff main...modernization --stat
dotnet build .\LegacyEnterpriseBanking.sln
dotnet test  .\LegacyEnterpriseBanking.sln

# --- Optional: database --------------------------------------------------
sqlcmd -S localhost -i .\database\001-create-database.sql
sqlcmd -S localhost -i .\database\002-seed-data.sql
```

Seed data available for manual testing: accounts `ACC-10001` (SAVINGS, 25,000), `ACC-10002` (SAVINGS,
15,000), `ACC-10003` (CURRENT, 90,000).

---



## Appendix B — Reference test code (Module 3)

If you get stuck, this is a working characterization test file. **Try to have Claude generate it first** —
comparing your output against this is the actual exercise. Verified: 4 passing tests on `net48`.

```csharp
// Characterization tests: these lock in the CURRENT behavior of TransferMoney,
// including behavior we consider defective. Do not "fix" the assertions to be nicer.
// Only the guard clauses are covered here: they execute before the first
// `new AccountRepository()`, so they need no database.

using System;
using LegacyBanking.Business;
using LegacyBanking.Domain.Models;
using Xunit;

namespace LegacyBanking.Tests
{
    public class TransferValidationCharacterizationTests
    {
        [Fact]
        public void NullRequest_Throws()
        {
            var service = new BankingService();
            var ex = Assert.Throws<Exception>(() => service.TransferMoney(null));
            Assert.Equal("Invalid request.", ex.Message);
        }

        [Fact]
        public void SameSourceAndTarget_Throws()
        {
            var service = new BankingService();
            var request = new TransferRequest
            {
                FromAccount = "ACC-10001",
                ToAccount = "ACC-10001",
                Amount = 100m
            };

            var ex = Assert.Throws<Exception>(() => service.TransferMoney(request));
            Assert.Equal("Source and target cannot match.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void NonPositiveAmount_Throws(decimal amount)
        {
            var service = new BankingService();
            var request = new TransferRequest
            {
                FromAccount = "ACC-10001",
                ToAccount = "ACC-10002",
                Amount = amount
            };

            var ex = Assert.Throws<Exception>(() => service.TransferMoney(request));
            Assert.Equal("Amount must be positive.", ex.Message);
        }
    }
}
```

Note what these tests document: business-rule violations throw bare `System.Exception`. A caller cannot
distinguish "insufficient balance" from "database unreachable" without string-matching the message. That
is a finding, and it is exactly why characterization tests are written before refactoring — they make the
current contract visible.

---



## Appendix C — Learning flow at a glance

```text
MODULE 0   Baseline        git + CLAUDE.md + a build that actually compiles
   ↓
MODULE 1   Understand      architecture map + Money Transfer flow + the money-loss window
   ↓
MODULE 2   Assess          technical debt · security · performance · reliability · testability
   ↓
MODULE 3   Plan            phased roadmap + characterization tests
   ↓
MODULE 4   Refactor        constructor injection · atomic transaction · idempotency design
   ↓
MODULE 5   Modernize       HTTP clients · configuration & secrets · data access · logging
   ↓
MODULE 6   Migrate         .NET 8 Money Transfer vertical slice, side by side
   ↓
FINAL      Validate        diff · build · test · independent AI review
```

> **The central lesson of this lab:**
> Do not use Claude to rewrite legacy code. Use Claude to **understand, assess, plan, test, refactor,
> modernize and review** it — incrementally, with a human accountable for every change.

