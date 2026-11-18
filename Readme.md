# GoTransport API

REST API for an intercity passenger-transport company in Colombia. Staff use it to manage the transport network (Points, Routes, Schedules), the fleet (Vehicles), the Reservations that Passengers hold on each Trip, and the staff Users themselves.

Despite the repository name, there is no Go code here: "Go" is the product name, and the API is built with .NET. The domain vocabulary used throughout the code and docs is defined in [`GLOSSARY.md`](GLOSSARY.md).

## Tech stack

- **.NET 6 / C#**, ASP.NET Core Web API, versioned routes (`/api/v1`). `net6.0` is out of support; moving to .NET 8 is planned (F-06).
- **EF Core 7.0.17** on **SQL Server**, with queries expressed as **Ardalis.Specification** objects.
- **ASP.NET Core Identity** with **JWT bearer** authentication and account lockout.
- **FluentValidation** for request validation, **Serilog** for structured logging, health checks, and **Swagger** in Development.
- **xUnit**, **Moq** and **EF Core InMemory** for unit and component tests.

## Repository layout

```text
.
├── src/
│   ├── GoTransport.Domain/          # entities and the auditing base
│   ├── GoTransport.Application/     # business logic: services, DTOs, validators, specifications
│   ├── GoTransport.Persistence/     # EF Core DbContext, configurations, migrations, Identity storage
│   └── GoTransport.Api/             # controllers, middleware, configuration (startup project)
├── tests/
│   ├── GoTransport.Api.Unit.Tests/
│   ├── GoTransport.Api.Component.Tests/
│   └── GoTransport.Api.Test.Utilities/
├── database/seed/                   # SQL seed scripts
├── docs/                            # technical audit and improvement plan
├── .claude/                         # Claude Code settings, rules and skills
└── GLOSSARY.md                      # domain vocabulary
```

Dependencies point one way: Api → Persistence → Application → Domain.

## Getting started

### Prerequisites

- .NET SDK 6.0 or later, plus the .NET 6 runtime
- SQL Server
- EF Core CLI: `dotnet tool install --global dotnet-ef --version 7.0.17`

### 1. Configure

The committed `appsettings.json` ships without secrets. Provide them through User Secrets (or environment variables):

```bash
cd src/GoTransport.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=<server>;Initial Catalog=GoTransport;User ID=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=True"
dotnet user-secrets set "JwtSettings:Secret" "<at least 50 characters, mixing letters, digits and symbols>"
dotnet user-secrets set "JwtSettings:TokenIssuer" "<issuer>"
dotnet user-secrets set "JwtSettings:TokenAudience" "<audience>"
dotnet user-secrets set "AdminBootstrap:Email" "<first administrator email>"
dotnet user-secrets set "AdminBootstrap:Password" "<first administrator password>"
```

| Section | Keys | Notes |
|---|---|---|
| `ConnectionStrings` | `DefaultConnection` | SQL Server connection string |
| `JwtSettings` | `Secret`, `ExpiryTime`, `TokenIssuer`, `TokenAudience` | `ExpiryTime` is `HH:mm` (default `01:00`) |
| `CachingSettings` | `SlidingExpiration`, `AbsoluteExpiration` | Minutes |
| `LockoutSettings` | `MaxFailedAccessAttempts`, `LockoutTimeInMinutes` | Defaults 5 attempts / 15 minutes |
| `AdminBootstrap` | `Email`, `Password`, optional `FirstName`, `LastName` | Creates the first `Administrador` at startup; remove it once that user exists |
| `Serilog` | — | Console plus a daily rolling file in `src/GoTransport.Api/Logs/` (7-day retention) |

### 2. Create the database

```bash
dotnet ef database update --project src/GoTransport.Persistence --startup-project src/GoTransport.Api
```

Then run the seed scripts from [`database/seed`](database/seed):

1. `INSERT_SEED.sql` (**required**): roles, identification types and vehicle manufacturers.
2. `INSERT_SEED_COLOMBIA.sql` (optional): Colombia's 33 departments and 1,122 municipalities.

The scripts are Windows-1252 encoded. They create no users.

> **Upgrading an existing database?** Earlier versions of `INSERT_SEED.sql` created an administrator whose password is public in this repository's history. Change that password on any database seeded with them.

### 3. Run

```bash
dotnet run --project src/GoTransport.Api
```

- **Address:** the API listens on `https://localhost:7046`.
- **Swagger UI:** available at `/swagger` in the Development environment.
- **Health:** `/health` reports the API and database health.
- **First administrator:** on startup, if no administrator exists and `AdminBootstrap` is set, the API creates one. If that creation fails, for example because the password breaks the policy, the API stops with the reason.

Log in with `POST /api/v1/account/login` (`{ "email": "...", "password": "..." }`) to get a JWT, then send it as `Authorization: Bearer <token>`.

## API overview

All routes live under `/api/v1` and are lowercase. Swagger documents the full contract.

| Resource | Routes | Access |
|---|---|---|
| Account | `POST /account/login` | Anonymous |
| Departments | `/departments` (CRUD, paged list, `/all`) | Administrador |
| Cities | `/cities` (CRUD, paged list), `GET /departments/{departmentId}/cities` | Administrador |
| Points | `/points` (CRUD, paged list, `/all`) | Administrador |
| Routes | `/routes` (CRUD, `/all`); the paged list is anonymous | Administrador |
| Schedules | `/schedules` (get by id, create, update, delete), `GET /routes/{routeId}/schedules` | Administrador |
| Vehicles | `/vehicles` (CRUD, paged list, `/all`) | Administrador |
| Manufacturers | `GET /manufacturers/all` | Administrador |
| Identification types | `GET /identification-types/all` | Anonymous |
| Reservations | `/reservations` (CRUD, `/all`, passenger details), `GET /schedules/{scheduleId}/reservations` | Any signed-in User; the per-Schedule list is anonymous (F-27) |
| Users | `/users` (paged list, get, create, update) | Administrador |
| Roles | `GET /roles/all` | Administrador |

Every response uses the same envelope, for example:

```json
{ "httpCode": 200, "ok": true, "data": { } }
```

Failed requests return `ok: false` with an `errors` list. Paged endpoints also return paging metadata in a `Pagination` response header.

## Tests

```bash
dotnet test tests/GoTransport.Api.Unit.Tests        # services in isolation, mocked repositories
dotnet test tests/GoTransport.Api.Component.Tests   # full HTTP pipeline over EF Core InMemory
```

Neither suite needs SQL Server, seed data or secrets.

## Contributing

- **Improvement plan:** work follows the plan in [`docs/audit/INFORME_AUDITORIA.md`](docs/audit/INFORME_AUDITORIA.md) §6.2 (items `F-01`…, in Spanish).
- **Commits:** English messages prefixed with `[Feature]`, `[Fix]`, `[Refactor]` or `[Test]`, referencing the plan item they implement (e.g. `Implements F-24.`).
- **Language:** new code text (logs, messages, comments) is written in English. Existing Spanish text and persisted domain values such as `Administrador`, `Operario` and `SISTEMA` stay as they are.
- **AI agents:** the root [`CLAUDE.md`](CLAUDE.md) and a `CLAUDE.md` in each area describe conventions for Claude Code. `.claude/skills` holds the `new-crud-resource` and `implement-plan-item` workflows.

## License

[MIT](LICENSE.txt)
