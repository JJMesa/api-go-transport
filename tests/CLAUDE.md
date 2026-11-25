# tests/ — automated tests

**Responsibility:** fast unit tests of the Application services and component tests of the full HTTP pipeline. All tests are xUnit.

## Contents

| Project | Purpose |
|---|---|
| `GoTransport.Api.Unit.Tests` | Application services in isolation with mocked repositories/cache |
| `GoTransport.Api.Component.Tests` | Real pipeline (routing, auth, validation, services, EF) over EF Core InMemory; specifications also on SQLite; real JWT round trip |

## Test helpers

Each suite keeps its own helpers; there is no shared test project. `GoTransport.Api.Test.Utilities` was removed (F-67) because no helper in it was used by both suites in the same sense: an id that is arbitrary for a mock must match a seeded row over HTTP.

- Unit: `Mothers/*BuilderMother` — test-data factories (Account, City parameters, Department, Reservation); `Mothers/TestIds` — ids for mocked lookups; `Helpers/` — `IdentityMocks`, `InlineUnitOfWork`.
- Component: `Infrastructure/ApiPaths` — URLs; `Infrastructure/DatabaseSeeder` — the seed and the ids of its rows; `Infrastructure/SqliteApplicationDbContext` — the model on a relational database.

Put a new helper in the suite that uses it.

## Conventions

- One test class per service or controller under `TestCases/`, named `<Service>Tests` / `<Controller>Tests`.
- Test methods read `Method_ShouldOutcome[ForCondition]`, e.g. `GetByIdAsync_ShouldReturnCityForValidId`.
- Test projects reference production code through `..\..\src\<Project>\<Project>.csproj`.
- Both suites run without SQL Server, seed data or secrets; keep it that way.
