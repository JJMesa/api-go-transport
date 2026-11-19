# GoTransport.Persistence

**Responsibility:** maps the Domain onto SQL Server with EF Core — the `DbContext`, per-entity configurations, migrations, the generic repository, and ASP.NET Identity storage.

## Contents

- `Contexts/ApplicationDbContext.cs` — the `DbContext`, including audit stamping on save.
- `Configurations/{App,Bas}/` — one `IEntityTypeConfiguration` per entity; `Configurations/Common/BaseEntityConfiguration<T>` maps the audit columns once.
- `Migrations/` — EF Core migrations, the source of truth for the schema.
- `Repositories/BaseRepository.cs` — open-generic `IRepository<T>` on Ardalis `RepositoryBase`.
- `RegisterLayerExtensions.cs` — `AddPersistenceLayer`: `DbContext`, Identity (lockout from `LockoutSettings`), repository registration.

## Dependencies

Domain, plus Application for `IRepository<T>`, settings, `Constants.ApplicationUser` and `ToPacificStandardTime()` (F-16 plans to cut this to Domain only).

## Conventions

### DB naming

SQL Server with `Modern_Spanish_CI_AS` collation. Tables are prefixed by area — `BAS_*` for `Entities/Bas`, `APP_*` for `Entities/App` — and columns are `UPPER_SNAKE_CASE`. Each entity configuration inherits `BaseEntityConfiguration<T>` and calls `base.Configure(builder)`. Configurations are auto-applied with `ApplyConfigurationsFromAssembly`, and the context declares no `DbSet`s: a configuration is all a new entity needs here.

### Auditing

`ApplicationDbContext.SaveChangesAsync` stamps `IsActive` (true on insert), `CreatedAt`, `UpdatedAt`, `CreationUser`, `UpdateUser` on every `IAuditableEntity`. The user comes from the JWT `NameIdentifier` claim, falling back to `"SISTEMA"`; timestamps are Colombia local time (UTC−5) despite the helper's name, `ToPacificStandardTime()` (it uses the Windows zone id `SA Pacific Standard Time`; F-53). On insert the creation and update fields use `??=`, so a value the caller set (e.g. in seeds) is kept; on update `UpdateUser` is always overwritten with the current user. Only the async overload stamps — the sync `SaveChanges` does not (F-13).

### Migrations

Create them with `dotnet ef migrations add <Name> --project src/GoTransport.Persistence --startup-project src/GoTransport.Api` and review the generated `Up`/`Down` before committing. Every schema change goes through a migration; `database/` holds data-only seed scripts.

## Gotchas

- No lazy loading: a navigation is `null` unless the specification `Include`s it.
- Deletes are hard deletes through `RepositoryBase` (F-23 covers soft delete).
- EF Core runtime is 7.0.17 on a `net6.0` target — see `.claude/rules/csproj.md` before touching packages.
