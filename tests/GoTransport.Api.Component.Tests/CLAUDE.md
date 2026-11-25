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
- Add new seed rows in `DatabaseSeeder`; keep their ids consistent with `Utils.DefaultId`. Seeded `Guid` ids (the Schedule and the upcoming/past Reservations) are public fields on `DatabaseSeeder`; reference them from tests instead of copying literals.
- One test class per controller (`<Controller>Tests`), plus `SwaggerDocumentTests`, which builds the OpenAPI document in-process. Prefer these tests for anything that depends on what EF actually loads: unit-test mocks hand back fully built entities and cannot catch a missing `Include`.
- Build URLs from the `ApiPaths` constants.
- To assert on state the DTOs don't expose (e.g. the audit columns), read the row back with `FindInDatabase<TEntity>(keys)` from `ComponentTest`. Signed-in requests run as `TestAuthHandler.TestUserId`; seeded rows are stamped with the `SISTEMA` system user.

## Gotchas

- InMemory has neither FK enforcement nor the SQL Server collation: string filters are case- and accent-exact, and SQL Server errors can't be raised; `ExceptionResponseMapperTests` covers their status mapping by error number. Write tests whose outcome holds on both providers.
- The host validates DI at startup (`ValidateOnBuild`): one broken registration fails every test here at once — read the host exception, not the individual assertions.
- Test classes run one at a time (`Infrastructure/TestParallelization.cs`). Every class boots its own host through `Program`, which assigns and freezes Serilog's static bootstrap logger; hosts built in parallel fail with "The logger is already frozen". Keep the assembly attribute when adding classes.
- The Api project grants `InternalsVisibleTo` to this project.
