namespace LeaveSystem.Shared.Dto;
using System.Collections.Generic;

public record UserDto(string Id, string? Name, string? FirstName, string? LastName, IEnumerable<string> Roles, string? JobTitle, bool? AccountEnabled);
