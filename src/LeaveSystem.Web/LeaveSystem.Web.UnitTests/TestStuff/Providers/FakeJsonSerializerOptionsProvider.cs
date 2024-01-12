using System.Text.Json;
using LeaveSystem.Web.UnitTests.TestStuff.Converters;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public static class FakeJsonSerializerOptionsProvider
{
    public static JsonSerializerOptions GetWithTimespanConverter() => new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new TimeSpanToStringConverter()
        },
    };
}