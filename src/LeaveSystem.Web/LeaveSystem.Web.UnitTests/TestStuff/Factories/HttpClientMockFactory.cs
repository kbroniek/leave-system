using System.Net;
using System.Text.Json;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.TestStuff.Factories;

public static class HttpClientMockFactory
{
    private const string baseFakeUrl = "http://localhost:5047/";
    public static HttpClient CreateWithJsonResponse<T>(string url, T? response) =>
        CreateWithJsonResponse(url, response, JsonSerializerOptions.Default, baseFakeUrl);

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, JsonSerializerOptions options, string baseFakeUrl = "http://localhost:5047/")
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(baseFakeUrl + url).RespondWithJson(response, options);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(baseFakeUrl);
        return httpClient;
    }

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, JsonSerializerOptions options, out MockedHttpValues mockedHttpValues, string baseFakeUrl = "http://localhost:5047/")
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var request = mockHttpMessageHandler.When(baseFakeUrl + url).RespondWithJson(response, options);
        mockedHttpValues = new MockedHttpValues(request, mockHttpMessageHandler);
        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(baseFakeUrl)
        };
        return httpClient;
    }

    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode)
        => CreateWithJsonContent(url, content, httpStatusCode, JsonSerializerOptions.Default);
    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode, JsonSerializerOptions options)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(baseFakeUrl + url).WithJsonContent(content, options).Respond(httpStatusCode);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(baseFakeUrl);
        return httpClient;
    }

    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode,
        HttpContent responseContent)
        => CreateWithJsonContent(url, content, httpStatusCode, responseContent, JsonSerializerOptions.Default);
    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode, HttpContent responseContent, JsonSerializerOptions options)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(baseFakeUrl + url).WithJsonContent(content, options).Respond(httpStatusCode, responseContent);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(baseFakeUrl);
        return httpClient;
    } 
    
    public static HttpClient Create(string url, HttpStatusCode httpStatusCode)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(baseFakeUrl + url).Respond(httpStatusCode);
        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(baseFakeUrl)
        };
        return httpClient;
    }

    public record MockedHttpValues(MockedRequest Request, MockHttpMessageHandler MockHttpMessageHandler)
    {
        public void ShouldMatchCount(int count = 1) => MockHttpMessageHandler.GetMatchCount(Request).Should().Be(count);
    }
}
