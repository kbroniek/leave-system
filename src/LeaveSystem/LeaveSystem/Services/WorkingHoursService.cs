using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Services;

public class WorkingHoursService
{
    public virtual ValueTask<IEnumerable<WorkingHoursModel>> GetUserWorkingHours(string[] userIds, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken _) =>
        ValueTask.FromResult(userIds.SelectMany(id => GetWorkingHours(id, dateFrom, dateTo)));

    public virtual async ValueTask<TimeSpan> GetUserSingleWorkingHoursDuration(string userId, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken cancellationToken)
    {
        var workingHours = await GetUserWorkingHours(new string[] { userId }, dateFrom, dateTo, cancellationToken);
        var workingHoursCollection = new WorkingHoursCollection(workingHours);
        return workingHoursCollection.GetDuration(userId, dateFrom, dateTo);
    }
    private static IEnumerable<WorkingHoursModel> GetWorkingHours(string userId, DateTimeOffset dateFrom, DateTimeOffset dateTo) =>
        new WorkingHoursModel[] { new WorkingHoursModel(userId, dateFrom, dateTo, WorkingHoursCollection.DefaultWorkingHours) };
}
