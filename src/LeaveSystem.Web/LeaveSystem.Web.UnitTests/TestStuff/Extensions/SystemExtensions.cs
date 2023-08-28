using System.Text.Json;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class SystemExtensions
{
    public static ODataResponse<T> ToODataResponse<T>(this T data, string contextUrl = "fakeUrl") => new()
        {
            ContextUrl = contextUrl,
            Data = data
        };

    public static object AsJson(this object source) => JsonSerializer.Serialize(source);
}