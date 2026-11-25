# GoTransport.Api

**Responsibility:** the thin HTTP layer and composition root — versioned controllers, middleware, DI/middleware wiring, and configuration.

## Contents

- `Controllers/v1/` — one controller per resource, plus `AccountController` (login).
- `Extensions/` — `ServiceExtensions` (versioning, JWT, CORS, Swagger, validation response), `LoggingExtensions` (Serilog), `HealthCheckExtensions`, `AppExtensions` (middleware registration).
- `Middlewares/ErrorHandlerMiddleware.cs`.
- `Program.cs`, `appsettings*.json`, `Properties/launchSettings.json`.

## Dependencies

Application and Persistence. Controllers depend only on `I<Entity>Service` interfaces.

## Conventions

### Controllers

Each action calls the service and returns `return StatusCode((int)response.HttpCode, response);`. Paged endpoints also write the response `Metadata` to a `Pagination` header. Routing uses Asp.Versioning: default `v1.0`, template `api/v{version:apiVersion}/[controller]`, lowercase URLs. Gate access with `[Authorize(Roles = Roles.Administrator)]` (constants in `Application/Commons/Roles.cs`); keep `[AllowAnonymous]` for endpoints that return no personal data (Passenger names or document numbers never qualify).

### Composition (`Program.cs`)

Everything composes through extension methods: `AddApplicationLayer` / `AddPersistenceLayer` plus the API-level `Add*Configuration` methods. `ValidateOnBuild` + `ValidateScopes` are on, so a missing or mis-scoped registration fails at startup — and therefore fails every component test. Right after `Build()`, `EnsureAdministratorAsync()` runs the administrator bootstrap and stops startup if it fails. `UseSerilogRequestLogging()` is deliberately the outermost middleware, ahead of `UseErrorHandlerMiddleware()`.

### Errors, auth and logging

- `ErrorHandlerMiddleware` logs unhandled exceptions and converts them into the `JsonResponse` envelope, translating SQL Server error 547 (foreign-key conflict) and 2601 (unique conflict) into `400`s.
- Authn/authz failures are rendered as the same envelope by the `JwtBearerEvents` in `ServiceExtensions.AddJwtAuthentication`.
- JWTs are issued by `AccountService.LoginAsync`, which signs in through `SignInManager` so Identity lockout applies and refuses inactive users. Each token carries the user's security stamp; `OnTokenValidated` calls `IAccountService.IsTokenUserValidAsync` on every request and rejects the token once the user is inactive or the stamp has rotated (user updates rotate it). That is one user lookup per authenticated request.
- Log through `ILogger<T>` (Serilog underneath) with structured templates (`"{Method} {Path}"`), keeping values as template arguments.

## Configuration

Supply settings through User Secrets or environment variables — the committed `appsettings.json` ships empty:

- `JwtSettings`: `Secret` (≥50 chars, mixed characters), `ExpiryTime` (`HH:mm`), `TokenIssuer`, `TokenAudience`
- `ConnectionStrings:DefaultConnection` — SQL Server connection string
- `CachingSettings`: `SlidingExpiration`, `AbsoluteExpiration` (minutes)
- `LockoutSettings`: `MaxFailedAccessAttempts`, `LockoutTimeInMinutes` (defaults 5 / 15)
- `AdminBootstrap`: `Email`, `Password` (optional `FirstName`, `LastName`) — creates the first `Administrador` at startup when none exists; set it only in User Secrets / environment, and remove it once the administrator exists
- `Serilog` section drives logging: Console + daily rolling file at `Logs/gotransport-*.log` under this project (7-day retention).

`appsettings.ComponentTesting.json` holds dummy JWT/caching values for the component tests. Developers keep local edits to `appsettings.json`; commit only intended changes to it.
