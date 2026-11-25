---
paths:
  - "**/*.csproj"
  - "Directory.*.props"
---

# Project files and package versions

- Every project targets `net10.0` (LTS, supported until November 2028). The target framework, `Nullable` and `ImplicitUsings` come from the root `Directory.Build.props`; don't repeat them in a `.csproj`.
- Central package management is on: a `PackageReference` has no `Version`, and every version lives in the root `Directory.Packages.props`. To add a package, add its `PackageVersion` there and a version-less `PackageReference` in the project.
- Keep the `Microsoft.AspNetCore.*`, `Microsoft.EntityFrameworkCore.*` and `Microsoft.Extensions.*` packages on one 10.0.x patch, and bump them together. Keep `System.IdentityModel.Tokens.Jwt` on the IdentityModel line that `Microsoft.AspNetCore.Authentication.JwtBearer` depends on: mixed IdentityModel versions can fail token validation at runtime.
- Application uses `<FrameworkReference Include="Microsoft.AspNetCore.App" />` for Identity, `IHttpContextAccessor` and MVC types instead of individual `Microsoft.Extensions.*` packages.
- `FluentValidation.AspNetCore` 11.3.1 is the last release of that package and holds FluentValidation on 11.x. Moving to FluentValidation 12 means replacing the auto-validation package.
- Run `dotnet list package --vulnerable --include-transitive` after any package change; it should report nothing. Restore also runs NuGet Audit and warns (`NU190x`) about vulnerable packages.
- `dotnet-ef` is a local tool pinned in the root `dotnet-tools.json` (`dotnet tool restore`); keep its version equal to the EF Core packages.
- `ProjectReference` paths are relative: projects in `src/` reference siblings as `..\<Project>\<Project>.csproj`; test projects reach production code through `..\..\src\<Project>\<Project>.csproj`.
- `InternalsVisibleTo` entries name assemblies (Application → `GoTransport.Api.Unit.Tests`, Api → `GoTransport.Api.Component.Tests`); keep them in sync if a test project is renamed.
