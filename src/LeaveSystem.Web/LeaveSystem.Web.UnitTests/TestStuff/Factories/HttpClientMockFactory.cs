using System.Text.Json;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.TestStuff.Factories;

public static class HttpClientMockFactory
{

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, string baseFakeUrl = "http://localhost:5047/") =>
        CreateWithJsonResponse(url, response, JsonSerializerOptions.Default, baseFakeUrl);

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, JsonSerializerOptions options, string baseFakeUrl = "http://localhost:5047/")
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(baseFakeUrl + url).RespondWithJson(response, options);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(baseFakeUrl);
        return httpClient;
    }
}