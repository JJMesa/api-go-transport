using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using GoTransport.Api.Component.Tests.Settings;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests;

public class ComponentTest
{
    protected readonly HttpClient Client;
    private static UserTokenDto userToken = new();
    protected AuthTestingSettings authTestingSettings;

    protected JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    protected ComponentTest()
    {
        IConfigurationRoot _config = null!;
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "ComponentTesting");
        Environment.SetEnvironmentVariable("Environment", "ComponentTesting");

        string environment = Environment.GetEnvironmentVariable("Environment")!;
        var isComponentTesting = !string.IsNullOrEmpty(environment) && environment.ToLower() == "componenttesting";

        var appFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((hostContext, cfgbuilder) =>
                {
                    if (isComponentTesting)
                    {
                        cfgbuilder.Sources.Clear();
                        cfgbuilder
                            .AddJsonFile("appsettings.json", true, true)
                            .AddJsonFile($"appsettings.{environment}.json", true, true)
                            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                            .AddEnvironmentVariables();
                    }

                    _config = cfgbuilder.Build();
                });

                builder.ConfigureServices(services =>
                {
                    services.BuildServiceProvider();
                });
            });

        Client = appFactory.CreateDefaultClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        authTestingSettings = _config.GetSection("AuthTestingSettings").Get<AuthTestingSettings>()!;
    }

    protected async Task AddAuthorization(bool regenerateToken = false)
    {
        if (!string.IsNullOrEmpty(userToken.Token) && !regenerateToken)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken.Token);
            return;
        }

        var userLoginDto = new UserLoginDto()
        {
            Email = authTestingSettings.Email,
            Password = authTestingSettings.Password
        };

        var contentRequest = new StringContent(JsonConvert.SerializeObject(userLoginDto, JsonSettings), Encoding.UTF8, MediaTypeNames.Application.Json);
        var apiResponse = await Client.PostAsync(ApiPaths.AuthBasePath, contentRequest);

        if (apiResponse.IsSuccessStatusCode)
        {
            var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
            var auth = JsonConvert.DeserializeObject<JsonResponse<UserTokenDto>>(jsonResponse, JsonSettings);
            userToken = auth?.Data!;
            if (regenerateToken) Client.DefaultRequestHeaders.Remove("Authorization");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Data!.Token);
        }
    }
}