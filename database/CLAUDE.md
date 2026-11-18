# database/ — seed data

**Responsibility:** SQL scripts that load seed data into a database whose schema already exists.

## Contents

- `seed/INSERT_SEED.sql` — **required**: roles, identification types, manufacturers and users, including an `Administrador` to log in with via `POST api/v1/account/login`.
- `seed/INSERT_SEED_COLOMBIA.sql` — optional: the 33 Colombian departments and 1,122 municipalities.

## Setting up a database

1. Create the schema: `dotnet ef database update --project src/GoTransport.Persistence --startup-project src/GoTransport.Api` (needs `ConnectionStrings:DefaultConnection`, see `src/GoTransport.Api/CLAUDE.md`).
2. Run `INSERT_SEED.sql`.
3. Optionally run `INSERT_SEED_COLOMBIA.sql`.

## Conventions

- The schema belongs to the EF Core migrations in `src/GoTransport.Persistence/Migrations`; scripts here contain data (`INSERT`s) only.
- Keep persisted Spanish values exactly as they are (`Administrador`, `Operario`, the `SISTEMA` audit user): the application matches on them.
- Table and column names follow Persistence's `BAS_*` / `APP_*` and `UPPER_SNAKE_CASE` convention.

## Gotchas

- Component tests don't use these scripts; their data comes from `tests/GoTransport.Api.Component.Tests/Infrastructure/DatabaseSeeder`.
- `INSERT_SEED.sql` contains a versioned admin credential (F-24).
- Both scripts are Windows-1252 / ISO-8859 encoded with CRLF line endings, not UTF-8. Save edits in that same encoding so accented values (Bogotá, Cédula) survive.
