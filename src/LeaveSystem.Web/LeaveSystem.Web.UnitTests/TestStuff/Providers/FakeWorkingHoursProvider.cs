using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public class FakeWorkingHoursProvider
{
    public static IEnumerable<string> GetUserIds() => FakeUserProvider.GetEmployees().Select(e => e.Id);

    public static IEnumerable<WorkingHoursModel> GetAll()
    {
        return new[]
        {
            new WorkingHoursModel(
                FakeUserProvider.PhilipId, DateTimeOffsetExtensions.Create(2020, 5,1),
                DateTimeOffsetExtensions.Create(2025, 5, 1), TimeSpan.FromHours(8)),
            new WorkingHoursModel(
                FakeUserProvider.BenId, DateTimeOffsetExtensions.Create(2018, 5,1),
                DateTimeOffsetExtensions.Create(2022, 5, 1), TimeSpan.FromHours(8)),
            new WorkingHoursModel(
                FakeUserProvider.HabibId, DateTimeOffsetExtensions.Create(2020, 2,2),
                DateTimeOffsetExtensions.Create(2027, 5, 7), TimeSpan.FromHours(8)),
        };
    }

    public static WorkingHoursCollection GetAllAsWorkingHoursCollection() =>
        new WorkingHoursCollection(GetAll());
}