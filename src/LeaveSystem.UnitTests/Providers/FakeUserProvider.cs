using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeUserProvider
{
    internal static FederatedUser GetUserWithNameFakeoslav()
    {
        return FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
    }
}