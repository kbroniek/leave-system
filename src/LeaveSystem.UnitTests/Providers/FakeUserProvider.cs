using System.Collections.Generic;
using System.Linq;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeUserProvider
{
    public static readonly string FakseoslavId = "1";
    public static readonly string BenId = "2";
    public static string HabibId = "3";
    public static string PhilipId = "4";
    public static FederatedUser GetUserWithNameFakeoslav()
    {
        return FederatedUser.Create(FakseoslavId, "fakeUser@fake.com", "Fakeoslav.Wagner", new[] { RoleType.GlobalAdmin.ToString() });
    }

    public static FederatedUser GetUserBen()
    {
        return FederatedUser.Create(BenId, "ben@fake.com", "Ben.Shapiro", new[] { RoleType.Employee.ToString() });
    }

    public static FederatedUser GetUserHabib()
    {
        return FederatedUser.Create(HabibId, "habib@arabia.com", "Ben.Shapiro", new[] { RoleType.Employee.ToString() });
    }

    public static FederatedUser GetUserPhilip()
    {
        return FederatedUser.Create(PhilipId, "philip.nov@fakemail.com", "Philip.Novak", new[] { RoleType.Employee.ToString() });
    }

    public static IEnumerable<FederatedUser> GetEmployees()
    {
        yield return GetUserBen();
        yield return GetUserHabib();
        yield return GetUserPhilip();
    }

    public static IEnumerable<FederatedUser> GetAllUsers()
    {
        return GetEmployees().Append(GetUserWithNameFakeoslav());
    }
}