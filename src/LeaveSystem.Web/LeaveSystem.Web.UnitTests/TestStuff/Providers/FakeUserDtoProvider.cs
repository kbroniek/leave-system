using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UsersManagement;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public static class FakeUserDtoProvider
{
    public static IEnumerable<UserDto> GetAll() =>
        FakeUserProvider.GetAllUsers().Select(ToUserDto);

    private static UserDto ToUserDto(FederatedUser federatedUser) =>
        new(federatedUser.Id, federatedUser.Name, federatedUser.Email, federatedUser.Roles);
}