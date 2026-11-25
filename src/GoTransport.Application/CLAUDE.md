# GoTransport.Application

**Responsibility:** all business logic. Services receive DTOs, enforce business rules, query through specifications, and return a `JsonResponse` envelope to the Api.

## Contents

| Folder | Holds |
|---|---|
| `Services/`, `Interfaces/` | One `I<Entity>Service` / `<Entity>Service` pair per resource (`IRepository<T>` and `IUnitOfWork` are Domain contracts) |
| `Dtos/<Entity>/`, `Mappings/` | `<Entity>Dto` / `CreationDto` / `UpdateDto` and their `<Entity>MappingExtensions` |
| `Validators/<Entity>/` | FluentValidation `<Entity>CreationValidator` / `<Entity>UpdateValidator` |
| `Specifications/<Entities>/`, `Parameters/` | Ardalis query objects (folder name is plural) and paging/filter inputs |
| `Builders/`, `Wrappers/` | `ResponseBuilder<T>` and `JsonResponse` / `JsonPagedResponse` / `Metadata` |
| `Commons/` | `ErrorMessages`, `Roles`, `CacheKey`, `Constants`, custom claim names |
| `Settings/` | Options bound from configuration (`JwtSettings`, `CachingSettings`, `LockoutSettings`, `AdminBootstrapSettings`) |
| `Extensions/` | Helpers such as `ToContainsPattern()` and the Schedule time rules (`ScheduleExtensions`) |

## Dependencies

References Domain, the ASP.NET Core shared framework (Identity, `IHttpContextAccessor`), Ardalis.Specification (core only, no EF Core), FluentValidation and `System.IdentityModel.Tokens.Jwt`. It also configures Identity's options (password policy, lockout). Data access goes through `IRepository<T>` + specifications only; the concrete repository and `DbContext` live in Persistence.

## Conventions

### Service registration

Register each service in `RegisterServices()` (`RegisterLayerExtensions.cs`) with one explicit line, e.g. `services.AddTransient<ICityService, CityService>();`. Services are transient and `internal`, paired with an `I{ClassName}` interface. A missing line fails startup validation (`ValidateOnBuild`) with the interface's name, which also fails every component test.

### Data access (repository + specification)

One generic `IRepository<TEntity>` (Ardalis `RepositoryBase` underneath). Encode every query as a Specification class under `Specifications/<Entities>/` and pass it to `ListAsync` / `FirstOrDefaultAsync` / `CountAsync` / `AnyAsync`. Common shapes per entity: `<Entity>Specification` (filter / by-id), `<Entity>DetailSpecification` (one row with the navigations its DTO shows, for get-by-id and for reloading after a create), `Paged<Entity>Specification` (paging + sort) and the count `<Entity>Specification(parameters)`, which both call one `<Entity>Filter.Apply` so a page and its total always agree, `Duplication<Entity>Specification` (uniqueness checks). Never answer a get-by-id with the repository's `GetByIdAsync`: it loads no navigations. Specifications that only feed responses or the cache call `Query.AsNoTracking()`; one that loads an entity to change and save it must not (`SpecificationTrackingTests` lists both kinds). Specifications can project straight to a DTO (`Specification<City, CityDto>`). Navigations are loaded only when the specification `Include`s them (no lazy loading).

Each repository call saves on its own. When a check and a write must be atomic, or several writes must succeed together, wrap them in `_unitOfWork.ExecuteInTransactionAsync(...)` (the `UnitOfWorkExtensions` overload for envelopes): it commits when the returned response is `Ok` and rolls back otherwise. User creation (with its Role) and the administrator bootstrap use it too. Load everything inside the operation, because a transient failure retries it from a clean change tracker. `ReservationService.CreateAsync` is the reference: it counts the places taken on the Trip (`TripReservationSpecification`) and inserts under `IsolationLevel.Serializable`, so two bookings cannot both take the last place.

### Mapping (manual, no library)

Each entity has an `internal static class <Entity>MappingExtensions` with up to three methods: `entity.ToDto()`, `creationDto.ToEntity()` and `updateDto.ApplyTo(entity)` (copies the editable fields onto the tracked entity; never the key, since services already reject a URL/body id mismatch). `ToDto` maps a navigation that wasn't loaded to `null` (`city.Department?.ToDto()!`) and a null `IsActive` to `false`. Map lists with `.Select(x => x.ToDto()).ToList()`. Paged specifications still project in `Query.Select(...)` because EF must translate that expression to SQL — keep it in sync with `ToDto` when a DTO changes.

### Responses

Every service method returns `JsonResponse<T>` or `JsonPagedResponse<T>`, built with the static `ResponseBuilder<T>` factory: `.Ok`, `.OkPaged`, `.Created`, `.NoContent`, `.NotFound`, `.BadRequest`, `.Conflict`, `.Unauthorized`, `.TooManyRequests` (rate-limited sign-ins), `.Forbidden`. Each wrapper carries an `HttpCode`. Return the matching `ResponseBuilder` result for expected business failures: `.BadRequest` for invalid input, `.NotFound` for an unknown id, `.Conflict` for a duplicate or a rule against stored data (a full Trip), `.Created` for every create. Message texts live in `Commons/ErrorMessages.cs`. The envelope is the API contract on purpose (`docs/adr/0001-keep-the-json-response-envelope.md`).

### Time

Take "now" from the injected `TimeProvider` (registered as `TimeProvider.System`), never `DateTime.Now`, so time rules are testable with `FakeTimeProvider`. Schedules and travel dates are Colombian wall-clock times: compare them with `ColombiaTime.Now(clock)`. Everything stored or returned as an instant (audit columns, token expiry) is UTC.

### Validation

Validators are discovered from this assembly and run automatically on controller input (`AddFluentValidationAutoValidation`), with `es-CO` messages.

### Catalog caching

Unpaged `GetAllAsync` methods on catalog-like services cache the entity list through `ICacheService<TEntity>` (an `IMemoryCache` wrapper, TTLs from `CachingSettings`) under a key from `Commons/CacheKey.cs`. Every create, update and delete calls the service's `InvalidateCachedLists()`, which removes each cached list the write makes stale, including lists that embed the entity (a Point or City write also drops `CacheKey.Routes`). Add the key there when a new cached list shows an entity.

## Gotchas

- `Roles` constant values are Spanish (`Administrador`, `Operario`) because they match persisted role names.
- `InternalsVisibleTo` exposes the internal services to `GoTransport.Api.Unit.Tests`.
