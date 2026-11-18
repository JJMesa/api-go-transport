---
name: new-crud-resource
description: Add a new CRUD resource end to end — entity, EF configuration and migration, DTOs, mapping, validators, specifications, service, controller and tests. Use when asked to add, create or scaffold a new entity, resource or endpoint set.
---

# New CRUD resource

Build the resource bottom-up, one layer per step, mirroring the closest existing resource: `Vehicle` is the most complete one (cached `GetAllAsync`, paged list, duplication check, validators); `City` is the best example of unit **and** component tests. Read the exemplar's file at each step before writing yours, and follow each folder's `CLAUDE.md`.

1. **Name it.** Use the `GLOSSARY.md` term. If the concept is not in the glossary, agree the term with the user and add it there before writing code. Choose `App` (data the business creates) or `Bas` (seeded reference data).
2. **Entity** — `src/GoTransport.Domain/Entities/{App,Bas}/<Entity>.cs`, extending `AuditableEntity`, with navigations to related entities.
3. **Persistence** — `src/GoTransport.Persistence/Configurations/{App,Bas}/<Entity>Configuration.cs` extending `BaseEntityConfiguration<T>` and calling `base.Configure(builder)`: `APP_`/`BAS_` table name, `UPPER_SNAKE_CASE` columns, keys and relationships. Then `dotnet ef migrations add Add<Entity> --project src/GoTransport.Persistence --startup-project src/GoTransport.Api` and read the generated `Up`/`Down`.
4. **Application** (`src/GoTransport.Application/`):
   1. `Dtos/<Entity>/` — `<Entity>Dto`, `<Entity>CreationDto`, `<Entity>UpdateDto`.
   2. `Mappings/<Entity>MappingExtensions.cs` — `ToDto`, `ToEntity`, `ApplyTo`.
   3. `Validators/<Entity>/` — `<Entity>CreationValidator`, `<Entity>UpdateValidator`.
   4. `Specifications/<Entities>/` — `<Entity>Specification`; `Paged<Entity>Specification` if listable (its `Query.Select` mirrors `ToDto`); `Duplication<Entity>Specification` for uniqueness rules. Add `Parameters/<Entity>Parameters.cs` if listable.
   5. `Interfaces/I<Entity>Service.cs` + `Services/<Entity>Service.cs` — `internal`, decorated `[Transient]`, names paired as `I{ClassName}`. For a catalog, add a `Commons/CacheKey` constant and cache `GetAllAsync`.
   6. New messages in `Commons/ErrorMessages.cs`, written in English.
5. **Api** — `src/GoTransport.Api/Controllers/v1/<Entities>Controller.cs`: `[Authorize(Roles = Roles.Administrator)]`, actions returning `StatusCode((int)response.HttpCode, response)`, `Pagination` header on the paged endpoint.
6. **Tests**:
   - `tests/GoTransport.Api.Test.Utilities` — an `ApiPaths` constant and a `<Entity>BuilderMother`.
   - Unit — `TestCases/<Entity>ServiceTests.cs`, plus the new mapping cases in `MappingExtensionsTests.cs`.
   - Component — seed rows in `Infrastructure/DatabaseSeeder.cs` and `TestCases/<Entities>ControllerTests.cs` covering success, `404`, validation `400` and unauthenticated `401`.

**Done when** `dotnet build GoTransport.sln` adds no new warnings, `dotnet test` passes in both test projects, and your final message lists the file created at every step above (or why a step was skipped, e.g. no paged list).
