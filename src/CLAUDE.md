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

One-way: **Api → Persistence → Application → Domain**, and Api also references Application directly. Domain depends on nothing but ASP.NET Core Identity.

Persistence reaches up into Application for `IRepository<T>` (declared in Application, implemented in Persistence), the settings classes and `Constants.ApplicationUser`. Plan item F-16 narrows that to Domain only; place new shared code in the lowest layer that needs it so the edge stops growing.

## Conventions

- Expected business failures travel up as `ResponseBuilder` results through every layer; exceptions are for the unexpected, and the Api middleware renders both in the same envelope.
- A change that adds a whole resource (entity → controller) touches all four projects in a fixed order: follow the `new-crud-resource` skill.

## See also

- `tests/CLAUDE.md` — how each layer is tested.
- `.claude/rules/csproj.md` — package-version rules, loaded when editing any `.csproj`.
