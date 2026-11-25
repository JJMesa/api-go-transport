# src/ — production code

**Responsibility:** the API's production code, split into four layered projects (Clean Architecture). Each project has its own `CLAUDE.md` with its conventions.

## Contents

| Project | Layer responsibility |
|---|---|
| `GoTransport.Domain` | Entities and the auditing base; no business logic |
| `GoTransport.Application` | All business logic: services, DTOs, mappings, validators, specifications, response envelope |
| `GoTransport.Persistence` | EF Core: `DbContext`, entity configurations, migrations, generic repository, Identity storage |
| `GoTransport.Api` | Thin HTTP layer and composition root: controllers, middleware, configuration |

## Dependencies

**Api → Application → Domain** and **Api → Persistence → Domain**. Persistence does not reference Application: it implements the Domain contracts (`IRepository<T>`, `IUnitOfWork`), and Api composes both layers. Domain references only Identity's store types (`Microsoft.Extensions.Identity.Stores`, no EF Core) and `Ardalis.Specification`. `LayerDependencyTests` in the component tests checks these rules on the compiled assemblies; place new shared code in the lowest layer that needs it.

## Conventions

- Expected business failures travel up as `ResponseBuilder` results through every layer; exceptions are for the unexpected, and the Api middleware renders both in the same envelope.
- A change that adds a whole resource (entity → controller) touches all four projects in a fixed order: follow the `new-crud-resource` skill.

## See also

- `tests/CLAUDE.md` — how each layer is tested.
- `.claude/rules/csproj.md` — package-version rules, loaded when editing any `.csproj`.
