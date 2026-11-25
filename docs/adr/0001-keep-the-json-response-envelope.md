# 1. Keep the JsonResponse envelope instead of ProblemDetails

- **Status:** Accepted
- **Date:** 2026-11-24
- **Plan item:** F-22 (finding A-31)

## Context

Every response, success or failure, is the `JsonResponse` envelope: `{ "httpCode", "ok", "data", "errors" }`. Paged lists add a `Pagination` header. The re-assessment (A-31) recommended RFC 9457 ProblemDetails for errors and domain exceptions instead of `ResponseBuilder` results.

Adopting ProblemDetails would change the body of every error the API returns. Every client that reads `ok` and `errors` would break at once, and so would the 401/403/429/5xx bodies added since (F-49, F-51, F-45), which deliberately use the same envelope. The envelope already gives each failure a stable status code and a message list. Domain exceptions would also replace the convention that expected business failures travel as `ResponseBuilder` results through every layer (see `src/CLAUDE.md`).

## Decision

Keep the envelope as the API contract for successes and errors, and keep expected failures as `ResponseBuilder` results. F-22 fixes the inconsistencies inside that contract instead:

- every create answers `201 Created` with a `Location` header pointing at the new resource (Reservations used to answer 200);
- a duplicate (description, Route, license plate, email) answers `409 Conflict`, like a full Trip and database constraint violations, instead of 400;
- `400` stays for invalid input.

## Consequences

- Clients keep parsing one body shape, with no versioned migration needed.
- The API does not speak ProblemDetails. Revisit this in a v2 of the API (new route version) if clients need it; that is where a breaking body change belongs.
- Unexpected failures still surface as exceptions and are rendered into the envelope by `ErrorHandlerMiddleware`.
