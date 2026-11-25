# GoTransport.Api

**Responsibility:** the thin HTTP layer and composition root — versioned controllers, middleware, DI/middleware wiring, and configuration.

## Contents

- `Controllers/v1/` — one controller per resource, plus `AccountController` (login).
- `Extensions/` — `ServiceExtensions` (versioning, JWT, CORS, Swagger, validation response), `LoggingExtensions` (Serilog), `HealthCheckExtensions` (anonymous `/health/live`, `/health/ready` and `/health` return a status only; `/health/details` is for Administrators), `AppExtensions` (middleware registration).
- `Middlewares/ErrorHandlerMiddleware.cs`.
- `Program.cs`, `appsettings*.json`, `Properties/launchSettings.json`.

## Dependencies

Application and Persistence. Controllers depend only on `I<Entity>Service` interfaces.

## Conventions

### Controllers

Each action calls the service and returns `return StatusCode((int)response.HttpCode, response);`. Paged endpoints also write the response `Metadata` to a `Pagination` header. Routing uses Asp.Versioning: default `v1.0`, template `api/v{version:apiVersion}/[controller]`, lowercase URLs. The version is read from the URL segment only, and the versioned API explorer groups endpoints as `v1` for Swagger with the version substituted into each documented route. Gate access with `[Authorize(Roles = Roles.Administrator)]` (constants in `Application/Commons/Roles.cs`); `ReservationsController` also admits `Roles.Operator`, the Role limited to Reservations; keep `[AllowAnonymous]` for endpoints that return no personal data (Passenger names or document numbers never qualify).

### Composition (`Program.cs`)

Everything composes through extension methods: `AddApplicationLayer` / `AddPersistenceLayer` plus the API-level `Add*Configuration` methods. `ValidateOnBuild` + `ValidateScopes` are on, so a missing or mis-scoped registration fails at startup — and therefore fails every component test. Right after `Build()`, `EnsureAdministratorAsync()` runs the administrator bootstrap and stops startup if it fails. A fatal startup error is logged and rethrown, so the process exits non-zero; only EF's design-time `HostAbortedException` is let through. `UseSerilogRequestLogging()` is deliberately the outermost middleware, ahead of `UseErrorHandlerMiddleware()`; `UseSecurityHeaders()` follows, and `UseHsts()` runs outside Development.

### Errors, auth and logging

- `ErrorHandlerMiddleware` logs unhandled exceptions and converts them into the `JsonResponse` envelope; `Middlewares/ExceptionResponseMapper` picks the status: SQL Server 547 (foreign key) and 2601/2627 (unique) → `409`, exhausted retries or a transient fault → `503` with `Retry-After`, a cancellation by the client → `499` with no body, anything else → `500`.
- Authn/authz failures are rendered as the same envelope, with the controllers' JSON options, by `Middlewares/EnvelopeAuthorizationResultHandler` (an `IAuthorizationMiddlewareResultHandler`): `401` with `ErrorMessages.Unauthorized` and `403` with `ErrorMessages.Forbidden`, for any authentication scheme.
- JWTs are issued by `AccountService.LoginAsync`, which signs in through `SignInManager` so Identity lockout applies and refuses inactive users. Unknown, inactive and locked-out accounts all get the same `400 InvalidCredentials`. The login endpoint is rate-limited per client IP (`Extensions/RateLimitingExtensions`, 10 attempts a minute, `429` in the envelope); behind a reverse proxy, forward the client IP or all callers share one limit. Each token carries the user's security stamp; `OnTokenValidated` calls `IAccountService.IsTokenUserValidAsync` on every request and rejects the token once the user is inactive or the stamp has rotated (user updates rotate it). That is one user lookup per authenticated request.
- Log through `ILogger<T>` (Serilog underneath) with structured templates (`"{Method} {Path}"`), keeping values as template arguments.

## Configuration

Supply settings through User Secrets (loaded in `Development` only) or environment variables, which take precedence — the committed `appsettings.json` ships empty. Every section below is bound with data-annotation validation and `ValidateOnStart` (`AddApplicationLayer`), so an invalid value stops startup with an `OptionsValidationException` naming it:

- `JwtSettings`: `Secret` (at least 32 bytes for HS256; 50+ mixed characters recommended), `ExpiryTime` (`HH:mm`, 1 minute to 7 days), `TokenIssuer`, `TokenAudience`
- `ConnectionStrings:DefaultConnection` — SQL Server connection string
- `CachingSettings`: `SlidingExpiration`, `AbsoluteExpiration` (minutes)
- `LockoutSettings`: `MaxFailedAccessAttempts`, `LockoutTimeInMinutes` (defaults 5 / 15)
- `AdminBootstrap`: `Email`, `Password` (optional `FirstName`, `LastName`) — creates the first `Administrador` at startup when none exists; the password must meet `Application/Commons/PasswordPolicy` (12+ characters, 6 distinct, upper, lower, digit, symbol) or startup stops; set it only in User Secrets / environment, and remove it once the administrator exists
- `Cors:AllowedOrigins` — browser front ends allowed to call the API. Unset: any origin in Development, none elsewhere.
- `Serilog` section drives logging: Console + daily rolling file at `Logs/gotransport-*.log` under this project (7-day retention).

`appsettings.ComponentTesting.json` holds dummy JWT/caching values for the component tests. Developers keep local edits to `appsettings.json`; commit only intended changes to it.
