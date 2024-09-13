namespace LeaveSystem.Shared.Dto;

using System.Text.Json.Serialization;
using LeaveSystem.Shared.LeaveRequests;
using static LeaveSystem.Shared.Dto.LeaveStatusSettingsDto;

public record LeaveStatusSettingsDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("leaveRequestStatus")] LeaveRequestStatus LeaveRequestStatus,
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("state")] LeaveStatusSettingsState State)
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveStatusSettingsState
    {
        Active,
        Inactive,
    };
}
