namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// URLs of the API under test. The test server accepts any host, so the absolute https address only gives
/// requests a scheme (HTTPS redirection and HSTS see an https request).
/// </summary>
internal static class ApiPaths
{
    #region Host-Configuration

    private static readonly string HostName = "localhost";
    private static readonly string Port = "7055";
    private static readonly string HttpProtocol = "https";

    #endregion Host-Configuration

    #region Routes

    public static readonly string BaseUrl = $"{HttpProtocol}://{HostName}:{Port}/api/v1";

    public static readonly string HealthPath = $"{HttpProtocol}://{HostName}:{Port}/health";

    public static readonly string AuthBasePath = $"{BaseUrl}/account/login";

    public static readonly string DepartmentsPath = "/departments";

    public static readonly string DepartmentsFullPath = $"{BaseUrl}{DepartmentsPath}";

    public static readonly string CitiesPath = "/cities";

    public static readonly string CitiesFullPath = $"{BaseUrl}{CitiesPath}";

    public static readonly string SchedulesPath = "/schedules";

    public static readonly string SchedulesFullPath = $"{BaseUrl}{SchedulesPath}";

    public static readonly string UsersPath = "/users";

    public static readonly string UsersFullPath = $"{BaseUrl}{UsersPath}";

    public static readonly string PointsPath = "/points";

    public static readonly string PointsFullPath = $"{BaseUrl}{PointsPath}";

    public static readonly string VehiclesPath = "/vehicles";

    public static readonly string VehiclesFullPath = $"{BaseUrl}{VehiclesPath}";

    public static readonly string RoutesPath = "/routes";

    public static readonly string RoutesFullPath = $"{BaseUrl}{RoutesPath}";

    public static readonly string ReservationsPath = "/reservations";

    public static readonly string ReservationsFullPath = $"{BaseUrl}{ReservationsPath}";

    #endregion Routes
}