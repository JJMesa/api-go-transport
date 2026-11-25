# tests/ — automated tests

**Responsibility:** fast unit tests of the Application services, component tests of the full HTTP pipeline, and the helpers both share. All tests are xUnit.

## Contents

| Project | Purpose |
|---|---|
| `GoTransport.Api.Unit.Tests` | Application services in isolation with mocked repositories/cache |
| `GoTransport.Api.Component.Tests` | Real pipeline (routing, auth, validation, services, EF) over EF Core InMemory; specifications also on SQLite; real JWT round trip |
| `GoTransport.Api.Test.Utilities` | Shared helpers (below) |

## Shared utilities (`GoTransport.Api.Test.Utilities`)

- `Commons/ApiPaths` — route constants; `Commons/Utils.DefaultId` — the id the seed data and tests agree on.
- `Builders/ResponseBuilder` — test-side response builder.
- `Mothers/*BuilderMother` — test-data factories (Account, City, Department, Reservation). Add one when a new resource needs test data.

## Conventions

- One test class per service or controller under `TestCases/`, named `<Service>Tests` / `<Controller>Tests`.
- Test methods read `Method_ShouldOutcome[ForCondition]`, e.g. `GetByIdAsync_ShouldReturnCityForValidId`.
- Test projects reference production code through `..\..\src\<Project>\<Project>.csproj`.
- Both suites run without SQL Server, seed data or secrets; keep it that way.
