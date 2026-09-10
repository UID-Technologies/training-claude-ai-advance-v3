# Lab 03 — Build CI/CD Pipeline and Deploy with Claude

**Hands-on lab · VS Code + Claude Code · Application: `ShipmentTrackerCICDLab` (.NET 8)**

| | |
|---|---|
| **Duration** | 3–4 hours · 16 tasks |
| **Level** | Intermediate |
| **Stack** | .NET 8, GitHub Actions, Docker, Azure Container Registry, Azure Container Apps |
| **Tools** | VS Code 1.94+, Claude Code, Git, GitHub, Docker Desktop, Azure CLI (for CD tasks) |

## Description

You will use Claude as a **DevOps engineering assistant** to take a small Shipment Tracking API from
"works on my machine" to an automated **build → test → container → deploy** flow. You do not manually
write the entire pipeline — you **analyze, design, generate, run, troubleshoot and review** each piece.

## Business scenario

> A logistics company runs **ShipmentTracker**, an internal API used by customer portals and operations.
> Releases are manual, nobody knows which version is in Dev, and a bad build once reached production.
> Your job: add CI on pull requests, containerize the API, and deploy to Azure Container Apps on every
> merge to `main` — with smoke tests and SHA-tagged immutable images.

## Target flow

```text
Developer → feature branch → GitHub → GitHub Actions
    → restore / build / test / publish
    → Docker image (SHA tag)
    → Azure Container Registry
    → Azure Container Apps
    → smoke test (/health)
```

## What you will produce

| # | Deliverable | Task |
|---|---|---|
| 1 | Repository analysis (build/test/publish commands) | 1–2 |
| 2 | `.github/workflows/ci.yml` | 4–6 |
| 3 | `Dockerfile` + `.dockerignore` | 8–9 |
| 4 | `.github/workflows/deploy-dev.yml` | 13–14 |
| 5 | One pipeline failure analysis (root cause + fix) | 6 or 14 |
| 6 | Deployment evidence (commit SHA → image tag → app URL) | 14 |
| 7 | Final AI review of the CI/CD implementation | 15 |

> **No Azure subscription?** Complete Tasks 0–10 (CI + local Docker). Skip Tasks 11–14 and note
> "CD not executed" in your submission. Tasks 11–14 require Azure.

---

## Prerequisites

```powershell
dotnet --list-sdks          # 8.0.x or 9.0.x
git --version
docker --version          # Docker Desktop running
gh --version                # optional but recommended
az --version                # for Azure CD tasks
```

Install **Claude Code** in VS Code. Open with Spark icon (✱) or `Ctrl+Shift+Esc`. Sign in.

**Permission modes:** **Plan** for analysis/design (Tasks 1, 3, 7, 11). **Manual** for file creation (Tasks 4, 8, 10, 13, 15).

---

# Task 0 — Setup (20 min)

### 0.1 Remove spoilers and pre-built pipelines

The repo ships with complete workflows, Dockerfile and trainer docs. If you leave them in place, Claude
will read them and the lab becomes copy-paste.

```powershell
cd "F:\#Training\training-claude-ai-advance-v3\03 - Build CICD Pipeline and Deploy\ShipmentTrackerCICDLab"

# trainer material — move out of the repo
Move-Item .\docs "$env:USERPROFILE\Desktop\lab03-trainer-docs"

# archive reference CI/CD — you will regenerate these with Claude
New-Item -ItemType Directory -Path .\reference-only -Force | Out-Null
Move-Item .\.github .\reference-only\
Move-Item .\Dockerfile .\reference-only\
Move-Item .\.dockerignore .\reference-only\
Move-Item .\scripts .\reference-only\
```

> Do **not** open `reference-only\` during the lab. Compare your output to it only after Task 15.

### 0.2 Fix the build (shipped solution builds nothing)

```powershell
dotnet build .\ShipmentTracker.sln
```

You get `Unable to find a project to restore!` then `Build succeeded` — **nothing compiled**. Fix it:

```powershell
Remove-Item .\ShipmentTracker.sln -Force
dotnet new sln -n ShipmentTracker
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
dotnet build .\ShipmentTracker.sln --configuration Release
```

Now you get a real error:

```text
tests\ShipmentTracker.Tests\ShipmentServiceTests.cs: error CS0246: 'Fact' could not be found
```

Add to `tests\ShipmentTracker.Tests\ShipmentTracker.Tests.csproj`:

```xml
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
```

Verify:

```powershell
dotnet build .\ShipmentTracker.sln --configuration Release
dotnet test  .\ShipmentTracker.sln --configuration Release
dotnet publish .\src\ShipmentTracker.Api\ShipmentTracker.Api.csproj --configuration Release --output .\publish
```

Expected: `0 Error(s)`, `Passed: 3`, publish folder created.

Run the API locally:

```powershell
dotnet run --project .\src\ShipmentTracker.Api\ShipmentTracker.Api.csproj
# browse http://localhost:5xxx/health  (port shown in console)
```

### 0.3 Git baseline and GitHub repo

```powershell
code .
git init
git add .
git commit -m "baseline: ShipmentTracker API without CI/CD"
git branch -M main
```

Create a GitHub repository and push:

```powershell
gh repo create shipment-tracker-cicd-lab --private --source=. --remote=origin --push
# or create the repo manually on github.com, then:
# git remote add origin https://github.com/<you>/shipment-tracker-cicd-lab.git
# git push -u origin main
```

### 0.4 Engineering contract

Run `/init` in Claude, then replace `CLAUDE.md` with [Appendix A](#appendix-a--claudemd). Confirm with `/context`.

```powershell
git add . ; git commit -m "build: repair solution and test project"
git push
```

**Check:** 3 tests pass, `/health` returns 200 locally, no `.github/` folder in repo root.

---

# PART A — Continuous Integration

## Task 1 — Understand the repository (10 min · Plan)

**Prompt:**

```text
Act as a senior .NET and DevOps engineer. Analyze this repository. Do not modify files.

Identify:
1. .NET version
2. Solution file
3. Application projects and test projects
4. Commands to restore, build, test and publish the API
5. Whether the app is suitable for containerization and why
6. Which project is the entrypoint for Docker
7. Health/readiness endpoint if any
8. Files you would need for a GitHub Actions CI pipeline

Create a short recommended CI flow. Do not generate YAML yet. Cite file paths.
```

**Expected discovery:**

```text
ShipmentTracker.sln
├── src/ShipmentTracker.Api          ← web host, port from Kestrel / ASPNETCORE_URLS
├── src/ShipmentTracker.Application  ← business logic
└── tests/ShipmentTracker.Tests      ← 3 xUnit tests

GET /health  →  Program.cs MapHealthChecks("/health")
```

---

## Task 2 — Verify Claude's commands yourself (10 min)

Never trust AI-generated commands until you run them.

```powershell
dotnet restore .\ShipmentTracker.sln
dotnet build   .\ShipmentTracker.sln --configuration Release
dotnet test    .\ShipmentTracker.sln --configuration Release
dotnet publish .\src\ShipmentTracker.Api\ShipmentTracker.Api.csproj --configuration Release --output .\publish
```

**Check:** all four succeed. Note the exact commands — you will embed them in YAML.

---

## Task 3 — Design the CI pipeline (15 min · Plan)

**Prompt:**

```text
Based on this repository, design a simple GitHub Actions CI pipeline.

Requirements:
- trigger on pull requests to main
- trigger on pushes to feature/* and bugfix/* branches
- Ubuntu runner
- .NET 8
- restore → build → test → publish the API
- upload publish output as a workflow artifact named with the commit SHA

Do not create the file yet. Explain:
1. workflow trigger
2. job structure
3. runner
4. each step and what failure looks like
5. minimum permissions the workflow needs
```

**Expected flow:**

```text
PR / feature push → checkout → setup-dotnet → restore → build → test → publish → upload-artifact
```

---

## Task 4 — Generate the CI workflow (20 min · Manual)

**Prompt:**

```text
Create .github/workflows/ci.yml for the CI pipeline we designed.

Keep it simple:
- no Docker, no Azure, no SonarQube
- use actions/checkout@v4 and actions/setup-dotnet@v4
- dotnet-version: 8.0.x
- test with --configuration Release
- publish src/ShipmentTracker.Api/ShipmentTracker.Api.csproj to ./publish
- upload artifact with retention 7 days
- permissions: contents read only

After creating the file, explain every section of the YAML in plain language.
```

Review the generated file. Common mistakes to reject:

- wrong solution or project path
- `dotnet test` without `--configuration Release` while build uses Release
- missing `permissions:` block
- hardcoded secrets (there should be none)

```powershell
git diff
git add .github/workflows/ci.yml
git commit -m "ci: add GitHub Actions build and test workflow"
```

---

## Task 5 — Self-review and push (10 min)

**Prompt:**

```text
Review .github/workflows/ci.yml as if another engineer wrote it.

Check: trigger correctness, .NET version, unnecessary steps, incorrect paths,
build/test duplication, failure handling, security (permissions, secrets).

List findings. Change the file only for real issues.
```

Push to a feature branch:

```powershell
git checkout -b feature/claude-cicd
git push -u origin feature/claude-cicd
```

Open a **Pull Request** to `main` on GitHub. Watch **Actions** — the CI workflow should run.

**Check:** workflow appears, all steps green.

**Deliverable 1 + 2.**

---

## Task 6 — Break a test and analyze a pipeline failure (15 min)

Intentionally fail CI to practise log analysis.

Edit `tests/ShipmentTracker.Tests/ShipmentServiceTests.cs` — change one assertion:

```csharp
Assert.Equal("Created", shipment.Status);   // was correct
Assert.Equal("Shipped", shipment.Status);   // will fail
```

```powershell
git add . ; git commit -m "test: intentional failure for pipeline demo"
git push
```

Copy the **Test** step error from GitHub Actions. Start a **new Claude message** (or `/clear`) and paste:

```text
The GitHub Actions pipeline failed. Here is the relevant error:

<PASTE ERROR HERE>

Analyze this failure. Do not modify code yet.

Tell me:
1. Which pipeline stage failed
2. Root cause
3. Whether this is a pipeline config problem or application/test code
4. Which file to inspect
5. Minimum fix
```

Fix the test, push again, confirm CI is green.

```powershell
git revert HEAD --no-edit
git push
```

**Deliverable 5** (failure analysis document).

---

# PART B — Containerization

## Task 7 — Containerization plan (10 min · Plan)

**Prompt:**

```text
Analyze this .NET 8 application for Docker deployment. Do not create a Dockerfile yet.

Explain:
1. Which project runs in the container
2. Build stage vs runtime stage
3. Application port (what the container should expose)
4. Required COPY order for layer caching
5. Why multi-stage build
6. What belongs in .dockerignore

Propose the Docker build flow as a diagram.
```

**Expected:**

```text
Source → SDK image → restore → publish Release → aspnet runtime → listen on 8080
```

The API maps health at `/health`. Container should set `ASPNETCORE_URLS=http://+:8080`.

---

## Task 8 — Generate Dockerfile and .dockerignore (20 min · Manual)

**Prompt:**

```text
Create a production-friendly multi-stage Dockerfile for this repository.

Requirements:
- mcr.microsoft.com/dotnet/sdk:8.0 for build
- mcr.microsoft.com/dotnet/aspnet:8.0 for runtime
- restore using copied .csproj files first (cache-friendly)
- publish Release build of src/ShipmentTracker.Api
- expose port 8080, set ASPNETCORE_URLS=http://+:8080
- create or update .dockerignore (exclude bin, obj, .git, TestResults)

Do not add Azure-specific configuration.
After creating the files, review them for: unnecessary layers, wrong paths,
SDK left in final image, security (non-root user if appropriate).
```

**Prompt — review:**

```text
Review the Dockerfile and .dockerignore you generated.

Check: cache efficiency, image size, security, working directory, project paths,
runtime port, and whether tests belong in the build stage (they should not run in Docker build unless we choose to).
```

```powershell
git add Dockerfile .dockerignore
git commit -m "docker: multi-stage Dockerfile for ShipmentTracker API"
```

---

## Task 9 — Build and run locally (15 min)

Ensure **Docker Desktop is running**, then:

```powershell
docker build -t shipment-tracker:local .
docker run --rm -p 8080:8080 shipment-tracker:local
```

Test in another terminal or browser:

```text
http://localhost:8080/health
```

If build fails, **do not** ask Claude to rewrite everything. Paste the error:

```text
Docker build failed with this error:

<PASTE ERROR>

Find the root cause. Is it: wrong path, COPY problem, restore, publish, or context?

Suggest only the minimum change.
```

**Check:** HTTP 200 from `/health`. **Deliverable 3.**

---

## Task 10 — Add Docker build to CI (15 min · Manual)

**Prompt:**

```text
Extend .github/workflows/ci.yml.

After tests pass, build the Docker image locally on the runner:
- image name: shipment-tracker
- tag: ${{ github.sha }}
- do NOT push to a registry yet
- Docker build must depend on test success

Modify only ci.yml. Show the diff and explain the new step.
```

GitHub Actions needs Docker on `ubuntu-latest` (available by default). Review:

```powershell
git diff
git add .github/workflows/ci.yml
git commit -m "ci: build Docker image after tests pass"
git push
```

Confirm the PR CI now includes a Docker build step.

---

# PART C — Continuous Deployment to Azure

> Skip this part if you have no Azure subscription. Complete Task 15 review using CI + Docker only.

## Task 11 — Design Azure deployment (15 min · Plan)

**Prompt:**

```text
We want to deploy this containerized app to Azure.

Target: GitHub Actions → Azure Container Registry → Azure Container Apps (Dev only).

Do not generate code yet. Explain:
1. Azure resources required
2. What GitHub needs (secrets vs variables)
3. Why OIDC is preferred over storing an Azure client secret in GitHub
4. How Docker images should be tagged (immutable SHA)
5. How the Container App receives a new image
6. How to validate deployment (health check)
```

**Expected resources:**

```text
Resource Group → ACR → Container Apps Environment → Container App (Dev)
```

**GitHub configuration (typical):**

| Name | Secret or Variable | Purpose |
|---|---|---|
| `AZURE_CLIENT_ID` | Secret | App registration / managed identity client ID |
| `AZURE_TENANT_ID` | Secret | Azure AD tenant |
| `AZURE_SUBSCRIPTION_ID` | Secret | Subscription |
| `AZURE_RESOURCE_GROUP` | Variable | Resource group name |
| `ACR_NAME` | Variable | Registry name (not login server) |
| `ACR_LOGIN_SERVER` | Variable | e.g. myacr.azurecr.io |
| `DEV_CONTAINER_APP_NAME` | Variable | Dev app name |

---

## Task 12 — Provision Azure and configure OIDC (30 min)

**Prompt:**

```text
Create minimal Azure CLI commands for this lab:

- Resource Group (centralindia or eastus)
- Azure Container Registry (Basic SKU)
- Container Apps Environment
- One Development Container App (placeholder image is fine initially)

Use placeholders for subscription ID and globally unique ACR name.
Keep commands student-friendly. Do not include Production yet.

Then explain the OIDC setup steps:
- Azure app registration or managed identity
- Federated credential for GitHub (repo, branch/environment)
- Role assignments (AcrPush, Container Apps Contributor or equivalent)
- GitHub Environment named "development"
```

Run the Azure commands yourself (replace placeholders):

```powershell
az login
az account set --subscription "<subscription-id>"
# run the generated az group create / az acr create / az containerapp commands
```

Configure **GitHub → Settings → Secrets and variables → Actions** and create environment **development**.

Reference script (after you understand it): `reference-only\scripts\setup-azure.sh`

**Check:** ACR exists, Container App exists, GitHub secrets/vars configured, OIDC federated credential links your repo.

---

## Task 13 — Generate deploy-dev workflow (25 min · Manual)

**Prompt:**

```text
Create .github/workflows/deploy-dev.yml

Trigger: push to main

Steps:
1. checkout
2. setup .NET 8
3. restore, build, test (gate — do not deploy if tests fail)
4. Azure login using OIDC (azure/login@v2) — no client secret in YAML
5. az acr login
6. docker build and tag with ${{ github.sha }} and dev-latest
7. docker push both tags to ACR
8. az containerapp update with the SHA-tagged image
9. resolve app FQDN and run curl against /health (retry loop)
10. write deployment summary to GITHUB_STEP_SUMMARY

Use:
- secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
- vars: AZURE_RESOURCE_GROUP, ACR_NAME, ACR_LOGIN_SERVER, DEV_CONTAINER_APP_NAME
- permissions: contents read, id-token write
- environment: development

Do not add Production deployment.
After creating the file, explain each step: why it exists, inputs, outputs, failure impact.
```

Review for security:

```text
Review deploy-dev.yml for:
- id-token: write permission present
- no hardcoded credentials
- tests run before push/deploy
- image tagged with github.sha (immutable)
- no excessive permissions

List any Critical/High findings before I merge.
```

```powershell
git add .github/workflows/deploy-dev.yml
git commit -m "cd: deploy to Azure Container Apps on main"
git push
```

Merge `feature/claude-cicd` → `main` (via PR). Watch the deploy workflow.

**Deliverable 4.**

---

## Task 14 — Troubleshoot deployment (20 min)

When something fails, copy the log and use targeted prompts — **do not rewrite the whole workflow first**.

### Azure login failed

```text
GitHub Actions Azure login step failed:

<PASTE ERROR>

Analyze only authentication. Check: id-token permission, client ID, tenant ID,
subscription ID, federated credential, branch/environment mismatch.

Give a diagnostic sequence from most likely to least likely cause.
Do not rewrite the workflow yet.
```

### Docker push failed

```text
Docker build succeeded but push to ACR failed:

<PASTE LOG>

Is the problem: registry login, registry name, image tag, permissions,
repository path, or role assignment? Give exact checks to run.
```

### Container App not healthy

```text
Image is in ACR but the Container App is not healthy.

Analyze: image reference, ACR pull access, target port vs app port (8080),
ASPNETCORE_URLS, startup failure, /health endpoint.

Give a logical troubleshooting sequence.
```

After success, record **Deliverable 6**:

```text
Git commit SHA  →  Docker image tag  →  Container App URL  →  /health = 200
```

---

# PART D — Validate and extend

## Task 15 — Final review + feature through the pipeline (30 min)

### 15.1 Independent AI review

`/clear` or new tab. Then:

```text
Act as a principal DevOps engineer. Review the complete CI/CD implementation:

.github/workflows/ci.yml
.github/workflows/deploy-dev.yml   (if present)
Dockerfile
.dockerignore

Check: duplication, triggers, security, hardcoded secrets, mutable tags,
Docker quality, missing test gates, deployment validation, excessive permissions.

Classify findings Critical / High / Medium / Low. Do not change files yet.
```

Then:

```text
Review the current git diff as if another AI wrote it.

Look for: dangerous permissions, secrets in YAML, command injection,
incorrect Azure commands, invalid Docker assumptions, unnecessary complexity.
```

**Deliverable 7.**

### 15.2 End-to-end feature challenge

Add `GET /api/shipments/count` returning the number of shipments.

**Prompt:**

```text
Add GET /api/shipments/count to the ShipmentTracker API.

Requirements:
- add Count() to IShipmentService and ShipmentService
- expose GET /api/shipments/count on ShipmentsController
- add one xUnit test for Count
- do not break existing tests

Show files to change before editing.
```

Run locally, then push through the full pipeline:

```powershell
dotnet test .\ShipmentTracker.sln --configuration Release
git checkout -b feature/shipment-count
git add . ; git commit -m "feat: add GET /api/shipments/count"
git push -u origin feature/shipment-count
```

Open PR → CI runs → merge → deploy (if Azure configured) → verify count endpoint on deployed URL.

---

# Appendix A — `CLAUDE.md`

```markdown
# ShipmentTracker — DevOps AI Contract

Act as a senior .NET and DevOps engineer helping build CI/CD for this repository.

## Project
- .NET 8 ASP.NET Core API: src/ShipmentTracker.Api
- Tests: tests/ShipmentTracker.Tests (xUnit)
- Health: GET /health
- Container port: 8080, ASPNETCORE_URLS=http://+:8080

## Commands
- Restore: dotnet restore ShipmentTracker.sln
- Build:   dotnet build ShipmentTracker.sln --configuration Release
- Test:    dotnet test ShipmentTracker.sln --configuration Release
- Publish: dotnet publish src/ShipmentTracker.Api/ShipmentTracker.Api.csproj --configuration Release --output publish
- Docker:  docker build -t shipment-tracker:local .

## Rules
1. Inspect the repo before proposing pipelines or Dockerfiles.
2. Do not generate the entire CI/CD solution in one step.
3. Explain design before creating YAML, Dockerfile or scripts.
4. Never hardcode credentials, connection strings or tokens in committed files.
5. Prefer GitHub OIDC for Azure — not long-lived client secrets.
6. Tag Docker images with immutable git SHA, not only :latest.
7. Build and test must pass before any deploy step.
8. Add post-deploy health validation (/health).
9. When a pipeline fails, analyze logs and root cause before changing files.
10. After changes, review git diff critically. Treat generated YAML as untrusted until reviewed.
```

---

# Appendix B — Troubleshooting

| Symptom | Fix |
|---|---|
| `Unable to find a project to restore!` then build "succeeds" | Rebuild `.sln` with `dotnet new sln` + `dotnet sln add` (Task 0.2) |
| `error CS0246: 'Fact' could not be found` | Add `<Using Include="Xunit" />` to test `.csproj` |
| CI workflow does not appear on GitHub | Push `.github/workflows/*.yml` to the branch; check Actions tab enabled |
| `dotnet format --verify-no-changes` fails in CI | Run `dotnet format ShipmentTracker.sln` locally and commit, or remove format step from v1 CI |
| Docker: `cannot connect to docker API` | Start Docker Desktop |
| Container exits immediately | Check `ASPNETCORE_URLS`, port mapping `-p 8080:8080`, and `USER` directive in Dockerfile |
| Azure login: `No matching federated identity` | Federated credential must match repo, branch/environment, and issuer |
| ACR push 401/403 | OIDC identity needs `AcrPush` on ACR |
| Container App unhealthy | Set ingress target port **8080**; image must listen on 8080 |
| Claude generates complete pipeline in one shot | Switch to **Plan** mode; use smaller prompts from this lab |

---

# Appendix C — Command summary

```powershell
# setup
cd ShipmentTrackerCICDLab
Move-Item .\docs "$env:USERPROFILE\Desktop\lab03-trainer-docs"
New-Item -ItemType Directory -Path .\reference-only -Force
Move-Item .\.github, .\Dockerfile, .\.dockerignore, .\scripts .\reference-only\

# fix build
Remove-Item .\ShipmentTracker.sln -Force
dotnet new sln -n ShipmentTracker
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }
# add <Using Include="Xunit" /> to tests project

dotnet build .\ShipmentTracker.sln --configuration Release
dotnet test  .\ShipmentTracker.sln --configuration Release
dotnet publish .\src\ShipmentTracker.Api\ShipmentTracker.Api.csproj -c Release -o publish

# git + feature branch
git init ; git add . ; git commit -m "baseline"
gh repo create shipment-tracker-cicd-lab --private --source=. --remote=origin --push
git checkout -b feature/claude-cicd

# docker
docker build -t shipment-tracker:local .
docker run --rm -p 8080:8080 shipment-tracker:local
curl http://localhost:8080/health

# validation loop (repeat after every change)
git diff
git add . ; git commit -m "..." ; git push
```

> **The rule:** Claude designs and generates. You run, review, and own every deployment decision.
