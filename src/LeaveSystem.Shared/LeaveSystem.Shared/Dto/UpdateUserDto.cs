namespace LeaveSystem.Shared.Dto;

using System.Text.Json.Serialization;

public record UpdateUserDto(
    [property: JsonPropertyName("accountEnabled")] bool? AccountEnabled,
    [property: JsonPropertyName("jobTitle")] string? JobTitle);

