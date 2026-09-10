# CI/CD Architecture

## Continuous Integration

Triggered for:

- Pull requests to `main`
- Pushes to `feature/**`
- Pushes to `bugfix/**`

Pipeline:

```text
Checkout
   ↓
Setup .NET
   ↓
Restore
   ↓
Format Check
   ↓
Build
   ↓
Unit Tests
   ↓
Dependency Vulnerability Check
   ↓
Publish
   ↓
Upload Artifact
```

## Development Deployment

Triggered by merge/push to `main`.

```text
main
  ↓
Build + Test
  ↓
Azure OIDC Login
  ↓
Docker Build
  ↓
Push immutable SHA image to ACR
  ↓
Deploy to Dev Container App
  ↓
Health/Smoke Test
```

## Production Deployment

Triggered manually using `workflow_dispatch`.

Production should use a GitHub Environment with required reviewers.

```text
Approved Dev Image
        ↓
Enter Git SHA
        ↓
Production Environment Approval
        ↓
Verify image exists
        ↓
Deploy SAME image
        ↓
Smoke Test
```

## Key Principle

Do not rebuild application code for production.

Promote the exact immutable image that was tested in the lower environment.

```text
Build Once
   ↓
SHA-tagged Image
   ↓
Dev
   ↓
Test
   ↓
Production
```
