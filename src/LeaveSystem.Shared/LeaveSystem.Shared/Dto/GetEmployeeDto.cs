namespace LeaveSystem.Shared.Dto;

public record GetEmployeeDto(string Id, string? Name, string? Email)
{
    public static GetEmployeeDto Create(FederatedUser user) => new(user.Id, user.Name, user.Email);
}
