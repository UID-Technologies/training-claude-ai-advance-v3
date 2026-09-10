# Trainer Notes

## Recommended Session Story

Do not start by showing the YAML.

Start with the delivery problem:

```text
Developer says:
"It works on my machine."

Operations says:
"We don't know what version is deployed."

Security says:
"Why is an Azure password stored in GitHub?"

Business says:
"Why did a failed release reach production?"
```

Then construct the solution progressively.

## Demo Sequence

1. Run app locally.
2. Run unit tests.
3. Push to a feature branch.
4. Show CI.
5. Break a test and show pipeline failure.
6. Fix it.
7. Build Docker image.
8. Explain build artifact vs container image.
9. Merge to main.
10. Show OIDC Azure login.
11. Push SHA-tagged image to ACR.
12. Deploy to Development.
13. Run smoke test.
14. Show GitHub Production Environment approval.
15. Promote same SHA to Production.
16. Demonstrate rollback to previous SHA.

## Concepts to Emphasize

### CI

CI answers:

> Is this change safe enough to merge?

### CD

CD answers:

> Can the validated software move reliably into an environment?

### Deployment

Deployment is one stage of CD, not the entire CI/CD process.

### Build Once, Deploy Many

Do not rebuild separately for Dev and Production.

Promote the same immutable artifact/image.

### OIDC

Prefer short-lived cloud credentials through federation rather than long-lived Azure client secrets.

### Environment Protection

Use GitHub Environment protection rules for Production approval.

### Health Check

A deployment succeeding technically does not prove the application is healthy.

Always add post-deployment validation.

## Optional Extensions

If time allows add:

- Code coverage gate
- SonarQube/SonarCloud
- Gitleaks
- Trivy image scanning
- SAST
- OpenAPI integration tests
- Blue/green deployment
- Azure Container Apps revisions
- Terraform/Bicep infrastructure provisioning
