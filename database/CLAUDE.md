# database/ — seed data

**Responsibility:** SQL scripts that load seed data into a database whose schema already exists.

## Contents

- `seed/INSERT_SEED.sql` — **required**: roles, identification types and manufacturers. It creates no users.
- `seed/INSERT_SEED_COLOMBIA.sql` — optional: the 33 Colombian departments and 1,122 municipalities.

## Setting up a database

1. Create the schema: `dotnet ef database update --project src/GoTransport.Persistence --startup-project src/GoTransport.Api` (needs `ConnectionStrings:DefaultConnection`, see `src/GoTransport.Api/CLAUDE.md`).
2. Run `INSERT_SEED.sql`.
3. Optionally run `INSERT_SEED_COLOMBIA.sql`.
4. Set `AdminBootstrap:Email` and `AdminBootstrap:Password` in User Secrets or environment variables and start the API: it creates the first `Administrador` (log in via `POST api/v1/account/login`), then the settings can be removed.

## Conventions

- The schema belongs to the EF Core migrations in `src/GoTransport.Persistence/Migrations`; scripts here contain data (`INSERT`s) only.
- Users and credentials stay out of these scripts; accounts come from the startup bootstrap or the Users API.
- Keep persisted Spanish values exactly as they are (`Administrador`, `Operario`, the `SISTEMA` audit user): the application matches on them.
- Audit timestamps are UTC: stamp `CREATED_AT` / `UPDATED_AT` with `GETUTCDATE()`, as both scripts do.
- Table and column names follow Persistence's `BAS_*` / `APP_*` and `UPPER_SNAKE_CASE` convention.

## Gotchas

- Component tests don't use these scripts; their data comes from `tests/GoTransport.Api.Component.Tests/Infrastructure/DatabaseSeeder`.
- Earlier versions of `INSERT_SEED.sql` created an `Administrador` whose password is public in the git history. Any database seeded with those versions must change that administrator's password.
- Both scripts are Windows-1252 / ISO-8859 encoded with CRLF line endings, not UTF-8. Save edits in that same encoding so accented values (Bogotá, Cédula) survive.
