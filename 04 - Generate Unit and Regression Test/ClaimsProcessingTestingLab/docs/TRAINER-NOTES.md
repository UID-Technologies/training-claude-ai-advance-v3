# Trainer Notes

## Recommended Demo Story

Tell students:

> We inherited this Claims Processing API. It has some tests, but we do not know whether important business rules are actually protected.

Do not reveal the intentional defects immediately.

Let Claude discover them through the test-design process.

## Best Live Demo

### Demo 1 – Test Matrix Before Code

Ask Claude to inspect `ClaimCalculator`.

Claude should identify:

- outpatient 80%
- emergency 90%
- preventive 100% with cap
- dental 60%
- unsupported treatment error

Then ask for tests.

The preventive cap test should fail.

This creates a strong teaching moment:

```text
Requirement
   ↓
AI-Generated Test
   ↓
Failure
   ↓
Root Cause Analysis
   ↓
Real Production Defect
```

### Demo 2 – Mocking

Use `ClaimService`.

Explain why repository and notification are mocked while validator and calculator remain real.

Show:

```text
ClaimService
    |
    +--> IClaimRepository      Mock
    |
    +--> INotificationService Mock
    |
    +--> ClaimValidator        Real
    |
    +--> ClaimCalculator       Real
```

### Demo 3 – Integration Testing

Use `WebApplicationFactory<Program>`.

Demonstrate:

```text
HTTP Request
   ↓
ASP.NET Pipeline
   ↓
Controller
   ↓
Service
   ↓
Repository
   ↓
EF Core InMemory
```

### Demo 4 – AI Validation

Ask a fresh Claude review of the generated tests.

Important discussion:

A large number of tests does not mean good coverage.

The goal is meaningful behavior protection, especially:

- boundaries
- business rules
- failure paths
- contracts
- integration points

## Intentional Defects

Trainer only:

1. `ClaimCalculator` does not cap Preventive at 10,000.
2. `ClaimService` has no duplicate/idempotency protection.

Do not fix these before the session.

## Optional Extension

If time permits ask Claude to:

- convert repeated tests into `[Theory]`
- produce coverage using Coverlet
- identify mutation-test candidates
- create contract-test ideas
- add test-data builders
- discuss Testcontainers vs EF InMemory
