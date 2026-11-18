# GoTransport.Application

**Responsibility:** all business logic. Services receive DTOs, enforce business rules, query through specifications, and return a `JsonResponse` envelope to the Api.

## Contents

| Folder | Holds |
|---|---|
| `Services/`, `Interfaces/` | One `I<Entity>Service` / `<Entity>Service` pair per resource; `Interfaces/Base/` declares `IRepository<T>` and `IServiceBase` |
| `Dtos/<Entity>/`, `Mappings/` | `<Entity>Dto` / `CreationDto` / `UpdateDto` and their `<Entity>MappingExtensions` |
| `Validators/<Entity>/` | FluentValidation `<Entity>CreationValidator` / `<Entity>UpdateValidator` |
| `Specifications/<Entities>/`, `Parameters/` | Ardalis query objects (folder name is plural) and paging/filter inputs |
| `Builders/`, `Wrappers/` | `ResponseBuilder<T>` and `JsonResponse` / `JsonPagedResponse` / `Metadata` |
| `Commons/` | `ErrorMessages`, `Roles`, `CacheKey`, `Constants`, custom claim names |
| `Settings/` | Options bound from configuration (`JwtSettings`, `CachingSettings`, `LockoutSettings`) |
| `Attributes/`, `Extensions/` | DI lifecycle attributes; helpers such as `ToPacificStandardTime()` |

## Dependencies

References Domain (plus Ardalis.Specification, FluentValidation and Identity packages). Data access goes through `IRepository<T>` + specifications only; the concrete repository and `DbContext` live in Persistence.

## Conventions

### Service registration (attribute-based DI)

Decorate the service class with `[Transient]`, `[Scoped]` or `[Singleton]`; `RegisterServices()` in `RegisterLayerExtensions.cs` reflectively registers it against the interface named `I{ClassName}`. The interface name **must** match the class name with an `I` prefix — otherwise the service is silently skipped and the host fails at startup (`ValidateOnBuild`). Service classes are `internal`.

### Data access (repository + specification)

One generic `IRepository<TEntity>` (Ardalis `RepositoryBase` underneath). Encode every query as a Specification class under `Specifications/<Entities>/` and pass it to `ListAsync` / `FirstOrDefaultAsync` / `CountAsync` / `AnyAsync`. Common shapes per entity: `<Entity>Specification` (filter / by-id), `Paged<Entity>Specification` (paging + sort), `Duplication<Entity>Specification` (uniqueness checks). Specifications can project straight to a DTO (`Specification<City, CityDto>`). Navigations are loaded only when the specification `Include`s them (no lazy loading).

### Mapping (manual, no library)

Each entity has an `internal static class <Entity>MappingExtensions` with up to three methods: `entity.ToDto()`, `creationDto.ToEntity()` and `updateDto.ApplyTo(entity)` (copies the editable fields onto the tracked entity; never the key, since services already reject a URL/body id mismatch). `ToDto` maps a navigation that wasn't loaded to `null` (`city.Department?.ToDto()!`) and a null `IsActive` to `false`. Map lists with `.Select(x => x.ToDto()).ToList()`. Paged specifications still project in `Query.Select(...)` because EF must translate that expression to SQL — keep it in sync with `ToDto` when a DTO changes.

### Responses

Every service method returns `JsonResponse<T>` or `JsonPagedResponse<T>`, built with the static `ResponseBuilder<T>` factory: `.Ok`, `.OkPaged`, `.Created`, `.NoContent`, `.NotFound`, `.BadRequest`, `.Conflict`, `.Unauthorized`, `.TooManyRequests` (locked-out logins). Each wrapper carries an `HttpCode`. Return the matching `ResponseBuilder` result for expected business failures; message texts live in `Commons/ErrorMessages.cs`.

### Validation

Validators are discovered from this assembly and run automatically on controller input (`AddFluentValidationAutoValidation`), with `es-CO` messages.

### Catalog caching

Unpaged `GetAllAsync` methods on catalog-like services cache the entity list through `ICacheService<TEntity>` (an `IMemoryCache` wrapper, TTLs from `CachingSettings`) under a key from `Commons/CacheKey.cs`. Writes do **not** invalidate the cache yet (F-10), so a created/updated record can be missing from `GetAllAsync` until the entry expires.

## Gotchas

- `Roles` constant values are Spanish (`Administrador`, `Operario`) because they match persisted role names.
- `Exceptions/` is empty (F-22). `RuoteParameters.cs` and `ruote*` identifiers are known typos for Route (F-20).
- `InternalsVisibleTo` exposes the internal services to `GoTransport.Api.Unit.Tests`.
