using System.Text.Json;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class MockHttpHandlerExtensions
{
    public static readonly string BaseFakeUrl = "http://localhost:5047/";

    public static MockedRequest WhenWithBaseUrl(this MockHttpMessageHandler source, string restOfUrl)
    {
        return source.When(BaseFakeUrl + restOfUrl);
    }

    public static MockedRequest RespondWithJson<T>(this MockedRequest source, T objectToSerialize)
    {
        var jsonContent = JsonSerializer.Serialize(objectToSerialize);
        return source.Respond("application/json", jsonContent);
    } 
}