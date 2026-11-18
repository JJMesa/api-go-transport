# GoTransport.Api.Component.Tests

**Responsibility:** exercises the real HTTP pipeline end to end — routing, auth, validation, services and EF Core — with no external dependencies.

## How the host is built (`Infrastructure/CustomWebApplicationFactory`)

- Runs in the `ComponentTesting` environment; `src/GoTransport.Api/appsettings.ComponentTesting.json` supplies dummy JWT/caching values.
- The SQL Server `DbContext` is swapped for an EF Core InMemory database, unique per factory.
- JWT auth is replaced by `Infrastructure/TestAuthHandler`: a request is authenticated as an `Administrador` only if it carries the `X-Test-Auth` header. Call `await AddAuthorization()` in a test to set it; omit it to test `401`s.

## Conventions

- Test classes derive from `ComponentTest` (`IClassFixture<CustomWebApplicationFactory>`), whose constructor drops and re-seeds the database from `Infrastructure/DatabaseSeeder` before **each** test.
- Add new seed rows in `DatabaseSeeder`; keep their ids consistent with `Utils.DefaultId`.
- Build URLs from the `ApiPaths` constants.

## Gotchas

- InMemory has neither FK enforcement nor the SQL Server collation: string filters are case- and accent-exact, and the 547/2601 error translation in `ErrorHandlerMiddleware` can't be exercised. Write tests whose outcome holds on both providers.
- The host validates DI at startup (`ValidateOnBuild`): one broken registration fails every test here at once — read the host exception, not the individual assertions.
- The Api project grants `InternalsVisibleTo` to this project.
