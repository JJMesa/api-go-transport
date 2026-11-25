# GoTransport.Api.Unit.Tests

**Responsibility:** tests each Application service in isolation — business rules and response codes, with no database or HTTP.

## Conventions

- Construct the `internal` service directly (Application grants `InternalsVisibleTo` to this project) with `Mock<IRepository<T>>` and `Mock<ICacheService<T>>` (Moq).
- Mapping is plain code called from the service, so there is no mapper to inject or mock. The mapping rules themselves are covered in `TestCases/MappingExtensionsTests.cs` — add cases there when a DTO or mapping changes.
- Assert on the returned envelope: `HttpCode` plus the data or error message.
- Build inputs and entities with the `Mothers/*BuilderMother` factories (add one when a new resource needs test data) and take ids from `Mothers/TestIds`: there is no seed, so an id only has to agree between arrange and assert.
