namespace LeaveSystem.Web.Pages.Roles;

public record class GetRolesDto(IEnumerable<GetUserRolesDto> Items);

public record class GetUserRolesDto(string Id, IEnumerable<string> Roles);