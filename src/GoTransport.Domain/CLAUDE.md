# GoTransport.Domain

**Responsibility:** the domain entities — what is persisted and how entities relate. Entities carry data and navigations only; behavior lives in Application services.

## Contents

- `Entities/App/` — data the business creates (Points, Routes, Schedules, Vehicles, Reservations) and the ASP.NET Identity entities (`User`, `Role`, `UserRole`, …). Mapped to `APP_*` tables.
- `Entities/Bas/` — seeded reference data that rarely changes (Departments, Cities, Identification types, Manufacturers). Mapped to `BAS_*` tables.
- `Commons/AuditableEntity.cs` + `Interfaces/IAuditableEntity.cs` — the audit-column base (`IsActive`, `CreatedAt`, `UpdatedAt`, `CreationUser`, `UpdateUser`); `Commons/AuditDefaults.SystemUser` is the `SISTEMA` audit user.
- `Interfaces/IRepository.cs`, `Interfaces/IUnitOfWork.cs` — the data-access contracts Application uses and Persistence implements.

## Dependencies

Only `Microsoft.Extensions.Identity.Stores` (`User`/`Role` and the Identity join entities extend the `Identity*<long>` types; no EF Core) and `Ardalis.Specification` (for `IRepository<T>`). It is the bottom of the graph: every other project may reference it, it references none of them.

## Conventions

- New entities extend `AuditableEntity`; Persistence stamps the audit fields on save (see `src/GoTransport.Persistence/CLAUDE.md`). `IsActive` is a plain `bool` that starts `true` for a new entity; the column is NOT NULL with no database default.
- Reference navigations to a required principal use EF's `= null!` pattern: they are null until a specification `Include`s them, so map them with `?.` (see the Application mapping convention).
- Name entities and properties with the `GLOSSARY.md` terms. Pick `App/` vs `Bas/` by the split above.
- Existing entity XML docs are in Spanish; write new ones in English.

## Gotchas

Some persisted names diverge from the glossary and keep their names because columns depend on them:

- `Reservation.ReservationDate` is the **travel date**, not the date the booking was made.
- `Vehicle.Model` holds the **model year** (e.g. 2022), not a model name.
