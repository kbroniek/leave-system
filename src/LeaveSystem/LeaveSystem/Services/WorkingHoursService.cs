using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Services;

public class WorkingHoursService
{
    public virtual ValueTask<IEnumerable<WorkingHoursModel>> GetUserWorkingHours(string[] userEmails, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken _) =>
        ValueTask.FromResult(userEmails.SelectMany(email => GetWorkingHours(email, dateFrom, dateTo)));

    public virtual async ValueTask<TimeSpan> GetUserSingleWorkingHoursDuration(string userEmail, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken cancellationToken)
    {
        var workingHours = await GetUserWorkingHours(new string[] { userEmail }, dateFrom, dateTo, cancellationToken);
        var workingHoursCollection = new WorkingHoursCollection(workingHours);
        return workingHoursCollection.GetDuration(userEmail, dateFrom, dateTo);
    }
    private static IEnumerable<WorkingHoursModel> GetWorkingHours(string userEmail, DateTimeOffset dateFrom, DateTimeOffset dateTo) =>
        new WorkingHoursModel[] { new WorkingHoursModel(userEmail, dateFrom, dateTo, WorkingHoursCollection.DefaultWorkingHours) };
}
