using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeUserProvider
{
    internal static FederatedUser GetUserWithNameFakeoslav()
    {
        return FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav", new string[] { RoleType.GlobalAdmin.ToString() });
    }
}