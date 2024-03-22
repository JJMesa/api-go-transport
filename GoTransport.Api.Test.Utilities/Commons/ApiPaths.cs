namespace GoTransport.Api.Test.Utilities.Commons;

public class ApiPaths
{
    #region Host-Configuration

    private static readonly string HostName = "localhost";
    private static readonly string Port = "7055";
    private static readonly string HttpProtocol = "https";

    #endregion Host-Configuration

    #region Routes

    public static readonly string BaseUrl = $"{HttpProtocol}://{HostName}:{Port}/api/v1";

    public static readonly string AuthBasePath = $"{BaseUrl}/account/login";

    public static readonly string DepartmentsPath = "/departments";

    public static readonly string DepartmentsFullPath = $"{BaseUrl}{DepartmentsPath}";

    public static readonly string CitiesPath = "/cities";

    public static readonly string CitiesFullPath = $"{BaseUrl}{CitiesPath}";

    #endregion Routes
}