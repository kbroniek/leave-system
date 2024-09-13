namespace LeaveSystem.Shared.Dto;
using System;
using System.Text.Json.Serialization;

public record LeaveLimitDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("limit")] TimeSpan? Limit,
    [property: JsonPropertyName("overdueLimit")] TimeSpan? OverdueLimit,
    [property: JsonPropertyName("workingHours")] TimeSpan WorkingHours,
    [property: JsonPropertyName("leaveTypeId")] Guid LeaveTypeId,
    [property: JsonPropertyName("validSince")] DateOnly? ValidSince,
    [property: JsonPropertyName("validUntil")] DateOnly? ValidUntil,
    [property: JsonPropertyName("assignedToUserId")] string AssignedToUserId,
    [property: JsonPropertyName("description")] string? Description = null);


public record SearchLeaveLimitQuery(int Year, string[] UserIds, Guid[] LeaveTypeIds, int? PageSize, string? ContinuationToken);

public record SearchUserLeaveLimitQuery(int Year, Guid[] LeaveTypeIds, int? PageSize, string? ContinuationToken);
