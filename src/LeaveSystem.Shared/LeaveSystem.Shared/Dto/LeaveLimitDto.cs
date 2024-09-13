namespace LeaveSystem.Shared.Dto;
using System;
using System.Text.Json.Serialization;
using static LeaveSystem.Shared.Dto.LeaveLimitDto;

public record LeaveLimitDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("limit")] TimeSpan? Limit,
    [property: JsonPropertyName("overdueLimit")] TimeSpan? OverdueLimit,
    [property: JsonPropertyName("workingHours")] TimeSpan WorkingHours,
    [property: JsonPropertyName("leaveTypeId")] Guid LeaveTypeId,
    [property: JsonPropertyName("validSince")] DateOnly? ValidSince,
    [property: JsonPropertyName("validUntil")] DateOnly? ValidUntil,
    [property: JsonPropertyName("assignedToUserId")] string AssignedToUserId,
    [property: JsonPropertyName("state")] LeaveLimitState State,
    [property: JsonPropertyName("description")] string? Description = null)
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveLimitState
    {
        Active,
        Inactive,
    };
}


public record SearchLeaveLimitQuery(int Year, string[] UserIds, Guid[] LeaveTypeIds, int? PageSize, string? ContinuationToken);

public record SearchUserLeaveLimitQuery(int Year, Guid[] LeaveTypeIds, int? PageSize, string? ContinuationToken);
