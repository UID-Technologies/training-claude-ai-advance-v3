# Claims Processing Testing Lab

A .NET 8 sample application designed for demonstrating how Claude can help engineers:

- understand existing business logic
- identify unit-test boundaries
- generate xUnit tests
- discover edge cases
- create mocks/fakes
- generate ASP.NET Core integration tests
- debug failing tests
- distinguish application defects from test defects
- improve test coverage without blindly changing production code

## Business Scenario

A health insurance company receives reimbursement claims from members.

A claim contains:

- member ID
- treatment type
- claimed amount
- treatment date
- emergency flag

The platform validates the claim, calculates the payable amount, stores it, and exposes APIs for submission and retrieval.

> This is a fictional training application. No real patient or health data is used.

## Business Rules

1. Member ID is required.
2. Claim amount must be greater than zero.
3. Claim amount cannot exceed ₹500,000.
4. Treatment date cannot be in the future.
5. Standard outpatient claims are reimbursed at 80%.
6. Emergency claims are reimbursed at 90%.
7. Preventive treatment is reimbursed at 100%, up to ₹10,000.
8. Dental treatment is reimbursed at 60%.
9. Unsupported treatment types are rejected.
10. Claims at or above ₹100,000 require manual review.

## Technology

- .NET 8
- ASP.NET Core Web API
- EF Core InMemory provider
- xUnit
- Moq
- Microsoft.AspNetCore.Mvc.Testing
- VS Code
- Claude / Claude Code

## Repository Structure

```text
ClaimsProcessingTestingLab
│
├── src
│   ├── Claims.Domain
│   ├── Claims.Application
│   ├── Claims.Infrastructure
│   └── Claims.Api
│
├── tests
│   ├── Claims.UnitTests
│   └── Claims.IntegrationTests
│
├── docs
│   ├── STUDENT-LAB.md
│   ├── CLAUDE-PROMPTS.md
│   └── TRAINER-NOTES.md
│
└── ClaimsProcessingTestingLab.sln
```

## Run Locally

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Claims.Api
```

Open:

```text
GET /health
GET /api/claims
```

Submit:

```http
POST /api/claims
Content-Type: application/json
```

```json
{
  "memberId": "MEM-1001",
  "treatmentType": "Outpatient",
  "claimedAmount": 5000,
  "treatmentDate": "2026-09-01",
  "isEmergency": false
}
```

## Intentional Training Characteristics

The project starts with only a few tests.

Students use Claude to discover and implement missing tests.

There are also two intentional defects:

- preventive reimbursement can exceed the ₹10,000 cap
- duplicate claim submission is not prevented

Students should first expose defects with tests before changing production code.
