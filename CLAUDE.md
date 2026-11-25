# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`GoTransport.Api` is a **.NET 10 / C# REST API** for an intercity passenger-transport company in Colombia (network of routes and schedules, fleet, reservations, staff users). Despite the `api_go_transport` repo name, there is **no Go code here** — "Go" is the product name. The pre-existing codebase is Spanish: comments, error messages and domain values (`Administrador`, `Operario`, `SISTEMA`) are in Spanish, and FluentValidation runs under the `es-CO` culture. `Readme.md` is in English.

Use the domain vocabulary below for names in code, docs and conversation:

@GLOSSARY.md

### Language policy (from 2026-07-02)

Pre-existing code stays in Spanish, but **all newly added application text must be written in English** — log messages, exception/error messages, API response messages, other user-facing strings, and new code comments / XML docs. Leave existing Spanish text as-is unless you are already editing it. **Exception:** identifiers and domain values tied to persisted data or existing conventions (e.g. role names like `Administrador`, the `SISTEMA` audit user, seed values, DB column names) keep their current form to avoid breaking data and consistency.

### Improvement plan & commits

Ongoing work follows the prioritized plan in `docs/audit/TECHNICAL_AUDIT.md` §7.2 (items `F-01` onward, with a status column per item; findings `A-xx` in §5); the `implement-plan-item` skill walks through implementing and recording one. Commit messages are in English, prefixed with a tag — `[Feature]`, `[Fix]`, `[Refactor]`, `[Test]` — and reference the plan item they implement (e.g. "Implements F-05").

## Repository map

Each area has its own `CLAUDE.md`, loaded when you work inside it; this file holds only what applies everywhere.

| Path | Responsibility |
|---|---|
| `src/` | Production code: four layered projects (Domain, Application, Persistence, Api) |
| `tests/` | xUnit unit and component tests, each suite with its own helpers |
| `docs/` | Audit report with the improvement plan; ADRs under `docs/adr/` |
| `database/` | SQL seed scripts (the schema itself comes from EF Core migrations) |
| `.github/workflows/` | CI (`ci.yml`): Release build with warnings as errors, both test suites with coverage, a coverage/CRAP gate (`tools/coverage/coverage_gate.py`) and a gate on High/Critical vulnerable packages, on pushes to `master`/`Dev` and on pull requests |
| `.editorconfig` | Code style and analyzer severities, enforced by every build; deliberate exceptions carry a comment |
| `.claude/` | Shared Claude Code settings, path-scoped `rules/`, and the `new-crud-resource` and `implement-plan-item` skills |
| `GLOSSARY.md` | Domain vocabulary (imported above) |

## Common commands

```bash
# Build the whole solution (CI also treats warnings as errors: add -warnaserror to check locally)
dotnet build GoTransport.sln

# Run the API (needs configuration — see src/GoTransport.Api/CLAUDE.md; Swagger at https://localhost:7046/swagger, probes at /health/live and /health/ready)
dotnet run --project src/GoTransport.Api

# Unit tests (services tested in isolation with mocked repositories/cache)
dotnet test tests/GoTransport.Api.Unit.Tests

# Component tests (full HTTP pipeline over EF Core InMemory — no SQL Server, seed or secrets needed)
dotnet test tests/GoTransport.Api.Component.Tests

# Coverage and CRAP gate, as CI runs it (dotnet tool restore first; output in coverage/, git-ignored)
dotnet test tests/GoTransport.Api.Unit.Tests --settings coverage.runsettings --results-directory coverage/raw
dotnet test tests/GoTransport.Api.Component.Tests --settings coverage.runsettings --results-directory coverage/raw
dotnet reportgenerator -reports:"coverage/raw/**/coverage.cobertura.xml" -targetdir:coverage/report -reporttypes:"Html;Cobertura"
python tools/coverage/coverage_gate.py coverage/report/Cobertura.xml
python -m unittest discover -s tools/coverage   # tests of the gate script itself

# A single test class or method
dotnet test --filter "FullyQualifiedName~CityServiceTests"
dotnet test --filter "Name=GetByIdAsync_ShouldReturnCityForValidId"

# EF Core migrations (migrations live in Persistence; Api is the startup project; run `dotnet tool restore` once for dotnet-ef)
dotnet ef database update --project src/GoTransport.Persistence --startup-project src/GoTransport.Api
dotnet ef migrations add <Name> --project src/GoTransport.Persistence --startup-project src/GoTransport.Api
```

To run the API against a real database, follow `database/CLAUDE.md` (migrate, then load the seed scripts).
