# Trainer Answer Sheet

Hide this file before the student investigation.

## Functional bugs
- zero/negative quantities accepted
- FLAT100 can make small orders negative
- coupon matching is case-sensitive
- inventory race condition
- partial stock updates possible
- payment happens after order persistence
- payment failure leaves inventory reduced
- duplicate submissions/idempotency not handled
- null password crashes hashing
- search null input can fail

## Security
- hardcoded SQL password
- hardcoded JWT secret
- unsalted fast SHA256 password hashing
- account enumeration messages
- SQL injection in AdminReportRepository
- stack trace disclosure
- fake authentication token
- admin API has no authorization
- path traversal in file upload
- no file size/content/type checks
- logging of request data
- no HTTPS/auth middleware/rate limiting

## Code quality
- generic Exception
- concrete PaymentGateway constructed inside service
- static logger
- sync database calls
- HttpClient per request
- blocking .Result
- magic status strings
- nullable disabled
- responsibilities mixed in OrderService

## Performance
- ToList before product filtering
- SaveChanges inside inventory loop
- sync file logging
- synchronous data access
- HttpClient lifecycle issue

## Testability
- concrete PaymentGateway
- static AuditLogger
- hard-coded DateTime
- minimal unit tests
- no integration/security tests

## Reliability
- no transaction around inventory and order
- no optimistic concurrency
- no idempotency
- no compensation strategy
