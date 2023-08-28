namespace LeaveSystem.Web.Pages.Roles;

public record GetRolesDto(IEnumerable<GetUserRolesDto> Items);

public record GetUserRolesDto(string Id, IEnumerable<string> Roles);