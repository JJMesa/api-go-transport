namespace GoTransport.Api.Unit.Tests.Mothers;

/// <summary>
/// Ids for mocked lookups. Unit tests have no seed data, so the values only need to agree between the
/// arrange and assert steps of a test; component tests take their ids from <c>DatabaseSeeder</c> instead.
/// </summary>
internal static class TestIds
{
    public const int Existing = 1;
    public const int Unknown = 9999999;
}
