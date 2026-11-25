namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// The component-test host with the production JWT bearer authentication instead of the test scheme.
/// </summary>
public class JwtWebApplicationFactory : CustomWebApplicationFactory
{
    protected override bool UseTestAuthentication => false;
}
