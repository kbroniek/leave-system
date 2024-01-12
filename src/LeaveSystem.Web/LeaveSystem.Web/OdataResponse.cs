using System.Text.Json.Serialization;

namespace LeaveSystem.Web;

public class ODataResponse<T> : ODataResponse
{
    [JsonPropertyName(name: "value")]
    public T? Data { get; set; }
}

public class ODataResponse
{
    [JsonPropertyName(name: "@odata.context")]
    public string? ContextUrl { get; set; }
}
