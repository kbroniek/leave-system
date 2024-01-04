using System.Collections.Generic;
using System.Linq;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeUserProvider
{
    public const string FakseoslavId = "1";
    public const string BenId = "2";
    public const string HabibId = "3";
    public const string PhilipId = "4";
    public static FederatedUser GetUserWithNameFakeoslav() =>
        FederatedUser.Create(FakseoslavId, "fakeUser@fake.com", "Fakeoslav.Wagner", new[] { RoleType.GlobalAdmin.ToString() });

    public static FederatedUser GetUserBen() =>
        FederatedUser.Create(BenId, "ben@fake.com", "Ben.Shapiro", new[] { RoleType.Employee.ToString() });

    public static FederatedUser GetUserHabib() =>
        FederatedUser.Create(HabibId, "habib@arabia.com", "Ben.Shapiro", new[] { RoleType.Employee.ToString() });

    public static FederatedUser GetUserPhilip() =>
        FederatedUser.Create(PhilipId, "philip.nov@fakemail.com", "Philip.Novak", new[] { RoleType.Employee.ToString() });

    public static IEnumerable<FederatedUser> GetEmployees()
    {
        yield return GetUserBen();
        yield return GetUserHabib();
        yield return GetUserPhilip();
    }

    public static IEnumerable<FederatedUser> GetAllUsers() =>
        GetEmployees().Append(GetUserWithNameFakeoslav());
}
