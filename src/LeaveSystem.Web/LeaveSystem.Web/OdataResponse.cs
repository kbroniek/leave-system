using System.Text.Json.Serialization;

namespace LeaveSystem.Web;

public class ODataResponse<T>
{
    [JsonPropertyName(name: "@odata.context")]
    public string? ContextUrl { get; set; }

    [JsonPropertyName(name: "value")]
    public T? Data { get; set; }
}
