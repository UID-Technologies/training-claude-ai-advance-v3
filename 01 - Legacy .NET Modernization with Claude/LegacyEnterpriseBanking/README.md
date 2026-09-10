# LegacyEnterpriseBanking

Training-only legacy .NET Framework 4.8 application for AI-assisted modernization demonstrations.

## Scenario
A fictional retail-banking platform used for:
- customer onboarding
- accounts
- transfers
- loans
- transaction search
- audit logging
- notifications
- nightly interest processing

## Intentionally Legacy
The code deliberately contains:
- ASP.NET Web API 2
- .NET Framework 4.8
- Entity Framework 6
- raw ADO.NET
- static helpers
- direct `new` dependencies
- God services
- sync HTTP calls
- `.Result`
- raw threads
- SQL injection
- plaintext secrets
- weak validation
- stack-trace leakage
- missing transaction boundaries
- N+1 queries
- minimal automated testing

Do NOT treat this repository as production guidance.

## Recommended Demo
1. Ask AI to explain the architecture.
2. Ask AI to find technical debt.
3. Ask AI to perform a security review.
4. Ask AI for a phased .NET 8 migration roadmap.
5. Refactor TransferMoney first.
6. Generate characterization/unit tests.
7. Move one feature to ASP.NET Core.
8. Ask AI to review its own modernization changes.
