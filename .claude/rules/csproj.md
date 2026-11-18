---
paths:
  - "**/*.csproj"
---

# Project files and package versions

- Every project targets `net6.0`, but the EF Core runtime is **7.0.17** (SqlServer, Tools, InMemory). Pin any new `Microsoft.EntityFrameworkCore.*` package to exactly `7.0.17`: a 6.x InMemory package once caused a `MissingMethodException` at runtime.
- There is no `Directory.Build.props` or central package management yet (F-06), so versions live in each `.csproj`. When you bump a package that several projects share, bump it in all of them in the same change.
- Known drift to leave alone unless the task is about it: `Ardalis.Specification.EntityFrameworkCore` is 7.0.0 in Application and 8.0.0 in Persistence; the component tests' `Microsoft.AspNetCore.Mvc.Testing` 6.0.28 causes an MSB3277 `Microsoft.Extensions.DependencyModel` 6-vs-7 warning.
- `ProjectReference` paths are relative: projects in `src/` reference siblings as `..\<Project>\<Project>.csproj`; test projects reach production code through `..\..\src\<Project>\<Project>.csproj`.
- `InternalsVisibleTo` entries name assemblies (Application → `GoTransport.Api.Unit.Tests`, Api → `GoTransport.Api.Component.Tests`); keep them in sync if a test project is renamed.
