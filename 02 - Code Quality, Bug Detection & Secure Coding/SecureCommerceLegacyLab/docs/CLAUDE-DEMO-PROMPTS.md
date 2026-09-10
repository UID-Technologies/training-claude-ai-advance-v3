# Claude Demo Prompts

## Understand
Act as a senior .NET engineer. Explain the repository, business flow, dependencies and highest-risk components. Do not modify code.

## Code Quality
Review code quality. Classify maintainability, complexity, SOLID, error handling, async usage, coupling, duplication and naming issues. Include exact files.

## Bug Detection
Find functional and edge-case bugs. For each give: trigger, expected behavior, actual behavior, business impact and a test that exposes it.

## Secure Coding
Perform a security review covering authentication, authorization, password handling, secrets, SQL, upload, logging, error handling, validation and external HTTP.

## Order Flow
Trace OrderController -> OrderService -> ProductRepository -> DB -> PaymentGateway and identify consistency, concurrency and failure scenarios.

## Safe Fix
Fix only one issue. Before editing explain root cause, proposed change, files affected, regression risk and tests.

## Review AI Changes
Review git diff like a skeptical principal engineer. Check behavior regressions, security gaps, race conditions, error handling and missing tests.
