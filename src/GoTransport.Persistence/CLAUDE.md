# GoTransport.Persistence

**Responsibility:** maps the Domain onto SQL Server with EF Core — the `DbContext`, per-entity configurations, migrations, the generic repository, and ASP.NET Identity storage.

## Contents

- `Contexts/ApplicationDbContext.cs` — the `DbContext`, including audit stamping on save.
- `Configurations/{App,Bas}/` — one `IEntityTypeConfiguration` per entity; `Configurations/Common/BaseEntityConfiguration<T>` maps the audit columns once.
- `Migrations/` — EF Core migrations, the source of truth for the schema.
- `Repositories/BaseRepository.cs` — open-generic `IRepository<T>` on Ardalis `RepositoryBase`.
- `Repositories/UnitOfWork.cs` — `IUnitOfWork`: one transaction (with the EF execution strategy) around several repository calls; a no-op on EF InMemory.
- `RegisterLayerExtensions.cs` — `AddPersistenceLayer`: `DbContext` (SQL Server with `EnableRetryOnFailure`), Identity (password policy, lockout from the validated `LockoutSettings` options), repository and unit-of-work registration.

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
- `EnableRetryOnFailure` makes a transaction opened outside the execution strategy throw. Open transactions only through `IUnitOfWork`.
- Deletes are hard deletes through `RepositoryBase` (F-23 covers soft delete).
- EF Core 10 on `net10.0`; versions are central in `Directory.Packages.props` — see `.claude/rules/csproj.md` before touching packages.
