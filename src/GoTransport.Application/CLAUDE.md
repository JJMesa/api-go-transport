# GoTransport.Application

**Responsibility:** all business logic. Services receive DTOs, enforce business rules, query through specifications, and return a `JsonResponse` envelope to the Api.

## Contents

| Folder | Holds |
|---|---|
| `Services/`, `Interfaces/` | One `I<Entity>Service` / `<Entity>Service` pair per resource; `Interfaces/Base/` declares `IRepository<T>` and `IUnitOfWork` |
| `Dtos/<Entity>/`, `Mappings/` | `<Entity>Dto` / `CreationDto` / `UpdateDto` and their `<Entity>MappingExtensions` |
| `Validators/<Entity>/` | FluentValidation `<Entity>CreationValidator` / `<Entity>UpdateValidator` |
| `Specifications/<Entities>/`, `Parameters/` | Ardalis query objects (folder name is plural) and paging/filter inputs |
| `Builders/`, `Wrappers/` | `ResponseBuilder<T>` and `JsonResponse` / `JsonPagedResponse` / `Metadata` |
| `Commons/` | `ErrorMessages`, `Roles`, `CacheKey`, `Constants`, custom claim names |
| `Settings/` | Options bound from configuration (`JwtSettings`, `CachingSettings`, `LockoutSettings`, `AdminBootstrapSettings`) |
| `Attributes/`, `Extensions/` | DI lifecycle attributes; helpers such as `ToPacificStandardTime()` |

## Dependencies

References Domain, the ASP.NET Core shared framework (Identity, `IHttpContextAccessor`), Ardalis.Specification (core only, no EF Core), FluentValidation and `System.IdentityModel.Tokens.Jwt`. Data access goes through `IRepository<T>` + specifications only; the concrete repository and `DbContext` live in Persistence.

## Conventions

### Service registration (attribute-based DI)

Decorate the service class with `[Transient]`, `[Scoped]` or `[Singleton]`; `RegisterServices()` in `RegisterLayerExtensions.cs` reflectively registers it against the interface named `I{ClassName}`. The interface name **must** match the class name with an `I` prefix — otherwise the service is silently skipped and the host fails at startup (`ValidateOnBuild`). Service classes are `internal`.

### Data access (repository + specification)

One generic `IRepository<TEntity>` (Ardalis `RepositoryBase` underneath). Encode every query as a Specification class under `Specifications/<Entities>/` and pass it to `ListAsync` / `FirstOrDefaultAsync` / `CountAsync` / `AnyAsync`. Common shapes per entity: `<Entity>Specification` (filter / by-id), `<Entity>DetailSpecification` (one row with the navigations its DTO shows, for get-by-id and for reloading after a create), `Paged<Entity>Specification` (paging + sort) and the count `<Entity>Specification(parameters)`, which both call one `<Entity>Filter.Apply` so a page and its total always agree, `Duplication<Entity>Specification` (uniqueness checks). Never answer a get-by-id with the repository's `GetByIdAsync`: it loads no navigations. Specifications that only feed responses or the cache call `Query.AsNoTracking()`; one that loads an entity to change and save it must not (`SpecificationTrackingTests` lists both kinds). Specifications can project straight to a DTO (`Specification<City, CityDto>`). Navigations are loaded only when the specification `Include`s them (no lazy loading).

Each repository call saves on its own. When a check and a write must be atomic, or several writes must succeed together, wrap them in `IUnitOfWork.ExecuteInTransactionAsync` (`Interfaces/Base/`): it commits when the returned response is `Ok` and rolls back otherwise. Load everything inside the operation, because a transient failure retries it from a clean change tracker. `ReservationService.CreateAsync` is the reference: it counts the places taken on the Trip (`TripReservationSpecification`) and inserts under `IsolationLevel.Serializable`, so two bookings cannot both take the last place.

### Mapping (manual, no library)

Each entity has an `internal static class <Entity>MappingExtensions` with up to three methods: `entity.ToDto()`, `creationDto.ToEntity()` and `updateDto.ApplyTo(entity)` (copies the editable fields onto the tracked entity; never the key, since services already reject a URL/body id mismatch). `ToDto` maps a navigation that wasn't loaded to `null` (`city.Department?.ToDto()!`) and a null `IsActive` to `false`. Map lists with `.Select(x => x.ToDto()).ToList()`. Paged specifications still project in `Query.Select(...)` because EF must translate that expression to SQL — keep it in sync with `ToDto` when a DTO changes.

### Responses

Every service method returns `JsonResponse<T>` or `JsonPagedResponse<T>`, built with the static `ResponseBuilder<T>` factory: `.Ok`, `.OkPaged`, `.Created`, `.NoContent`, `.NotFound`, `.BadRequest`, `.Conflict`, `.Unauthorized`, `.TooManyRequests` (rate-limited sign-ins), `.Forbidden`. Each wrapper carries an `HttpCode`. Return the matching `ResponseBuilder` result for expected business failures; message texts live in `Commons/ErrorMessages.cs`.

### Validation

Validators are discovered from this assembly and run automatically on controller input (`AddFluentValidationAutoValidation`), with `es-CO` messages.

### Catalog caching

Unpaged `GetAllAsync` methods on catalog-like services cache the entity list through `ICacheService<TEntity>` (an `IMemoryCache` wrapper, TTLs from `CachingSettings`) under a key from `Commons/CacheKey.cs`. Every create, update and delete calls the service's `InvalidateCachedLists()`, which removes each cached list the write makes stale, including lists that embed the entity (a Point or City write also drops `CacheKey.Routes`). Add the key there when a new cached list shows an entity.

## Gotchas

- `Roles` constant values are Spanish (`Administrador`, `Operario`) because they match persisted role names.
- `InternalsVisibleTo` exposes the internal services to `GoTransport.Api.Unit.Tests`.
