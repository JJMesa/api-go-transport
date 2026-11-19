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
  2. 🟠 **Known-vulnerable runtime and packages:** `net6.0` is out of support, and so are .NET 8 and 9 since 2026-11-10. High-severity advisories affect transitive packages (A-06, A-42 → F-06, F-34).
  3. 🟠 **Personal data and access control:**
     - The Passenger list of any Schedule is anonymous (A-35).
     - Deactivated users keep signing in (A-43).
     - Passenger document numbers land in request logs (A-51).

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

- **A-06 — Runtime out of support.** All 7 projects target `net6.0` (EOL 2024-11-12). The planned target, `net8.0`, also lost support on 2026-11-10. Retarget to **`net10.0`** (LTS). → **F-06**
- **A-04 — Thin test coverage** *(partial)*.
  - Unit tests cover 5 of 14 service classes; component tests cover 2 of 12 controllers (`CitiesControllerTests`, and `ReservationsControllerTests` for delete only).
  - Detailed test gaps are in A-63. → **F-55**
- **A-35 — Anonymous endpoint returns Passenger names and document numbers.**
  - **Evidence:** `src/GoTransport.Api/Controllers/v1/ReservationsController.cs:36` puts `[AllowAnonymous]` on `GET api/v1/schedules/{scheduleId}/reservations`, which returns `PassengerFirstName`, `PassengerLastName` and `PassengerIdentification`. This is personal data under Colombia's Ley 1581 de 2012.
  - **Mitigation today:** Schedule ids are GUIDs and cannot be listed anonymously. → **F-27**
- **A-37 — Capacity is counted per Schedule, not per Trip, and the check is not atomic.**
  - **Evidence:** `ReservationService.cs:68` counts with `ReservationSpecification.cs:28`, which filters by `ScheduleId` only.
  - Because a Schedule recurs daily, it becomes "full" for ever after `Capacity` Reservations on any dates.
  - The count-then-insert also runs with no transaction or lock, so two concurrent bookings can both take the last place. → **F-29**
- **A-41 — "Last modified by" never changes** *(new)*.
  - **Evidence:** `src/GoTransport.Persistence/Contexts/ApplicationDbContext.cs:48` stamps `Modified` entries with `UpdateUser ??= username`. A row loaded from the database already holds a value (NOT NULL), so the assignment never runs.
  - **Impact:** every audited row keeps its creator as last modifier.
  - **Fix:** `entry.Entity.UpdateUser = username;` → **F-33**
- **A-42 — Vulnerable packages in the dependency closure** *(new)*.
  - **Evidence** (`dotnet list package --vulnerable --include-transitive`):

    | Severity | Package | Advisory | Projects |
    |---|---|---|---|
    | High | System.Text.Json 7.0.0 | GHSA-hh2w-p6rv-4g7w | Api |
    | High | System.Formats.Asn1 5.0.0 | GHSA-447r-wph3-92pm | Api, tests |
    | High | Microsoft.Extensions.Caching.Memory 6.0.1 | GHSA-qj66-m88j-hmgj | Application, Domain, Test.Utilities |
    | Moderate | Azure.Identity 1.10.3 | GHSA-wvxc-855f-jvrv | Api |
    | Low | Microsoft.Identity.Client 4.56.0 | GHSA-x674-v45j-fwxw | Api |
    | High | System.Net.Http 4.3.0 | GHSA-7jgj-8wvc-jh57 | test projects only |
    | High | System.Text.RegularExpressions 4.3.0 | GHSA-cmhx-cq75-c4mj | test projects only |

  - **Recommendation:** pin patched versions directly now, and add the scan to CI. The F-06 upgrade removes most of them. → **F-34**
- **A-43 — Deactivated users keep signing in; role changes are ignored** *(new)*.
  - **Evidence:**
    - `AccountService.LoginAsync` (`src/GoTransport.Application/Services/AccountService.cs:37-47`) never checks `user.IsActive`.
    - `UserService.UpdateAsync` (`UserService.cs:72-84`) applies `IsActive` but ignores the required `RoleId`, saves through the generic repository, and never refreshes the security stamp.
  - **Impact:** a disabled or compromised account, including an administrator, keeps its access. → **F-35**
- **A-44 — Updating a Reservation bypasses the Trip rules** *(new)*.
  - **Evidence:** `ReservationService.cs:89` calls `ReservationMappingExtensions.ApplyTo`, which overwrites `ScheduleId` and `ReservationDate` (`ReservationMappingExtensions.cs:36-37`). It checks neither Capacity on the target Trip nor that the new travel date is in the future.
  - **Impact:** an update can move a Passenger onto a full Trip or a past date. → **F-36**
- **A-45 — Reservation responses omit the travel date** *(new)*.
  - **Evidence:** `src/GoTransport.Application/Dtos/Reservation/ReservationDto.cs` has no `ReservationDate`, although create and update accept it.
  - **Impact:** no read endpoint can tell a Passenger which day the Trip is. → **F-37**
- **A-46 — `GET reservations/all` is unbounded and cached** *(new)*.
  - **Evidence:**
    - `ReservationsController.cs:28` exposes the endpoint.
    - `ReservationService.cs:32-41` loads every active Reservation ever made, with no paging, and stores the list in `IMemoryCache`, which has no size limit.
  - **Who can call it:** any signed-in user, whatever their role.
  - **Impact:** memory and latency grow with booking history, and personal data sits in the cache. → **F-38**

#### 🟡 Medium

- **A-08 — Two different JWT keys.**
  - Validation uses `new HMACSHA512(secret).Key` (`src/GoTransport.Api/Extensions/ServiceExtensions.cs:35`); signing uses the raw secret bytes with HS256 (`AccountService.cs:71`).
  - They diverge for secrets over 128 bytes, and the `HMACSHA512` instance is never disposed.
  - A custom `LifetimeValidator` (`ServiceExtensions.cs:46`) also removes all clock skew. → **F-07**
- **A-09 — One role per token.** `AccountService.cs:96` uses `roleList.First()`. → **F-08**
- **A-10 — No `AsNoTracking`** on entity reads (0 occurrences in `src/`). → **F-09**
- **A-11 — No unit of work.** Each repository call saves on its own.
  - Concrete cases: `UserService.cs:55-59` and `AdminBootstrapService` create the user and assign the role in separate saves.
  - **Impact:** a failure in between leaves a user with no role, and the bootstrap then fails on every restart. → **F-16**
- **A-13 — `ReservationService.GetAllAsync` loads no navigations** *(partial)*. The `NullReferenceException` is gone (mappings are null-safe), but `Schedule` and `IdentificationType` come back `null` and are cached that way (`ReservationService.cs:34-38`). → **F-14**
- **A-16 — The cache is never invalidated.** `CacheService.Remove` (`src/GoTransport.Application/Services/CacheService.cs:40`) has no callers. → **F-10**
- **A-17 — Persistence depends on Application** for `IRepository<T>`, settings, `Constants` and `ToPacificStandardTime()`. → **F-16**
- **A-18 — `IServiceBase` covers only 5 of 14 service interfaces** (Department, Point, Reservation, Route, Vehicle). CRUD-like City, Schedule and User do not use it. → **F-18**
- **A-19 — No HSTS or security headers.** There is `UseHttpsRedirection` (`Program.cs:62`) but no `UseHsts`. → **F-17**
- **A-22 — No `.editorconfig`, analyzers or CI** (`.github/workflows` is empty). → **F-11, F-12**
- **A-23 — Domain depends on ASP.NET Core Identity**, which pulls EF Core into Domain. → **F-16**
- **A-38 — An unknown `ScheduleId` gives 500 instead of 404.** `vehicle!.Capacity` at `ReservationService.cs:70`. → **F-30**
- **A-39 — Overnight Schedules.** `Duration = Arrival − Departure` turns negative (`ScheduleService.cs:40,56`), and the "trip passed" check uses `ReservationDate + ArrivalTime` (`ReservationService.cs:110`). → **F-31**
- **A-47 — The Route destination search uses the origin text** *(new)*.
  - `PagedRouteSpecification.cs:21-24` and `RouteSpecification.cs:30-33` search the destination with `OriginPointSearchCriteria`. All four `Search` calls share search group 1, so they are OR-ed.
  - **Impact:** a destination-only search on the anonymous route finder becomes `LIKE '%%'`. → **F-39**
- **A-48 — Paged and count specifications are copy-pasted and have drifted** *(new)*.
  - `PagedCitySpecification` filters by `DepartmentId`; `CitySpecification(CityParameters)` does not. `CityService.GetAsync` therefore returns wrong `TotalItems`/`TotalPages` when filtering by Department.
  - All six paged/count pairs duplicate their filters. → **F-40**
- **A-49 — The same DTO comes back in different shapes** *(new)*.
  - Get-by-id uses `GetByIdAsync` with no `Include`, so navigations such as Points, Manufacturer and Schedule are `null` on detail endpoints but filled on paged ones.
  - Evidence: `RouteService.cs:51`, `VehicleService.cs:50`, `ScheduleService.cs:32`, `ReservationService.cs:60`. → **F-41**
- **A-50 — Uniqueness rules have no database constraint** *(new)*.
  - The migration has only two unique indexes, both on Identity tables (`APP_ROLE`, `APP_USER`).
  - Duplicate checks for Department and City description, the Route origin–destination pair and the license plate are check-then-insert in code, so concurrent requests can insert duplicates. → **F-42**
- **A-51 — Passenger lookup by document number** *(new)*. Three problems:
  - **Type mismatch:** `GET reservations/{id:guid}/person/{passengerIdentification}/details` binds `id` as `string` (`ReservationsController.cs:47`).
  - **Full scan:** the lookup compares `ReservationId.ToString().StartsWith(id)` (`ReservationSpecification.cs:21`), so it cannot use an index.
  - **Personal data in logs:** the national ID travels in the URL path, so Serilog request logging and the error middleware write it to log files kept for 7 days. → **F-43**
- **A-52 — Weak password policy** *(new)*. Identity password options are never set (`src/GoTransport.Persistence/RegisterLayerExtensions.cs:31`), and `UserCreationValidator` mirrors a 6-character minimum. That includes the bootstrap administrator. → **F-44**
- **A-53 — Login enumeration and lockout abuse** *(new)*.
  - An unknown email returns 400 at once; a known one returns 429 after 5 failures (`AccountService.cs:37-43`).
  - There is no rate limiting, so anyone can confirm accounts and keep the administrator locked out with 5 requests every 15 minutes. → **F-45**
- **A-54 — Reservations need a login but no role** *(new)*.
  - `ReservationsController` has a plain `[Authorize]` (`:14`), and `Roles.Operator` is never used (`src/GoTransport.Application/Commons/Roles.cs:6`).
  - `RoleService.AssignRolesToUser` succeeds with an unknown `roleId` (`RoleService.cs:44-55`), so a user can end up with no role and still use Reservations. → **F-46**
- **A-55 — Configuration precedence and validation** *(new)*.
  - `Program.cs:23` adds User Secrets last in **every** environment, so they override environment variables. The component-test factory has to blank `AdminBootstrap` to cope.
  - Settings are bound without validation: a zero cache TTL or a short JWT secret fails at runtime, not at startup. → **F-47**
- **A-56 — Startup resilience** *(new)*.
  - `UseSqlServer` has no `EnableRetryOnFailure` (`src/GoTransport.Persistence/RegisterLayerExtensions.cs:19`).
  - The fatal `catch` in `Program.cs:76` only logs, so a failed start (including the admin bootstrap) exits with code 0 and orchestrators see a clean shutdown. → **F-48**
- **A-57 — Database errors map to the wrong status** *(new)*.
  - `ErrorHandlerMiddleware` turns every `SqlException` inside a `DbUpdateException` into **400**. Codes other than 547/2601 get an "internal server error" text (`src/GoTransport.Api/Middlewares/ErrorHandlerMiddleware.cs:72-86`).
  - `TaskCanceledException` becomes 400 (`:51`). → **F-49**
- **A-58 — Validators disagree with the database; paging input is not range-checked** *(new)*.
  - City names allow 128 characters (`CityCreationValidator.cs:13`) for a `varchar(32)` column (`CityConfiguration.cs:24-25`).
  - Department has no validators at all.
  - Vehicle `Capacity` only has `NotEmpty`, so negative values pass.
  - The `PageNumber` setter is not clamped (`PaginationParameters.cs:9`); only the constructor clamps. *(inferred)* `pageNumber=0` produces a negative `OFFSET`, so SQL Server returns 500. → **F-50**
- **A-59 — Auth error bodies differ from the rest of the API** *(new)*. The JWT events serialize the envelope in PascalCase while everything else is camelCase, and `OnForbidden` sends a 403 status with a body built by `Unauthorized(...)` (`ServiceExtensions.cs:108-113`). → **F-51**
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
  - No 401/403 test anywhere: `TestAuthHandler` can only produce an `Administrador`, and every component test authenticates.
  - No JWT issue → validate round trip, which would catch A-08.
  - 0 of 14 validators are tested.
  - No specification runs against a relational provider, and the Reservation Object Mother hides A-36. → **F-55**
- **A-64 — Nullable annotations are defeated** *(new)*. 7 `?.ToDto()!` and 19 `IsActive!.Value` in Application, plus `Errors = null!` in `JsonResponse`. The root cause is the tri-state `bool? IsActive` on entities. → **F-56**
- **A-65 — Anonymous `/health` exposes error text** *(new)*. `src/GoTransport.Api/Extensions/HealthCheckExtensions.cs:58` returns `Exception?.Message`; SQL connection failures can reveal server or login names. → **F-57**
- **A-66 — Package and test-stack drift** *(new)*.
  - ASP.NET packages sit at 6.0.27/6.0.28.
  - Extensions packages mix 6.x and 7.0.0.
  - Unit tests use xunit 2.4.2 / coverlet 3.2.0, while component tests use 2.6.1 / 6.0.0.
  - Unused packages: `Identity.UI`, EF InMemory in the unit tests, and UserSecrets in the component tests. → **F-06**

#### 🟢 Low

- **A-12 — Sync `SaveChanges` is not overridden** *(baseline 🟡 → 🟢: nothing in `src/` calls it, so the gap is latent)*. → **F-13**
- **A-21 — `IOptionsSnapshot<T>` for stable config** in `AccountService.cs:28` and `CacheService.cs:14` *(baseline 🟡 → 🟢: `ValidateScopes` already guards lifetimes; the cost is per-request rebinding)*. → **F-19**
- **A-24 — CORS `AllowAnyOrigin`** (`ServiceExtensions.cs:174`), mitigated because the API uses bearer tokens. → **F-17**
- **A-25 — Reflection-based DI over `AppDomain` assemblies.** It also filters with `inherit: true` but registers with `inherit: false`, so a subclass that only inherits the attribute is silently skipped (`src/GoTransport.Application/RegisterLayerExtensions.cs:39` vs `:49-56`). → **F-15**
- **A-26 — Thin generic repository** with an unused public `Context`. → **F-15**
- **A-27 — Hard deletes despite `IsActive` columns.** → **F-23**
- **A-29 — `iat` claim** written with `ToLongDateString()`, which is not a numeric date (`AccountService.cs:67`). → **F-07**
- **A-30 — Typos** *(partial: "transaporte" is gone)*. `RuoteParameters.cs`, `ruoteId`, `ruote` lambdas and `IsDuplicarePoints` remain. → **F-20**
- **A-31 — HTTP conventions.**
  - No `Location` header, a custom `Pagination` header, no ProblemDetails.
  - Creating a Reservation returns 200 while every other create returns 201 (`ReservationService.cs:75`).
  - Duplicates return 400 but "vehicle full" returns 409. → **F-22**
- **A-32 — Dead code.** Includes:
  - the empty `Exceptions/` folder;
  - the `KeyNotFoundException` branch;
  - `ValidateAndThrowArgumentException`;
  - unused specifications and `ErrorMessages`;
  - a singleton `TokenValidationParameters` that is never resolved. → **F-20**
- **A-33 — Conservative C#** (no records). Primary constructors need C# 12, which F-06 unlocks. *(subjective)* → **F-06**
- **A-34 — XML docs not exposed in Swagger.** The versioned API explorer is also missing, so Swagger does not substitute the API version in routes. → **F-25**
- **A-40 — A Route can have the same origin and destination Point.** → **F-32**
- **A-67 — Specification query hygiene** *(new)*.
  - `col == p || p == null` filters (`CitySpecification`, `ReservationSpecification.cs:28`).
  - `ToLower()` on columns in the duplication specs, although the collation is already case-insensitive.
  - Raw user input inside `LIKE` patterns, so `%` and `_` act as wildcards. → **F-58**
- **A-68 — Inconsistent conventions** *(new)*.
  - `CityService` is `public` while the other services are `internal`.
  - `Metadata` takes the same parameters in two different orders.
  - Services mutate input DTOs.
  - The `Pagination`-header code is repeated in 6 controllers. → **F-20**
- **A-69 — Repository hygiene** *(new)*. `LICENSE.txt` still says `[year] [fullname]`, and `.gitignore` lacks `.idea/`, `.vscode/` and `.env`. → **F-59**

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
| A-36 | 🔴 Every Reservation cancellation failed with a 500 | F-28 / QW-6 (`[Fix] Load the Schedule when cancelling a Reservation`). `DeleteAsync` now loads the Reservation through `ReservationSpecification`, which includes `Schedule`. Component tests over the real pipeline cover an upcoming Trip (204), a past travel date (400) and an unknown id (404). |

### 5.3 No longer applicable

None. Every baseline finding is either resolved (§5.2) or still present (§5.1).

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
  // Current (PagedRouteSpecification) — destination searched with the origin text, all in group 1
  Query.Search(r => $"{r.DestinationPoint.Detail}", $"%{parameters.OriginPointSearchCriteria}%");
  // Recommended — right criteria, separate group so origin AND destination combine
  Query.Search(r => $"{r.DestinationPoint.Detail}", $"%{parameters.DestinationPointSearchCriteria}%", searchGroup: 2);
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
  // Current (ApplicationDbContext.SaveChangesAsync, Modified branch)
  entry.Entity.UpdateUser ??= username;
  // Recommended
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
  // Current (AccountService.LoginAsync)
  var user = await _userManager.FindByEmailAsync(userLogin.Email);
  if (user is null) return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);
  // Recommended — inactive accounts are treated like unknown ones
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
| [ ] | QW-7 | Require auth on `GET schedules/{id}/reservations` (F-27) | 🟠 | ⬜ Open | — | — | ⬜ |
| [ ] | QW-8 | Always stamp `UpdateUser` on modified entities (F-33) | 🟠 | ⬜ Open | — | — | ⬜ |
| [ ] | QW-9 | Pin patched versions of the vulnerable packages (F-34) | 🟠 | ⬜ Open | — | — | ⬜ |
| [ ] | QW-10 | Reject inactive users at login (F-35) | 🟠 | ⬜ Open | — | — | ⬜ |
| [ ] | QW-11 | Return the travel date in `ReservationDto` (F-37) | 🟠 | ⬜ Open | — | — | ⬜ |

### 7.2 Full prioritized plan

| ✔ | ID | Action | Sev. | Dimension | Effort | Status | Owner | PR / Commit | Verified |
|:--:|:--:|--------|:--:|---|:--:|:--:|---|---|:--:|
| [x] | F-01 | Require auth on Reservations POST/PUT | 🔴 | Security/API | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-02 | Explicit `AddMemoryCache()` + `ValidateOnBuild` | 🟡 | DI/Performance | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-03 | Lockout + `CheckPasswordSignInAsync` | 🟠 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-04 | Structured logging (`ILogger`/Serilog) + health checks | 🟠 | Observability | M | ✅ Done | Juan Mesa | — | ✅ |
| [x] | F-05 | Isolate the database in component tests + broaden unit coverage | 🟠 | Testing | M | ✅ Done | Juan Mesa | — | ✅ |
| [ ] | F-06 | Migrate to `net10.0` (LTS; `net8.0` also lost support on 2026-11-10) + `Directory.Build.props` + CPM; align package and test-stack versions, drop unused packages (A-06, A-33, A-66) | 🟠 | NuGet/Maintainability | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-07 | One `SymmetricSecurityKey` for signing and validation; built-in lifetime validation with a small `ClockSkew`; numeric `iat` (A-08, A-29) | 🟡 | Security | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-08 | Emit every role as a claim (multi-role) (A-09) | 🟡 | Security | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-09 | `AsNoTracking` in read specifications + `Include`s in `GetAll` (A-10) | 🟡 | Data/Performance | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-10 | Invalidate the cache (`Remove`) on writes + raise the TTL (A-16) | 🟡 | Performance | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-11 | CI pipeline (`dotnet build`/`test` + vulnerable-package scan) in `.github/workflows` (A-22) | 🟠 | Maintainability | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-12 | `.editorconfig` + analyzers + `TreatWarningsAsErrors` in CI (A-22) | 🟡 | Maintainability | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-13 | Override sync `SaveChanges` (or an auditing `ISaveChangesInterceptor`) (A-12) | 🟢 *(was 🟡)* | Data | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-14 | Fix `ReservationService.GetAllAsync` (`Include`s / projection) (A-13) | 🟡 | Data | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-15 | Replace reflection DI with Scrutor or explicit registration; drop the thin repository surface (A-25, A-26) | 🟢 | Patterns/DI | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-16 | Persistence → Domain only; move the constant/helper to Domain/SharedKernel; unit of work for multi-step writes (A-11, A-17, A-23) | 🟡 | Architecture | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-17 | Restrict CORS to per-environment origins + `UseHsts` + security headers (A-19, A-24) | 🟢 | Security | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-18 | Put every service behind `IServiceBase` (or remove it) (A-18) | 🟡 | Architecture | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-19 | `IOptions<T>` instead of `IOptionsSnapshot<T>` for stable config (A-21) | 🟢 *(was 🟡)* | DI | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-20 | Fix typos (`Ruote`→`Route`) + remove dead code + align conventions (A-30, A-32, A-68) | 🟢 | Quality | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-21 | `CancellationToken` on Create/Update/Delete (services + endpoints) | 🟡 | Async | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-22 | Domain exceptions + ProblemDetails, consistent status codes and `Location`; remove the empty `Exceptions` folder (A-31) | 🟢 | Errors/API | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-23 | Soft delete (`IsActive = false`) where the business requires it (A-27) | 🟢 | Data | M | ⬜ Open | — | — | ⬜ |
| [x] | F-24 | Remove test credentials from version control (A-28) | 🟢 | Security | S | ✅ Done | Juan Mesa | — | ✅ |
| [ ] | F-25 | `GenerateDocumentationFile` + `IncludeXmlComments` + versioned API explorer in Swagger (A-34) | 🟢 | Maintainability | S | ⬜ Open | — | — | ⬜ |
| [x] | F-26 | Reorganize the repo into `src/` `tests/` `docs/` `database/` with a `CLAUDE.md` per area, glossary and `.claude/` | 🟢 | Maintainability | M | ✅ Done | Juan Mesa | — | ✅ |
| [ ] | F-27 | Require auth on `GET schedules/{id}/reservations` (A-35) | 🟠 | Security | S | ⬜ Open | — | — | ⬜ |
| [x] | F-28 | Load `Schedule` in `ReservationService.DeleteAsync` so cancellation stops failing (A-36) | 🔴 *(was 🟠)* | Data/Errors | S | ✅ Done | Juan Mesa | — | ✅ |
| [ ] | F-29 | Count Capacity per Trip (Schedule + travel date) and make the check atomic (A-37) | 🟠 | Business rules | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-30 | 404 instead of 500 for an unknown `ScheduleId` in `CreateAsync` (A-38) | 🟡 | Errors/API | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-31 | Support Schedules that cross midnight (`Duration` and "trip passed") (A-39) | 🟡 | Business rules | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-32 | Reject Routes whose origin and destination are the same Point (A-40) | 🟢 | Business rules | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-33 | Stamp `UpdateUser` on every modification (A-41) | 🟠 | Data | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-34 | Pin patched versions of the vulnerable transitive packages (A-42) | 🟠 | NuGet/Security | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-35 | Reject inactive users at login; apply `RoleId` on user update through `UserManager`; refresh the security stamp (A-43) | 🟠 | Security | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-36 | Apply the Trip Capacity and travel-date rules on Reservation update (A-44) | 🟠 | Business rules | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-37 | Return the travel date in `ReservationDto` and its projections (A-45) | 🟠 | API | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-38 | Page (by Trip/date) or remove `GET reservations/all`; stop caching Reservations (A-46) | 🟠 | Performance/API | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-39 | Fix the Route destination search (right criteria, separate search groups) (A-47) | 🟡 | Quality/Data | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-40 | Build paged and count specifications from one shared filter (A-48) | 🟡 | Patterns | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-41 | Detail specifications with `Include`s for get-by-id and create responses (A-49) | 🟡 | Data/API | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-42 | Unique indexes backing the duplicate checks (A-50) | 🟡 | Data | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-43 | Exact `Guid` lookup for Passenger details; move the document number out of the URL path (A-51) | 🟡 | Security/API | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-44 | Stronger password policy in Identity options, single source shared with the validators (A-52) | 🟡 | Security | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-45 | Rate-limit login; uniform response for unknown and locked accounts (A-53) | 🟡 | Security | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-46 | Role-based authorization on Reservations (`Administrador`, `Operario`); reject unknown roles (A-54) | 🟡 | Security | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-47 | Load User Secrets only in Development; validate settings at startup (`ValidateOnStart`) (A-55) | 🟡 | Config/DI | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-48 | `EnableRetryOnFailure` + non-zero exit code on fatal startup (A-56) | 🟡 | Resilience | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-49 | Map database errors to 409/503/500 and handle cancellations (A-57) | 🟡 | Errors/API | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-50 | Align validators with the database columns; clamp `PageNumber` (A-58) | 🟡 | API/Data | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-51 | Consistent auth error bodies (camelCase, a real 403) (A-59) | 🟡 | API | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-52 | Decouple Application and Persistence from HTTP; `AddIdentityCore` instead of `AddDefaultIdentity` (A-60) | 🟡 | Architecture | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-53 | Consistent time handling: injected clock, UTC storage, cached Colombia time zone, rename `ToPacificStandardTime` (A-61) | 🟡 | Quality/Data | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-54 | Correlation id in responses and logs + tracing (A-62) | 🟡 | Observability | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-55 | Close the test gaps: per-controller component tests with a 401/403 role matrix, JWT round trip, validator tests, specifications on a relational provider (A-04, A-63) | 🟡 | Testing | L | ⬜ Open | — | — | ⬜ |
| [ ] | F-56 | Truthful nullable annotations (nullable navigations, non-nullable `IsActive`) (A-64) | 🟡 | Quality | M | ⬜ Open | — | — | ⬜ |
| [ ] | F-57 | Hide `/health` details from anonymous callers; split liveness and readiness (A-65) | 🟡 | Security/Observability | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-58 | Specification query hygiene: conditional filters, no `ToLower` on columns, escaped `LIKE` input (A-67) | 🟢 | Data/Performance | S | ⬜ Open | — | — | ⬜ |
| [ ] | F-59 | Repository hygiene: LICENSE placeholder, `.gitignore` IDE/env entries (A-69) | 🟢 | Maintainability | S | ⬜ Open | — | — | ⬜ |

### 7.3 Progress summary

| Severity | Total | ⬜ Open | 🟦 In progress | ✅ Done |
|---|:--:|:--:|:--:|:--:|
| 🔴 Critical | 2 | 0 | 0 | 2 |
| 🟠 High | 13 | 10 | 0 | 3 |
| 🟡 Medium | 31 | 30 | 0 | 1 |
| 🟢 Low | 13 | 11 | 0 | 2 |
| **Total** | **59** | **51** | **0** | **8** |
| **% Completed** | | | | **14%** |

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
