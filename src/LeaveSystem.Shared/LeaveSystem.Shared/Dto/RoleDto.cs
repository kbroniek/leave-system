namespace LeaveSystem.Shared.Dto;
using System;
using System.Text.Json.Serialization;

public record RoleDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("roles")] string[] Roles);
