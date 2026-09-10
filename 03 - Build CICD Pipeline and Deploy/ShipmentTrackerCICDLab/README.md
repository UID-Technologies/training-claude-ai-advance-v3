# ShipmentTracker CI/CD Lab

A student-ready .NET 8 sample project designed specifically for demonstrating:

- Continuous Integration
- Continuous Delivery / Deployment
- GitHub Actions
- Build, test and quality gates
- Docker containerization
- Artifact generation
- Azure Container Registry (ACR)
- Azure Container Apps deployment
- GitHub Environments
- OIDC-based Azure authentication
- Development and Production promotion
- Smoke testing
- Rollback concepts

## Business Scenario

A logistics company exposes a Shipment Tracking API used by customer portals and internal operations teams.

The API supports:

- Create shipment
- Get shipment
- Update shipment status
- List shipments
- Health check

The application itself is intentionally small so the training can focus on CI/CD.

## Solution

```text
ShipmentTrackerCICDLab
│
├── src
│   ├── ShipmentTracker.Api
│   └── ShipmentTracker.Application
│
├── tests
│   └── ShipmentTracker.Tests
│
├── .github
│   └── workflows
│       ├── ci.yml
│       ├── deploy-dev.yml
│       └── deploy-prod.yml
│
├── scripts
│   ├── smoke-test.sh
│   └── setup-azure.sh
│
├── docs
│   ├── STUDENT-LAB.md
│   ├── PIPELINE-ARCHITECTURE.md
│   └── TRAINER-NOTES.md
│
├── Dockerfile
├── .dockerignore
└── ShipmentTracker.sln
```

## Local Commands

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project src/ShipmentTracker.Api
```

Health endpoint:

```text
GET /health
```

## CI/CD Flow

```text
Developer
   |
   v
Pull Request
   |
   v
CI Pipeline
   |
   +--> Restore
   +--> Build
   +--> Tests
   +--> Format Check
   +--> Vulnerability Check
   +--> Publish Artifact
   |
   v
Merge to main
   |
   v
Dev Deployment
   |
   +--> Azure Login using OIDC
   +--> Build Docker Image
   +--> Push Image to ACR
   +--> Deploy to Azure Container Apps
   +--> Smoke Test
   |
   v
Production Promotion
   |
   +--> Manual/Environment Approval
   +--> Deploy SAME image
   +--> Smoke Test
```
