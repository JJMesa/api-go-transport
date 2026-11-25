# GoTransport.Api.Component.Tests

**Responsibility:** exercises the real HTTP pipeline end to end — routing, auth, validation, services and EF Core — with no external dependencies.

## How the host is built (`Infrastructure/CustomWebApplicationFactory`)

- Runs in the `ComponentTesting` environment; `src/GoTransport.Api/appsettings.ComponentTesting.json` supplies dummy JWT/caching values.
- The SQL Server `DbContext` is swapped for an EF Core InMemory database, unique per factory. The factory removes `DbContextOptions<T>` **and** EF's `IDbContextOptionsConfiguration<T>` registrations; leaving the latter keeps SQL Server configured and fails every test with "Services for database providers ... have been registered".
- User Secrets load only in `Development`, so the developer's secrets (and their `AdminBootstrap`) never reach these tests.
- Settings are validated when the host starts: `WithWebHostBuilder(b => b.UseSetting(key, value))` on the factory is how `StartupValidationTests` proves a bad value stops startup.
- JWT auth is replaced by `Infrastructure/TestAuthHandler`: a request is authenticated only if it carries the `X-Test-Auth` header, whose value is the user's Role. `await AddAuthorization()` signs in as an `Administrador`; pass `Roles.Operator` or `TestAuthHandler.NoRole` to test role rules (`403`); omit it to test `401`s.

## Conventions

- Test classes derive from `ComponentTest` (`IClassFixture<CustomWebApplicationFactory>`), whose constructor drops and re-seeds the database from `Infrastructure/DatabaseSeeder` before **each** test.
- Add new seed rows in `DatabaseSeeder`. The ids tests rely on are public constants there — `AntioquiaDepartmentId`, `MedellinCityId`, `UnknownId` (no row uses it, for 404s), the Schedule and upcoming/past Reservation `Guid`s, and the Role ids (`Administrador` 1, `Operario` 2, as `INSERT_SEED.sql`); reference them from tests instead of copying literals.
- One test class per controller (`<Controller>Tests`), plus cross-cutting classes: `AuthorizationMatrixTests` (every endpoint × anonymous/no-Role/Operator/Administrador against the access table in `AccessOf`; a new endpoint is covered automatically, so add its access there), `JwtRoundTripTests` (real login and JWT bearer, through `JwtWebApplicationFactory`), `RelationalSpecificationTests` (specifications on in-memory SQLite), `UnitOfWorkTests` (commit, rollback, isolation level, nesting and retry of the transaction on SQLite), `SwaggerDocumentTests`, `LayerDependencyTests` and others. Prefer these tests for anything that depends on what EF actually loads: unit-test mocks hand back fully built entities and cannot catch a missing `Include`.
- Build URLs from the `Infrastructure/ApiPaths` constants.
- To assert on state the DTOs don't expose (e.g. the audit columns), read the row back with `FindInDatabase<TEntity>(keys)` from `ComponentTest`. Signed-in requests run as `TestAuthHandler.TestUserId`; seeded rows are stamped with the `SISTEMA` system user.

## Gotchas

- InMemory has neither FK enforcement nor the SQL Server collation, and it evaluates queries in memory, so an expression SQL cannot translate still passes there. String filters are case- and accent-exact, and SQL Server errors can't be raised (`ExceptionResponseMapperTests` covers their status mapping by error number). When a change touches a specification, add a case to `RelationalSpecificationTests`, which runs on SQLite. Write tests whose outcome holds on both providers.
- The host validates DI at startup (`ValidateOnBuild`): one broken registration fails every test here at once — read the host exception, not the individual assertions.
- Test classes run one at a time (`Infrastructure/TestParallelization.cs`). Every class boots its own host through `Program`, which assigns and freezes Serilog's static bootstrap logger; hosts built in parallel fail with "The logger is already frozen". Keep the assembly attribute when adding classes.
- The Api and Persistence projects grant `InternalsVisibleTo` to this project (Persistence so `UnitOfWorkTests` can build the internal `UnitOfWork`).
- For a relational database, use `Infrastructure/SqliteApplicationDbContext.CreateSeeded(connection)`: the real model minus the SQL Server details SQLite cannot create, with the same seed. SQLite does not reproduce SQL Server's `Serializable` range locks, and it raises `ReadUncommitted` to `Serializable` unless the connection uses `Cache=Shared`.
