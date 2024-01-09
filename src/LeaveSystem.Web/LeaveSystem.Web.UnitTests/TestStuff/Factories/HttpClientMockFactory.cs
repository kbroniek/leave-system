using System.Net;
using System.Text.Json;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.TestStuff.Factories;

using Helpers;

public static class HttpClientMockFactory
{
    private const string BaseFakeUrl = "http://localhost:5047/";

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response) =>
        CreateWithJsonResponse(url, response, JsonSerializerOptions.Default);

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, JsonSerializerOptions options)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(BaseFakeUrl + url).RespondWithJson(response, options);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(BaseFakeUrl);
        return httpClient;
    }

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, JsonSerializerOptions options, out MockedHttpValues mockedHttpValues)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var request = mockHttpMessageHandler.When(BaseFakeUrl + url).RespondWithJson(response, options);
        mockedHttpValues = new MockedHttpValues(request, mockHttpMessageHandler);
        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(BaseFakeUrl)
        };
        return httpClient;
    }

    public static HttpClient CreateWithJsonResponse<T>(string url, T? response, HttpStatusCode statusCode, JsonSerializerOptions options, out MockedHttpValues mockedHttpValues)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var request = mockHttpMessageHandler.When(BaseFakeUrl + url).RespondWithJson(response, statusCode, options);
        mockedHttpValues = new MockedHttpValues(request, mockHttpMessageHandler);
        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(BaseFakeUrl)
        };
        return httpClient;
    }

    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode)
        => CreateWithJsonContent(url, content, httpStatusCode, JsonSerializerOptions.Default);
    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode, JsonSerializerOptions options)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(BaseFakeUrl + url).WithJsonContent(content, options).Respond(httpStatusCode);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(BaseFakeUrl);
        return httpClient;
    }

    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode,
        HttpContent responseContent)
        => CreateWithJsonContent(url, content, httpStatusCode, responseContent, JsonSerializerOptions.Default);
    public static HttpClient CreateWithJsonContent<T>(string url, T? content, HttpStatusCode httpStatusCode, HttpContent responseContent, JsonSerializerOptions options)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(BaseFakeUrl + url).WithJsonContent(content, options).Respond(httpStatusCode, responseContent);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(BaseFakeUrl);
        return httpClient;
    }

    public static HttpClient Create(string url, HttpStatusCode httpStatusCode)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When(BaseFakeUrl + url).Respond(httpStatusCode);
        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(BaseFakeUrl)
        };
        return httpClient;
    }

    public static HttpClient CreateWithException(string url, Exception exception)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var fakeResponseContent = new ExceptionThrowingContent(exception);
        mockHttpMessageHandler.When(BaseFakeUrl + url).Respond(fakeResponseContent);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(BaseFakeUrl);
        return httpClient;
    }

    public static HttpClient CreateWithException(string url, Exception exception, out MockedHttpValues mockedHttpValues)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var fakeResponseContent = new ExceptionThrowingContent(exception);
        var request = mockHttpMessageHandler.When(BaseFakeUrl + url).Respond(fakeResponseContent);
        mockedHttpValues = new MockedHttpValues(request, mockHttpMessageHandler);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri(BaseFakeUrl);
        return httpClient;
    }

    public record MockedHttpValues(MockedRequest Request, MockHttpMessageHandler MockHttpMessageHandler)
    {
        public void RequestShouldBeMatched(int expectedMatchedCount = 1) => this.MockHttpMessageHandler.GetMatchCount(this.Request).Should().Be(expectedMatchedCount);
    }
}
