namespace LeaveSystem.Shared.Dto;
using System.Collections.Generic;

public record UserDto(string Id, string? Name, string? Email, IEnumerable<string>? Roles);
