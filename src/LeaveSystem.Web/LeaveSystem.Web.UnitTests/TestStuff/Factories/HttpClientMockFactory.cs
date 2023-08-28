using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.TestStuff.Factories;

public static class HttpClientMockFactory
{
    
    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, string baseFakeUrl = "http://localhost:5047/")
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(baseFakeUrl + url).RespondWithJson(response);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(baseFakeUrl);
        return httpClient;
    }
}