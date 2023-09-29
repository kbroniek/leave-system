using System.Text.Json;
using LeaveSystem.Shared;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class MockHttpHandlerExtensions
{
    public static readonly string BaseFakeUrl = "http://localhost:5047/";

    public static MockedRequest WhenWithBaseUrl(this MockHttpMessageHandler source, string restOfUrl)
    {
        return source.When(BaseFakeUrl + restOfUrl);
    }

    public static MockedRequest RespondWithJson<T>(this MockedRequest source, T objectToSerialize) =>
        RespondWithJson(source, objectToSerialize, JsonSerializerOptions.Default);
    
    public static MockedRequest RespondWithJson<T>(this MockedRequest source, T objectToSerialize, JsonSerializerOptions jsonSerializerOptions)
    {
        var jsonContent = JsonSerializer.Serialize(objectToSerialize, jsonSerializerOptions);
        return source.Respond("application/json", jsonContent);
    } 
    
    public static MockedRequest WithJsonContent(this MockedRequest source, object objectToSerialize)
    {
        var serializedObject = JsonSerializer.Serialize(objectToSerialize);
        return source.WithContent(serializedObject);
    } 
}