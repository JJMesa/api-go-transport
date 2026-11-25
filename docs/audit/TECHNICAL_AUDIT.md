# Technical Audit — GoTransport.Api

> **Project:** GoTransport.Api (ASP.NET Core Web API, .NET 6) · **Baseline:** 2026-06-22 · **Re-assessment:** 2026-11-17
> **Scope:** whole solution (`GoTransport.sln`) · **Depth:** exhaustive · **Mode:** read-only
> **Method:** 14 weighted dimensions. The re-assessment re-verified every baseline finding against the current code, searched for new findings by dimension group, and verified every new 🔴/🟠 by hand. Severities are the ones that survived verification.
> **Conventions:** paths are relative to the repository root and line numbers are as of the re-assessment. Domain terms (Point, Route, Schedule, Trip, Reservation, Passenger, Capacity…) follow [`GLOSSARY.md`](../../GLOSSARY.md). The language policy for new code text lives in the root [`CLAUDE.md`](../../CLAUDE.md).

---

## 1. Executive summary

- **Overall score: 5.6 / 10 — Acceptable** (baseline 6.0).
- **Why the score dropped although seven plan items were done:**
  - The fixes are real: anonymous reservation writes, brute-force login, zero observability, non-reproducible component tests and the seeded admin credential are all gone, and the testing, observability and maintainability scores rose.
  - The re-assessment went deeper than the baseline, especially into data access and business rules. It found defects that already existed in June, including a 🔴 that has been there since 2024. The weighted score reflects that wider evidence, not a regression of the code.
- **Verdict:** a well-organised layered API that is now reproducibly tested, logged and documented for humans and AI agents. It is not production-ready:
  - The reservation flow, the core of the business, has broken or bypassable rules: cancellation always fails, Capacity is counted per Schedule instead of per Trip, and updates skip the Capacity and travel-date checks.
  - The runtime and several packages carry known vulnerabilities with no supported patch line.
  - All of it is fixable with bounded, mostly small changes.
- **Top 3 strengths:**
  1. Clean layering with thin controllers and the Specification pattern, which pages and projects to DTOs in SQL.
  2. Engineering hygiene that now exists: structured logging, health checks, DI validated at startup, 121 isolated green tests.
  3. Extreme consistency across the 12 resource services, plus documentation that keeps it that way: per-area `CLAUDE.md` files, a glossary and a tracked plan.
- **Top 3 urgent risks:**
  1. 🔴 **Every Reservation cancellation fails with a 500** (A-36 → F-28). *Resolved on 2026-11-19.*
  2. 🟠 **Known-vulnerable runtime and packages:** `net6.0` is out of support, and so are .NET 8 and 9 since 2026-11-10. High-severity advisories affect transitive packages (A-06, A-42 → F-06, F-34). *Resolved on 2026-11-24.*
  3. 🟠 **Personal data and access control:**
     - The Passenger list of any Schedule is anonymous (A-35). *Resolved on 2026-11-19.*
     - Deactivated users keep signing in (A-43). *Resolved on 2026-11-24.*
     - Passenger document numbers land in request logs (A-51). *Resolved on 2026-11-24.*

---

## 2. Project context (Phase 0)

| Aspect | Detail |
|---|---|
| **Stack** | ASP.NET Core Web API (REST), C# 10 |
| **Target framework** | `net6.0` in all **7** projects. **Out of support** since 2024-11-12. .NET 8 and 9 also reached end of support on 2026-11-10, so the supported LTS target is **.NET 10** (supported until November 2028). |
| **Type** | Headless REST API with JWT bearer authentication |
| **Purpose** | Intercity passenger transport in Colombia: network (Points, Routes, Schedules), fleet (Vehicles), Reservations, staff Users |
| **Architecture** | Layered (N-tier with Clean leanings): `Api → Persistence → Application → Domain`. Repository reorganised into `src/`, `tests/`, `docs/`, `database/` (F-26). |
| **Persistence** | EF Core 7.0.17 on SQL Server, ASP.NET Core Identity with `long` keys, Ardalis.Specification, a single migration |
| **Key dependencies** | FluentValidation 11.3, Ardalis.Specification (**7.0.0 in Application / 8.0.0 in Persistence** ⚠️), Serilog.AspNetCore 6.1, Swashbuckle 6.5, Asp.Versioning.Mvc 6.4.1, JwtBearer 6.0.28. AutoMapper was removed in favour of manual mappings (`4d6f553`). |
| **Tests** | xUnit + Moq; component tests run over EF Core InMemory. **85 unit + 36 component tests, all green.** Coverage: 5 of 14 service classes and 1 of 12 controllers. |
| **Package governance** | No `Directory.Build.props`, `Directory.Packages.props` or `global.json` |
| **Tooling / CI** | No `.editorconfig` or explicit analyzers. `.github/workflows` is still empty. AI-agent docs exist: per-area `CLAUDE.md`, `GLOSSARY.md`, Claude Code skills. |

**Weighting:** the methodology's default weights, unchanged from the baseline so the two scores are comparable. All 14 dimensions apply.

**Empirical verification:**

| Step | Result |
|---|---|
| Baseline | A `net6.0` project replicating the DI chain settled the `IMemoryCache` question. |
| Test suite | `dotnet test` ran 121 tests, all green. |
| Package scan | `dotnet list package --vulnerable --include-transitive` produced the A-42 results. |
| .NET support dates | Confirmed against the Microsoft .NET Blog. |

Items marked *(inferred)* were reasoned from the code but not executed.

---

## 3. Scores by dimension

| # | Dimension | Weight | Jun 2026 | Nov 2026 | Δ | Comment |
|---|---|:---:|:---:|:---:|:---:|---|
| 1 | Architecture & design | 18% | 7.0 | **6.5** | −0.5 | Layers and thin controllers hold. HTTP concerns leak into Application and Persistence, and the same DTO returns in different shapes per endpoint. |
| 2 | Code quality & C# | 14% | 6.5 | **6.0** | −0.5 | Uniform services and mappings. Copy-paste bugs (route search, city totals), nullable annotations defeated, dead code. |
| 9 | Security | 14% | 5.5 | **5.0** | −0.5 | Anonymous writes, lockout and seeded credential fixed. Open: High CVEs in transitive packages, anonymous Passenger list, deactivated users sign in. |
| 11 | Testing | 12% | 4.5 | **5.5** | +1.0 | Reproducible, isolated, 121 green tests. Still 1 of 12 controllers, no 401/403 tests, and mocks hide data-loading bugs. |
| 7 | Data access | 8% | 6.5 | **5.0** | −1.5 | Server-side paging and projection. "Last modified by" never updates, no unique constraints, check-then-insert races, missing `Include`s. |
| 10 | Errors & resilience | 7% | 5.0 | **5.0** | 0.0 | The global handler now logs. Every cancellation crashes, DB errors map to 400, no transient-fault retry, fatal startup exits 0. |
| 8 | API design | 6% | 6.3 | **5.0** | −1.3 | Consistent envelope and versioning. Reservations omit the travel date, status codes and shapes are inconsistent, and an anonymous route exposes personal data. |
| 3 | Patterns / anti-patterns | 5% | 7.0 | **6.0** | −1.0 | Specification, Options and the Result envelope are used well. Specifications are duplicated and use conditional tricks; no unit of work. |
| 6 | Async & concurrency | 5% | 7.2 | **6.5** | −0.7 | Still no sync-over-async. Check-then-insert races on Capacity and uniqueness; no `CancellationToken` on writes. |
| 12 | Configuration / observability | 4% | 3.5 | **6.0** | +2.5 | Serilog, health checks, Options. User Secrets precedence, no startup validation, personal data in request logs, no tracing. |
| 4 | Dependencies (NuGet) | 3% | 5.0 | **3.5** | −1.5 | Runtime out of support, High CVEs in transitive packages, version drift. |
| 5 | Dependency injection | 2% | 6.5 | **6.0** | −0.5 | `ValidateOnBuild`/`ValidateScopes` on. Settings never validated, reflection-registration quirks, dead registrations. |
| 13 | Performance | 1% | 6.0 | **5.5** | −0.5 | Paging in SQL. Unbounded cached Reservation list and a full-scan Passenger lookup. |
| 14 | Maintainability / tooling | 1% | 5.0 | **6.5** | +1.5 | README, per-area `CLAUDE.md`, glossary, tracked plan. Still no CI or analyzers. |

**Weighted overall: 6.0 → 5.6 / 10**

---

## 4. Strengths

- **[Architecture]** One-way dependencies with no cycles, and controllers that only delegate: every action is `return StatusCode((int)response.HttpCode, response);`.
  - Evidence: `src/GoTransport.Api/Controllers/v1/CitiesController.cs`.
  - The per-area `CLAUDE.md` files document and protect these boundaries.
- **[Data / Patterns]** Specifications page with `Skip`/`Take` and **project to DTOs in SQL** (`Specification<City, CityDto>`), so there are no N+1 queries and no lazy loading.
  - Evidence: `src/GoTransport.Application/Specifications/Cities/PagedCitySpecification.cs`.
- **[Async]** `async`/`await` end to end, with no `async void`, `.Result` or `.Wait()` in `src/`. `CancellationToken` flows through all reads.
- **[Quality]** About 12 resource services follow one pattern:
  - manual `ToDto`/`ToEntity`/`ApplyTo` mappings (`src/GoTransport.Application/Mappings/`);
  - one `ResponseBuilder` envelope instead of exceptions for expected failures;
  - audit stamping centralised in `ApplicationDbContext.SaveChangesAsync`.
- **[Config / Security]** Options pattern with **no secrets in the repo**. The committed `appsettings.json` ships empty, and the first administrator comes from configuration (`AdminBootstrapService`, `801b35c`).
- **[Observability / DI]** Serilog with a bootstrap logger and `CloseAndFlush`, structured templates, a `/health` check against the database, and `ValidateOnBuild` + `ValidateScopes`, which turn wiring errors into startup failures.
  - Evidence: `src/GoTransport.Api/Program.cs`.
- **[Testing]** Component tests run the real pipeline over an isolated InMemory database that is reset before each test, with a test authentication handler.
  - Unit tests use AAA, `[Theory]`, verified mocks and Object Mothers.
  - Evidence: `tests/GoTransport.Api.Component.Tests/Infrastructure/CustomWebApplicationFactory.cs`.

---

## 5. Findings

Each finding has a stable ID: **A-01…A-40** come from the baseline (A-35…A-40 were added on 2026-11-17 while building the glossary), and **A-41+** are new in this re-assessment. Each finding links to the plan item that fixes it (§7). A severity in *(baseline X → Y)* was re-graded during verification.

### 5.1 Open findings

#### 🔴 Critical

None open. A-36 was resolved by F-28 (see §5.2).

#### 🟠 High

- **A-04 — Thin test coverage** *(partial)*.
  - Unit tests cover 5 of 14 service classes; component tests cover 2 of 12 controllers (`CitiesControllerTests`, and `ReservationsControllerTests` for delete and the per-Schedule list only).
  - Detailed test gaps are in A-63. → **F-55**
#### 🟡 Medium

- **A-11 — Multi-step user writes are not atomic** *(partial: `IUnitOfWork` exists since F-29)*.
  - `UserService.CreateAsync` and `AdminBootstrapService` still create the user and assign the role in separate saves.
  - **Impact:** a failure in between leaves a user with no role, and the bootstrap then fails on every restart. → **F-16**
- **A-17 — Persistence depends on Application** for `IRepository<T>`, settings, `Constants` and `ToPacificStandardTime()`. → **F-16**
- **A-22 — No `.editorconfig` or analyzers** *(partial: CI exists since F-11)*. Nothing enforces a code style or analyzer rules, and warnings do not fail the build. → **F-12**
- **A-23 — Domain depends on ASP.NET Core Identity**, which pulls EF Core into Domain. → **F-16**
- **A-60 — HTTP and Identity UI leak into inner layers** *(new; partly subjective)*.
  - Services return `HttpStatusCode`, and `UserService` uses `[FromQuery]`.
  - `ApplicationDbContext` depends on `IHttpContextAccessor` (`:16`).
  - `AddDefaultIdentity` (`src/GoTransport.Persistence/RegisterLayerExtensions.cs:31`) registers cookie authentication and the Razor UI in a JWT-only API. → **F-52**
- **A-61 — Inconsistent time handling** *(new)*.
  - `ToPacificStandardTime()` actually converts to Colombia time (Windows id `SA Pacific Standard Time`, UTC−5) and looks the zone up on every call.
  - Audit columns store local time without an offset.
  - The "trip passed" check uses the server's `DateTime.Now` (`ReservationService.cs:110`). → **F-53**
- **A-62 — No correlation id or tracing** *(new)*. Error responses carry no trace id and logs have no request enrichment, so a client error cannot be matched to its log line. → **F-54**
- **A-63 — Test gaps that let defects through** *(new)*.
  - Few authorization tests: the per-Schedule Reservation list has 401, 403 and Operator cases (F-27, F-46); other controllers have none. `TestAuthHandler` can now sign in with any Role.
  - 6 of 18 validators have tests (User creation, City, Department and Vehicle limits); the Reservation, Route, Point and Schedule validators have none.
  - No specification runs against a relational provider, and the Reservation Object Mother hides A-36. → **F-55**
- **A-64 — Nullable annotations are defeated** *(new)*. 7 `?.ToDto()!` and 19 `IsActive!.Value` in Application, plus `Errors = null!` in `JsonResponse`. The root cause is the tri-state `bool? IsActive` on entities. → **F-56**
#### 🟢 Low

- **A-25 — Reflection-based DI over `AppDomain` assemblies.** It also filters with `inherit: true` but registers with `inherit: false`, so a subclass that only inherits the attribute is silently skipped (`src/GoTransport.Application/RegisterLayerExtensions.cs:39` vs `:49-56`). → **F-15**
- **A-26 — Thin generic repository** with an unused public `Context`. → **F-15**
### 5.2 Resolved since the baseline

| ID | Finding | Resolved by |
|---|---|---|
| A-01 | 🔴 Anonymous Reservation writes (`[AllowAnonymous]` on POST/PUT) | F-01 / QW-1 (`f92a5ce`) |
| A-02 | 🟠 Login without brute-force protection | F-03 / QW-5 (`67e2ff1`): lockout through `LockoutSettings`, `CheckPasswordSignInAsync(lockoutOnFailure: true)`, 429 on lockout |
| A-03 | 🟠 Zero observability | F-04 (`4e2df23`, `d8f7e9b`): Serilog (Console + daily file, 7-day retention), request logging, logging error middleware |
| A-05 | 🟠 Non-reproducible component tests | F-05 (`25872e3`): InMemory database per factory, deterministic seeder, `TestAuthHandler` |
| A-07 | 🟡 `IMemoryCache` only registered through `AddDefaultIdentity` | F-02 / QW-2 (`720a0cb`): explicit `AddMemoryCache()` + `ValidateOnBuild` |
| A-14 | 🟡 Error middleware did not log, read the body, and was registered last | QW-3 (`27ef48d`, `d8f7e9b`) |
| A-15 | 🟡 `CacheKey.Manufacturers` collided with `IdentificationTypes` | QW-4 (`fbca271`) |
| A-20 | 🟡 No health checks | F-04 (`4e2df23`): `/health` with a database check (see A-65 for its exposure) |
| A-28 | 🟢 Admin credential versioned in the seed | F-24 (`801b35c`). The first administrator now comes from `AdminBootstrap`. Databases seeded with the old script must still rotate that password. |
| A-35 | 🟠 Anonymous endpoint returned Passenger names and document numbers | F-27 / QW-7 (`[Fix] Require authentication on the per-Schedule Reservation list`). `GET schedules/{scheduleId}/reservations` no longer has `[AllowAnonymous]`, so the controller's `[Authorize]` applies. Component tests pin 401 without credentials (and no Passenger data in the body) and 200 when signed in. Role-based access to Reservations stays open as F-46. |
| A-41 | 🟠 "Last modified by" never changed | F-33 / QW-8 (`[Fix] Stamp the current user as UpdateUser on every update`). `ApplicationDbContext.SaveChangesAsync` now overwrites `UpdateUser` on `Modified` entries instead of `??=`. A component test updates a seeded City as a signed-in user and reads the row back: `CreationUser` stays `SISTEMA`, `UpdateUser` becomes the signed-in user. Rows updated before the fix still show their creator as last modifier. |
| A-36 | 🔴 Every Reservation cancellation failed with a 500 | F-28 / QW-6 (`[Fix] Load the Schedule when cancelling a Reservation`). `DeleteAsync` now loads the Reservation through `ReservationSpecification`, which includes `Schedule`. Component tests over the real pipeline cover an upcoming Trip (204), a past travel date (400) and an unknown id (404). |
| A-43 | 🟠 Deactivated users kept signing in; role changes on update were ignored | F-35 / QW-10 (`[Fix] Retire the access of deactivated users and apply role changes`). Login answers an inactive account like an unknown one. `UserService.UpdateAsync` saves through `UserManager`, rotates the security stamp and applies `RoleId`. Tokens now carry the stamp, and `OnTokenValidated` rejects a token whose user is inactive or whose stamp has rotated, so existing tokens stop working at once. Tokens issued before the fix have no stamp claim and are rejected: every user signs in again once. |
| A-45 | 🟠 Reservation responses omitted the travel date | F-37 / QW-11 (`[Fix] Return the travel date in Reservation responses`). `ReservationDto.ReservationDate` is filled by `ToDto` and by the `PagedReservationSpecification` projection, under the same name the create and update requests use. Component tests read it back from get-by-id and the per-Schedule list. |
| A-06 | 🟠 Runtime out of support (`net6.0`) | F-06 (`[Feature] Move to .NET 10 with central build and package management`). All 7 projects target `net10.0` (LTS, supported until November 2028) through the root `Directory.Build.props`; `global.json` pins the 10.0 SDK band. |
| A-42 | 🟠 Vulnerable packages in the dependency closure | F-06 / F-34 / QW-9 (same commit). The upgrade replaced every flagged package; `dotnet list package --vulnerable --include-transitive` now reports none in any project, so nothing had to be pinned. NuGet Audit warns on restore if one reappears. |
| A-66 | 🟡 Package and test-stack drift | F-06 (same commit). `Directory.Packages.props` holds one version per package: ASP.NET Core and EF Core on 10.0.15, Ardalis.Specification 10 in both layers (core package only in Application), one xunit/coverlet/Test SDK stack for both test projects. EF InMemory left the unit tests and UserSecrets the component tests. `Identity.UI` stays until F-52 replaces `AddDefaultIdentity`. |
| A-33 | 🟢 Conservative C# (no records, no primary constructors) | F-06 (same commit). The code now compiles as C# 14, so the language ceiling is gone; existing classes are not rewritten, and new code may use records and primary constructors. |
| A-22 (CI part) | 🟡 No CI | F-11 (`[Feature] Add a CI workflow with tests and a vulnerable-package gate`). `.github/workflows/ci.yml` builds in Release, runs both suites with coverage, uploads the results, and fails on High/Critical advisories in the dependency closure. The `.editorconfig`/analyzer part stays open as F-12. |
| A-37 | 🟠 Capacity counted per Schedule, not per Trip, and not atomically | F-29 (`[Fix] Count Capacity per Trip inside a serializable transaction`). `CreateAsync` counts the active Reservations on the Trip (`TripReservationSpecification`: Schedule + travel date) and inserts inside `IUnitOfWork.ExecuteInTransactionAsync` with `Serializable`, so a concurrent booking waits or deadlocks and is retried instead of overselling. A migration replaces the `SCHEDULE_ID` index with (`SCHEDULE_ID`, `RESERVATION_DATE`) to keep those range locks to one Trip. Travel dates are stored without a time part. |
| A-38 | 🟡 An unknown `ScheduleId` gave 500 instead of 404 | F-30 (`[Fix] Answer 404 for a Reservation on an unknown Schedule`). `CreateAsync` loads the Schedule with its Vehicle (`ScheduleSpecification`) and returns 404 with `ErrorMessages.InvalidSchedule` when it does not exist. The unused `VehicleSpecification(scheduleId)` is gone. |
| A-44 | 🟠 Updating a Reservation bypassed the Trip rules | F-36 (`[Fix] Apply the Trip rules when a Reservation moves`). When an update changes the Schedule or the travel date, the target Trip must exist (404), must not have taken place (400 `TravelDatePassed`) and must have a free place, not counting the Reservation itself (409), all under the same `Serializable` transaction as a booking. Editing a Reservation without moving it skips the Capacity check. New bookings now also reject a Trip that has already taken place. |
| A-46 | 🟠 `GET reservations/all` was unbounded and cached | F-38 (`[Fix] Replace the unbounded Reservation list with a per-Trip filter`). The endpoint and `ReservationService.GetAllAsync` are gone, and Reservations are no longer cached. To list one Trip, `GET schedules/{scheduleId}/reservations` takes an optional `travelDate`, paged like the rest of that list. |
| A-39 | 🟡 Overnight Schedules got a negative `Duration` and the wrong "trip passed" moment | F-31 (`[Fix] Support Schedules that arrive the next day`). `ScheduleExtensions.TripDuration` counts an arrival at or before the departure as next-day, and `ArrivalOn(travelDate)` gives the real arrival, used by every "Trip has taken place" check. The Schedule validators accept 00:00 and reject an arrival equal to the departure. A negative `time` value could never be saved, so no stored row needs fixing. |
| A-47 | 🟡 The Route destination search used the origin text | F-39 (`[Fix] Search Route destinations by the destination text`). Both Route specifications search the destination City and Point with `DestinationPointSearchCriteria` in search group 2, so origin and destination terms combine with AND. Component tests cover origin-only, destination-only and combined searches. |
| A-40 | 🟢 A Route could have the same origin and destination Point | F-32 (`[Fix] Reject Routes whose origin and destination are the same Point`). Both Route validators require `DestinationPointId` to differ from `OriginPointId`, so create and update answer 400. |
| A-08 | 🟡 Two different JWT keys; custom lifetime validator with no clock skew | F-07 (`[Fix] Sign and validate tokens with one key and standard lifetime rules`). `JwtSettings.CreateSigningKey()` is the only key, used to sign in `AccountService` and to validate through `JwtSettings.CreateValidationParameters()`, which uses the built-in lifetime check with a 30-second `ClockSkew`. A unit test issues a token and validates it with those parameters, including a secret over 128 bytes. The unused singleton `TokenValidationParameters` is gone. |
| A-29 | 🟢 `iat` written with `ToLongDateString()` | F-07 (same commit). The token descriptor sets `IssuedAt`, so `iat` is a numeric date like `nbf` and `exp`. |
| A-09 | 🟡 One role per token (`roleList.First()`) | F-08 (`[Fix] Put every Role of a User in the token`). `GenerateToken` adds one `ClaimTypes.Role` claim per Role, and none for a User without Roles (it used to add an empty one). |
| A-54 | 🟡 Reservations needed a login but no Role; unknown roles were accepted | F-46 (`[Fix] Limit Reservations to Administrators and Operators`). `ReservationsController` requires `Administrador` or `Operario`. `RoleService.AssignRolesToUser` fails for an unknown role, and `UserService` checks the role before creating or updating a user (400 `UnknownRole`). `TestAuthHandler` now signs in with any Role or none, and component tests pin 200 for an Operator and 403 without a Role. |
| A-52 | 🟡 Weak password policy (Identity defaults, 6-character validator) | F-44 (`[Fix] Require strong passwords through one shared policy`). `Application/Commons/PasswordPolicy` (12+ characters, 6 distinct, upper, lower, digit, symbol) is applied to Identity's options, so it covers every stored password including the bootstrap administrator's, and `UserCreationValidator` reports the same rules on input. Existing passwords keep working until they are changed. |
| A-59 | 🟡 Auth error bodies were PascalCase, and 403 was built by `Unauthorized(...)` | F-51 (`[Fix] Render 401 and 403 in the same envelope as every other response`). `EnvelopeAuthorizationResultHandler` writes 401 (`Unauthorized`) and 403 (`Forbidden`, a new `ResponseBuilder` method) with the controllers' JSON options, for any authentication scheme, replacing the JWT `OnAuthenticationFailed`/`OnChallenge`/`OnForbidden` events. Component tests assert both bodies. |
| A-55 | 🟡 User Secrets overrode environment variables in every environment; settings were never validated | F-47 (`[Fix] Validate settings at startup and load User Secrets only in Development`). `Program.cs` no longer adds User Secrets itself: `WebApplication.CreateBuilder` loads them in `Development` only, below environment variables, so the component-test factory no longer blanks `AdminBootstrap`. JWT, caching, lockout and bootstrap settings are bound with data annotations and `ValidateOnStart`; JWT validation and Identity lockout read the validated options. `StartupValidationTests` pins a startup failure for each kind of bad value. |
| A-56 | 🟡 No transient-fault retry; a failed start exited with code 0 | F-48 (same commit). SQL Server is registered with `EnableRetryOnFailure` (transactions already go through the execution strategy in `IUnitOfWork`). `Program.cs` logs a fatal startup error and rethrows it, so the process exits non-zero, while EF's design-time `HostAbortedException` is no longer logged as fatal. |
| A-57 | 🟡 Database errors mapped to the wrong status; cancellations became 400 | F-49 (`[Fix] Map database errors and cancellations to the right status codes`). `ExceptionResponseMapper` (used by `ErrorHandlerMiddleware`) maps SQL Server 547 and 2601/2627 to 409, exhausted retries and transient faults to 503 with `Retry-After`, other SQL errors to 500, and a cancellation by the client to 499 without a body (logged as Information). A cancellation the client did not ask for is a 500. The unused `KeyNotFoundException` branch is gone. |
| A-65 | 🟡 Anonymous `/health` exposed error text | F-57 (`[Fix] Keep health check details for Administrators and split the probes`). `/health/live` runs no checks, `/health/ready` (and `/health`) runs the tagged database check, and both answer anonymous callers with the overall status only. Check names, durations and errors are served by `/health/details` to Administrators. `HealthChecksTests` covers all four endpoints. |
| A-51 | 🟡 Passenger lookup by document number: string id, prefix match, national ID in the URL | F-43 (`[Fix] Look Passengers up by exact Reservation id with the document in the body`). `POST reservations/{id:guid}/passenger-lookup` binds a `Guid`, matches it exactly (primary-key seek) together with the document number, and takes the document number from a JSON body (`PassengerLookupDto`, validated), so it never appears in a URL or in the request and error logs. The old `GET .../person/{passengerIdentification}/details` route is gone. |
| A-53 | 🟡 Login enumeration and lockout abuse | F-45 (`[Fix] Rate-limit sign-in and answer locked accounts like unknown ones`). A locked-out account now gets the same `400 InvalidCredentials` as an unknown email (the lockout is logged instead), and `POST account/login` is limited to 10 attempts a minute per client IP (`429` in the envelope, `Retry-After: 60`). A distributed attacker can still lock an account out; the limit only slows one source down. |
| A-50 | 🟡 Uniqueness rules had no database constraint | F-42 (`[Fix] Back each duplicate check with a unique index`). A migration adds unique indexes on Department description, City (Department, description), Route (origin, destination) and Vehicle license plate; the composite ones replace the single-column foreign-key indexes. A concurrent duplicate now fails with SQL error 2601, answered as 409 since F-49. `DatabaseModelTests` checks the indexes on the EF model. The Colombia seed has no duplicate names within a Department. |
| A-58 | 🟡 Validators disagreed with the database; paging input was not range-checked | F-50 (`[Fix] Align validators with the database columns and clamp the page number`). City descriptions are limited to the `varchar(32)` column, Department gets creation and update validators, Vehicle `Capacity` must be positive and the model year between 1950 and next year, and `PaginationParameters.PageNumber` is clamped in its setter, which model binding uses. `ValidatorColumnLimitsTests` covers each rule. |
| A-49 | 🟡 The same DTO came back in different shapes | F-41 (`[Fix] Return detail and create responses with their navigations`). Get-by-id for Routes, Vehicles, Schedules and Reservations uses a detail specification with the `Include`s its DTO shows (`RouteDetailSpecification`, `VehicleDetailSpecification`, `ScheduleSpecification(id)`, `ReservationDetailSpecification`), and the create endpoints of those plus City and Point reload the new row through it. `DetailResponseShapeTests` checks the navigations over the real pipeline. |
| A-48 | 🟡 Paged and count specifications were copy-pasted and had drifted | F-40 (`[Refactor] Build each page and its count from one shared filter`). Each of the seven paged/count pairs (Cities, Departments, Points, Reservations, Routes, Users, Vehicles) now calls one `<Entity>Filter.Apply`, and the count specifications no longer sort. The City count now honours `DepartmentId`, which a component test pins through the `Pagination` header. |
| A-67 | 🟢 Specification query hygiene | F-58 (`[Refactor] Tidy specification filters and escape LIKE input`). Optional filters are added only when given (no `col == @p OR @p IS NULL`), the duplicate checks compare columns directly (the CI collation already ignores case, so `ToLower` only defeated the unique indexes), and every `Search` term goes through `ToContainsPattern()`, which escapes `%`, `_` and `[` so they match literally. |
| A-69 | 🟢 Repository hygiene | F-59 (`[Refactor] Fill in the license holder and ignore IDE, env and log files`). `LICENSE.txt` names the copyright holder (2024, Juan Mesa) instead of the template placeholder, and `.gitignore` covers `.idea/`, `.vscode/`, `.env` files and the Serilog `Logs/` folder. |
| A-21 | 🟢 `IOptionsSnapshot<T>` for stable config | F-19 (`[Refactor] Read stable settings through IOptions`). `AccountService` and `CacheService` take `IOptions<T>`: JWT and cache settings never change at runtime, so they are bound once instead of on every request. |
| A-12 | 🟢 Sync `SaveChanges` was not overridden | F-13 (`[Fix] Stamp audit columns on synchronous saves too`). `ApplicationDbContext` overrides the two `bool acceptAllChangesOnSuccess` overloads that every `SaveChanges`/`SaveChangesAsync` call ends in, so both paths stamp the audit columns. A component test saves synchronously and reads the stamps back. |
| A-10 | 🟡 No `AsNoTracking` | F-09 (`[Refactor] Read without change tracking where nothing is saved back`). The specifications behind GET endpoints and cached catalogs (City, Department, Point, Route, Vehicle, the Schedules of a Route, Reservation detail and passenger lookup) call `AsNoTracking()`; the ones that load an entity to update stay tracked. The cached Vehicle catalog now includes the Manufacturer and the Schedules of a Route include their Vehicle. `SpecificationTrackingTests` pins both kinds. |
| A-16 | 🟡 The cache was never invalidated | F-10 (`[Fix] Drop cached catalogs on every write and keep them longer`). Department, Point, Route, Vehicle and City writes call `ICacheService.Remove` for every cached list they make stale (a Point or City write also drops the Route list, whose Routes embed them). With invalidation in place the committed TTLs rise from 1/1 to 30/120 minutes (sliding/absolute). `CatalogCacheTests` covers a create and a cross-list update over the real memory cache. |
| A-19 | 🟡 No HSTS or security headers | F-17 (`[Fix] Restrict CORS to configured origins and add HSTS and security headers`). `UseHsts` runs outside Development, and `SecurityHeadersMiddleware` adds `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` and a `default-src 'none'` content policy (Swagger UI excepted). `HttpHardeningTests` checks them. |
| A-24 | 🟢 CORS `AllowAnyOrigin` | F-17 (same commit). Origins come from `Cors:AllowedOrigins`; with none configured, Development allows any origin and every other environment allows none. Component tests cover the unconfigured and configured cases. |
| A-34 | 🟢 XML docs not exposed in Swagger; versioned API explorer missing | F-25 (`[Feature] Show XML comments in Swagger`). Api and Application generate their XML documentation (CS1591 off, so undocumented members stay optional) and Swagger includes both files. The versioned API explorer came with F-06. A Swagger test reads a DTO property description from the generated document. |
| A-30 | 🟢 Typos | F-20 (`[Refactor] Fix the Ruote typos, drop dead code and align conventions`). `RuoteParameters.cs` is `RouteParameters.cs`, and the `ruote`/`ruoteId` identifiers and `IsDuplicarePoints` are now `route`, `routeId` and `IsDuplicateRouteAsync`. The `LICENCE_PLATE` column keeps its persisted name. |
| A-32 | 🟢 Dead code | F-20 (same commit). Removed: the empty `Exceptions/` folder, `ValidateAndThrowArgumentException`, `SerializeWithCamelCase`, `ErrorMessages.ConflictValidators` and `RoleSpecification(User)`. The `KeyNotFoundException` branch (F-49) and the singleton `TokenValidationParameters` (F-07) went earlier. |
| A-68 | 🟢 Inconsistent conventions | F-20 (same commit). `CityService` is `internal` like the other services, `Metadata` takes (page number, page size, total items) in one order through `Create`, the services no longer rewrite their input DTOs (the mappings trim the free text they copy), and the controllers write the `Pagination` header through one `AddPaginationHeader` helper. |
| A-18 | 🟡 `IServiceBase` covered only 4 of 14 service interfaces | F-18 (`[Refactor] Remove IServiceBase; each service interface declares its own methods`). The resources differ too much for one base (no "all" list for Reservations, per-Department Cities, per-Route Schedules, Users without delete), so `IServiceBase` is gone and the Department, Point, Route and Vehicle interfaces declare their methods explicitly, like the other ten. |
| A-31 | 🟢 HTTP conventions | F-22 (`[Fix] Answer creates with 201 and Location, and duplicates with 409`). Every create answers 201 with a `Location` header (Reservations answered 200), and duplicates answer 409 like a full Trip. ProblemDetails is declined on purpose: [ADR 0001](../adr/0001-keep-the-json-response-envelope.md) keeps the `JsonResponse` envelope as the contract. The empty `Exceptions/` folder went with F-20. |
| A-27 | 🟢 Hard deletes despite `IsActive` columns | F-23 (`[Feature] Cancel Reservations instead of deleting them`). Reservations are where history matters: `DELETE` now cancels (sets `IsActive = false`), the cancelled Reservation stays readable, cannot be updated or cancelled again (404), and its place on the Trip is free because only active Reservations count. Catalog rows keep hard deletes; one still referenced fails with SQL error 547 (409). The glossary gains Cancellation. |

### 5.3 No longer applicable

| ID | Finding | Why |
|---|---|---|
| A-13 | 🟡 `ReservationService.GetAllAsync` loaded no navigations and cached them as `null` | F-38 removed `GetAllAsync` and the `reservations/all` endpoint, so there is nothing left to fix; F-14 is marked Won't do. |

---

## 6. Detailed analysis by dimension

**1. Architecture & design (6.5).**
- **Strengths:** the layers and dependency direction hold, the controllers are uniform one-liners, and the repository layout now matches the architecture (F-26).
- **Weaknesses:**
  - Transport and framework concerns leak inward (A-60, A-17, A-23).
  - The same DTO has different shapes depending on the endpoint (A-49).
- **Recommendation:** return a transport-neutral result from Application and map it to HTTP in Api.

**2. Code quality & C# (6.0).**
- **Strengths:** consistent, readable services and small documented mappings.
- **Weaknesses:** the visible defects are copy-paste bugs (A-47, A-48), suppressed nullability (A-64), dead code (A-32) and leftover typos (A-30).

  ```csharp
  // At re-assessment (PagedRouteSpecification) — destination searched with the origin text, all in group 1
  Query.Search(r => $"{r.DestinationPoint.Detail}", $"%{parameters.OriginPointSearchCriteria}%");
  // Recommended — right criteria, separate group so origin AND destination combine; applied by F-39
  Query.Search(r => $"{r.DestinationPoint.Detail}", $"%{parameters.DestinationPointSearchCriteria}%", group: 2);
  ```

**3. Patterns / anti-patterns (6.0).**
- **Strengths:** Specification, Options and the Result envelope are used deliberately.
- **Weaknesses:**
  - Specifications are duplicated per paged/count pair (A-48) and rely on conditional tricks (A-67). A conditional `Include` is the root cause of A-36.
  - There is no unit of work (A-11).

**4. Dependencies (3.5).**
- **Weaknesses:**
  - The runtime is out of support (A-06), and High-severity advisories sit in the transitive closure (A-42).
  - Versions drift across layers and test projects (A-66).
- **Recommendation:** F-06 should move to `net10.0` with central package management, and F-34 pins the patched versions in the meantime.

**5. Dependency injection (6.0).**
- **Strengths:** startup validation of the container.
- **Weaknesses:** User Secrets take precedence over environment variables (A-55), settings are never validated (A-55), and the reflection registration has quirks (A-25).

**6. Async & concurrency (6.5).**
- **Strengths:** no sync-over-async.
- **Weaknesses:**
  - The real concurrency risks are check-then-insert races on Capacity (A-37) and uniqueness (A-50).
  - Writes take no `CancellationToken` (F-21).

**7. Data access (5.0).**
- **Strengths:** queries are efficient where it matters: paging and DTO projection in SQL.
- **Weaknesses:**
  - Correctness gaps cluster here: A-41, A-36, A-49, A-13 and A-50.
  - Constraints that business rules rely on must live in the database (unique indexes), and multi-step writes need a transaction.

  ```csharp
  // At re-assessment (ApplicationDbContext.SaveChangesAsync, Modified branch)
  entry.Entity.UpdateUser ??= username;
  // Recommended — applied by F-33
  entry.Entity.UpdateUser = username;
  ```

**8. API design (5.0).**
- **Strengths:** versioning and the envelope are consistent.
- **Weaknesses:**
  - The Reservation contract is incomplete (A-45) and its endpoints expose personal data (A-35, A-51).
  - Status codes are inconsistent (A-31, A-57, A-59).
  - Some input is not validated against the schema (A-58).

**9. Security (5.0).**
- **Fixed:** the critical gaps of June (A-01, A-02, A-28).
- **Remaining work**, roughly in priority order:
  1. Personal-data exposure (A-35, A-51).
  2. Account lifecycle (A-43, A-54).
  3. Vulnerable packages (A-42).
  4. Login abuse (A-52, A-53).
  5. Token handling (A-08, A-09, A-29).
  6. Transport hardening (A-19, A-24).

  ```csharp
  // At re-assessment (AccountService.LoginAsync)
  var user = await _userManager.FindByEmailAsync(userLogin.Email);
  if (user is null) return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);
  // Recommended — inactive accounts are treated like unknown ones; applied by F-35
  if (user is null || user.IsActive != true) return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);
  ```

**10. Errors & resilience (5.0).**
- **Strengths:** the global handler logs with structured templates.
- **Weaknesses:**
  - A crash on a core path (A-36).
  - Misclassified database errors (A-57).
  - No transient-fault retry, and silent startup failure (A-56).

**11. Testing (5.5).**
- **Strengths:** the suite is reproducible and fast.
- **Weaknesses:**
  - Breadth is low (A-04).
  - The missing kinds of test (authorization, token round trip, validators, relational specifications) are exactly where the open defects live (A-63).

**12. Configuration / observability (6.0).**
- **Strengths:** big improvement since June (Serilog, health checks).
- **Weaknesses:** configuration precedence (A-55), personal data in logs (A-51), no correlation (A-62) and health-check exposure (A-65).

**13. Performance (5.5).**
- **Strengths:** paging in SQL; catalog caching works.
- **Weaknesses:** caching transactional data with no bound (A-46) and the full-scan lookup (A-51) are the scaling risks. Missing `AsNoTracking` (A-10) is minor.

**14. Maintainability / tooling (6.5).**
- **Strengths:** documentation is now a strength (README, per-area `CLAUDE.md`, glossary, this tracked plan).
- **Weaknesses:** no CI, analyzers or warnings-as-errors (A-22), so nothing enforces it.

---

## 7. Improvement plan

> **Status legend:** ⬜ Open · 🟦 In progress · ✅ Done · 🔁 In review · ⏭️ Postponed · ❌ Won't do
> Update **Status** (and **Verified**) as items progress, and tick `[ ]` → `[x]` when an item is done. Item IDs are stable: commits reference them.

### 7.1 Quick wins (high impact, low effort)

| ✔ | ID | Action | Sev. | Status | Owner | PR / Commit | Verified |
|:--:|:--:|--------|:--:|:--:|---|---|:--:|
| [x] | QW-1 | Remove `[AllowAnonymous]` from `ReservationsController` POST/PUT | 🔴 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-2 | Explicit `services.AddMemoryCache();` + `ValidateOnBuild = true` | 🟡 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-3 | Inject `ILogger` into `ErrorHandlerMiddleware` and stop reading the body | 🟠 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-4 | Fix `CacheKey.Manufacturers = "manufacturersList"` | 🟡 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-5 | Identity lockout + `CheckPasswordSignInAsync(lockoutOnFailure: true)` | 🟠 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-6 | Load `Schedule` before the "trip passed" check in `DeleteAsync` (F-28) | 🔴 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-7 | Require auth on `GET schedules/{id}/reservations` (F-27) | 🟠 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-8 | Always stamp `UpdateUser` on modified entities (F-33) | 🟠 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-9 | Pin patched versions of the vulnerable packages (F-34) | 🟠 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-10 | Reject inactive users at login (F-35) | 🟠 | ✅ Done | Juan Mesa | — | ✅ |
| [x] | QW-11 | Return the travel date in `ReservationDto` (F-37) | 🟠 | ✅ Done | Juan Mesa | — | ✅ |

### 7.2 Full prioritized plan

| ✔ | ID | Action | Sev. | Dimension | Effort | Status | Owner | PR / Commit | Verified |
|:--:|:--:|--------|:--:|---|:--:|:--:|---|---|:--:|
| [x] | F-01 | Require auth on Reservations POST/PUT | 🔴 | Security/API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-02 | Explicit `AddMemoryCache()` + `ValidateOnBuild` | 🟡 | DI/Performance | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-03 | Lockout + `CheckPasswordSignInAsync` | 🟠 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-04 | Structured logging (`ILogger`/Serilog) + health checks | 🟠 | Observability | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-05 | Isolate the database in component tests + broaden unit coverage | 🟠 | Testing | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-06 | Migrate to `net10.0` (LTS; `net8.0` also lost support on 2026-11-10) + `Directory.Build.props` + CPM; align package and test-stack versions, drop unused packages (A-06, A-33, A-66) | 🟠 | NuGet/Maintainability | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-07 | One `SymmetricSecurityKey` for signing and validation; built-in lifetime validation with a small `ClockSkew`; numeric `iat` (A-08, A-29) | 🟡 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-08 | Emit every role as a claim (multi-role) (A-09) | 🟡 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-09 | `AsNoTracking` in read specifications + `Include`s in `GetAll` (A-10) | 🟡 | Data/Performance | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-10 | Invalidate the cache (`Remove`) on writes + raise the TTL (A-16) | 🟡 | Performance | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-11 | CI pipeline (`dotnet build`/`test` + vulnerable-package scan) in `.github/workflows` (A-22) | 🟠 | Maintainability | S | ✅ Done | Juan Mesa | — | ✅ |
| [ ] | F-12 | `.editorconfig` + analyzers + `TreatWarningsAsErrors` in CI (A-22) | 🟡 | Maintainability | S | ⬜ Open | — | — | ⬜ |
| [x] | F-13 | Override sync `SaveChanges` (or an auditing `ISaveChangesInterceptor`) (A-12) | 🟢 *(was 🟡)* | Data | S | ✅ Done | Juan Mesa | — | ✅ |
| [ ] | F-14 | Fix `ReservationService.GetAllAsync` (`Include`s / projection) (A-13) — *superseded by F-38, which removed `GetAllAsync`* | 🟡 | Data | S | ❌ Won't do | Juan Mesa | — | ✅ |
| [ ] | F-15 | Replace reflection DI with Scrutor or explicit registration; drop the thin repository surface (A-25, A-26) | 🟢 | Patterns/DI | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-16 | Persistence → Domain only; move the constant/helper to Domain/SharedKernel; unit of work for multi-step writes (A-11, A-17, A-23) | 🟡 | Architecture | M | ⬜ Open | — | — | ⬜ |
| [x] | F-17 | Restrict CORS to per-environment origins + `UseHsts` + security headers (A-19, A-24) | 🟢 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-18 | Put every service behind `IServiceBase` (or remove it) (A-18) | 🟡 | Architecture | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-19 | `IOptions<T>` instead of `IOptionsSnapshot<T>` for stable config (A-21) | 🟢 *(was 🟡)* | DI | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-20 | Fix typos (`Ruote`→`Route`) + remove dead code + align conventions (A-30, A-32, A-68) | 🟢 | Quality | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-21 | `CancellationToken` on Create/Update/Delete (services + endpoints) | 🟡 | Async | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-22 | Domain exceptions + ProblemDetails, consistent status codes and `Location`; remove the empty `Exceptions` folder (A-31) | 🟢 | Errors/API | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-23 | Soft delete (`IsActive = false`) where the business requires it (A-27) | 🟢 | Data | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-24 | Remove test credentials from version control (A-28) | 🟢 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-25 | `GenerateDocumentationFile` + `IncludeXmlComments` + versioned API explorer in Swagger (A-34) | 🟢 | Maintainability | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-26 | Reorganize the repo into `src/` `tests/` `docs/` `database/` with a `CLAUDE.md` per area, glossary and `.claude/` | 🟢 | Maintainability | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-27 | Require auth on `GET schedules/{id}/reservations` (A-35) | 🟠 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-28 | Load `Schedule` in `ReservationService.DeleteAsync` so cancellation stops failing (A-36) | 🔴 *(was 🟠)* | Data/Errors | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-29 | Count Capacity per Trip (Schedule + travel date) and make the check atomic (A-37) | 🟠 | Business rules | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-30 | 404 instead of 500 for an unknown `ScheduleId` in `CreateAsync` (A-38) | 🟡 | Errors/API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-31 | Support Schedules that cross midnight (`Duration` and "trip passed") (A-39) | 🟡 | Business rules | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-32 | Reject Routes whose origin and destination are the same Point (A-40) | 🟢 | Business rules | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-33 | Stamp `UpdateUser` on every modification (A-41) | 🟠 | Data | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-34 | Pin patched versions of the vulnerable transitive packages (A-42) | 🟠 | NuGet/Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-35 | Reject inactive users at login; apply `RoleId` on user update through `UserManager`; refresh the security stamp (A-43) | 🟠 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-36 | Apply the Trip Capacity and travel-date rules on Reservation update (A-44) | 🟠 | Business rules | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-37 | Return the travel date in `ReservationDto` and its projections (A-45) | 🟠 | API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-38 | Page (by Trip/date) or remove `GET reservations/all`; stop caching Reservations (A-46) | 🟠 | Performance/API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-39 | Fix the Route destination search (right criteria, separate search groups) (A-47) | 🟡 | Quality/Data | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-40 | Build paged and count specifications from one shared filter (A-48) | 🟡 | Patterns | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-41 | Detail specifications with `Include`s for get-by-id and create responses (A-49) | 🟡 | Data/API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-42 | Unique indexes backing the duplicate checks (A-50) | 🟡 | Data | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-43 | Exact `Guid` lookup for Passenger details; move the document number out of the URL path (A-51) | 🟡 | Security/API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-44 | Stronger password policy in Identity options, single source shared with the validators (A-52) | 🟡 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-45 | Rate-limit login; uniform response for unknown and locked accounts (A-53) | 🟡 | Security | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-46 | Role-based authorization on Reservations (`Administrador`, `Operario`); reject unknown roles (A-54) | 🟡 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-47 | Load User Secrets only in Development; validate settings at startup (`ValidateOnStart`) (A-55) | 🟡 | Config/DI | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-48 | `EnableRetryOnFailure` + non-zero exit code on fatal startup (A-56) | 🟡 | Resilience | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-49 | Map database errors to 409/503/500 and handle cancellations (A-57) | 🟡 | Errors/API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-50 | Align validators with the database columns; clamp `PageNumber` (A-58) | 🟡 | API/Data | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-51 | Consistent auth error bodies (camelCase, a real 403) (A-59) | 🟡 | API | S | ✅ Done | Juan Mesa | — | ✅ |
| [ ] | F-52 | Decouple Application and Persistence from HTTP; `AddIdentityCore` instead of `AddDefaultIdentity` (A-60) | 🟡 | Architecture | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-53 | Consistent time handling: injected clock, UTC storage, cached Colombia time zone, rename `ToPacificStandardTime` (A-61) | 🟡 | Quality/Data | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-54 | Correlation id in responses and logs + tracing (A-62) | 🟡 | Observability | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-55 | Close the test gaps: per-controller component tests with a 401/403 role matrix, JWT round trip, validator tests, specifications on a relational provider (A-04, A-63) | 🟡 | Testing | L | ⬜ Open | — | — | ⬜ |
| [ ] | F-56 | Truthful nullable annotations (nullable navigations, non-nullable `IsActive`) (A-64) | 🟡 | Quality | M | ⬜ Open | — | — | ⬜ |
| [x] | F-57 | Hide `/health` details from anonymous callers; split liveness and readiness (A-65) | 🟡 | Security/Observability | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-58 | Specification query hygiene: conditional filters, no `ToLower` on columns, escaped `LIKE` input (A-67) | 🟢 | Data/Performance | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-59 | Repository hygiene: LICENSE placeholder, `.gitignore` IDE/env entries (A-69) | 🟢 | Maintainability | S | ✅ Done | Juan Mesa | — | ✅ |

### 7.3 Progress summary

| Severity | Total | ⬜ Open | 🟦 In progress | ✅ Done | ❌ Won't do |
|---|:--:|:--:|:--:|:--:|:--:|
| 🔴 Critical | 2 | 0 | 0 | 2 | 0 |
| 🟠 High | 13 | 0 | 0 | 13 | 0 |
| 🟡 Medium | 31 | 7 | 0 | 23 | 1 |
| 🟢 Low | 13 | 1 | 0 | 12 | 0 |
| **Total** | **59** | **8** | **0** | **50** | **1** |
| **% Completed** (Done ÷ Total − Won't do) | | | | **86%** | |

---

## 8. Development process recommendations

- **Mandatory CI gate.** On every push and PR run:
  - `dotnet build`;
  - `dotnet test` with coverage (coverlet is already referenced);
  - `dotnet list package --vulnerable --include-transitive`, failing on High.

  A gate like this would have flagged A-42 and stops regressions on the fixed items.
- **Runtime policy.** Target only supported LTS runtimes. Track the support dates and plan each upgrade before end of support: .NET 10 is supported until November 2028.
- **Quality as part of the build.** `.editorconfig`, `EnableNETAnalyzers`/`AnalysisMode`, `TreatWarningsAsErrors` and nullable warnings as errors. Use `NetArchTest` to pin the layer rules (A-17, A-23, A-60).
- **Test against a real relational database where data access matters.** SQL Server via Testcontainers catches missing `Include`s, invalid `OFFSET`s, unique-constraint violations and collation effects, none of which InMemory can see. Keep InMemory for the fast HTTP tests.
- **Make test data mirror production loading.** An Object Mother that pre-loads navigations hid A-36 for months.
- **Enforce business rules in the database.** Unique indexes and transactional checks for anything that must never be duplicated or oversold (A-37, A-44, A-50).
- **Validate configuration at startup** (`ValidateOnStart`) and exit non-zero on failure, so misconfiguration never becomes a runtime 500.
- **Keep this audit alive.** Re-assess after each major plan milestone. Keep finding and plan IDs stable, so commits ("Implements F-xx") stay traceable; the `implement-plan-item` skill records each item.

---

## 9. Conclusion

GoTransport.Api has moved from "well structured but unobservable and untested" to "well structured, observable and reproducibly tested". The June critical issue and the brute-force, observability and credential problems are closed.

The re-assessment shows where the remaining distance to production lies:

- **Business-rule correctness in the Reservation flow:** cancellation, Capacity per Trip, updates.
- **Personal-data handling.**
- **A supported, patched runtime.**

The recommended next step is the new quick wins (QW-6 … QW-11, all small), then F-06 to `net10.0` together with F-11/F-34, so that every later change ships through a CI gate on a supported runtime.

> **Reliability note:**
> - **Baseline (June):** the two findings first reported as 🔴 were verified. The `IMemoryCache` one was disproved by running real `net6` code; the anonymous-writes one stood (A-01).
> - **This re-assessment:**
>   - Every finding re-graded or added at 🔴/🟠 was checked by hand against the code.
>   - The vulnerability list comes from the NuGet audit tool.
>   - The .NET support dates come from Microsoft's announcements.
>   - Items marked *(inferred)* or *(subjective)* are labelled as such.
