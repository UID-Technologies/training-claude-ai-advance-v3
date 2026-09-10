# Student Lab – Build a CI/CD Pipeline and Deploy the Application

## Objective

You are responsible for automating delivery of the Shipment Tracking API.

By the end, your repository should:

1. Validate every pull request.
2. Prevent broken code from reaching `main`.
3. Build and test automatically.
4. Package the application as a Docker image.
5. Push images to Azure Container Registry.
6. Deploy automatically to Development.
7. Require controlled promotion to Production.
8. Verify deployment through a smoke test.

---

# Lab 1 – Understand and Run the Application

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/ShipmentTracker.Api
```

Verify:

```text
GET http://localhost:<port>/health
```

Then create a branch:

```bash
git checkout -b feature/cicd-lab
```

---

# Lab 2 – Build the Continuous Integration Pipeline

Open:

```text
.github/workflows/ci.yml
```

Understand the stages:

```text
Restore → Format → Build → Test → Security Check → Publish Artifact
```

Push the branch and open a pull request.

Observe which step runs first and what happens if a step fails.

## Exercise

Intentionally break one test.

Push again.

Observe that CI becomes red.

Fix the test/application and confirm the pipeline turns green.

### Learning Point

A CI pipeline is a quality gate.

Code should not be merged merely because it compiles on a developer's laptop.

---

# Lab 3 – Containerize the Application

Build locally:

```bash
docker build -t shipment-tracker:local .
```

Run:

```bash
docker run --rm -p 8080:8080 shipment-tracker:local
```

Test:

```text
http://localhost:8080/health
```

Understand the multi-stage Dockerfile:

```text
SDK Image
   ↓
Restore
   ↓
Build/Publish
   ↓
ASP.NET Runtime Image
   ↓
Smaller Production Container
```

---

# Lab 4 – Configure Azure and GitHub

Azure components:

```text
Azure Container Registry
Azure Container Apps Environment
Dev Container App
Production Container App
```

GitHub Environments:

```text
development
production
```

Configure GitHub environment/repository values.

Secrets:

```text
AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
```

Variables:

```text
AZURE_RESOURCE_GROUP
ACR_NAME
ACR_LOGIN_SERVER
DEV_CONTAINER_APP_NAME
PROD_CONTAINER_APP_NAME
```

Use OIDC/federated identity instead of storing an Azure client secret.

---

# Lab 5 – Continuous Deployment to Development

Open:

```text
.github/workflows/deploy-dev.yml
```

Merge your pull request to `main`.

The pipeline should:

```text
Build
  ↓
Test
  ↓
Azure Login
  ↓
Docker Build
  ↓
Push to ACR
  ↓
Deploy to Development
  ↓
Smoke Test
```

Find the deployed application URL in the workflow summary.

Verify:

```text
/health
/api/shipments
```

### Image Tagging

The image is tagged with:

```text
github.sha
```

Example:

```text
shipment-tracker:a81c5e...
```

This creates an immutable link between source code and deployment.

---

# Lab 6 – Production Promotion

Configure the GitHub `production` environment with required reviewers.

Run:

```text
Actions → Deploy Production → Run workflow
```

Provide the Git commit SHA/image tag already tested in Development.

Observe:

```text
Request Deployment
      ↓
Environment Approval
      ↓
Deploy Existing Image
      ↓
Smoke Test
```

Important:

Production should deploy the same image tested in Development.

Do not rebuild it.

---

# Lab 7 – Failure and Rollback Exercise

Deploy a deliberately unhealthy application change to Development.

Observe the smoke-test failure.

Then redeploy the previous known-good SHA.

Conceptually:

```text
Bad SHA
   ↓
Health Check Fails
   ↓
Identify Last Good SHA
   ↓
Redeploy Last Good Image
```

This demonstrates a simple rollback model.

---

# Final Student Deliverables

Submit:

1. CI workflow run showing Build + Tests passed.
2. Docker image running locally.
3. ACR repository containing SHA-tagged images.
4. Development Container App deployment.
5. Production environment with approval configured.
6. Successful production smoke test.
7. Screenshot or notes showing one failed pipeline and its correction.
8. Short explanation of rollback using an earlier immutable image.
