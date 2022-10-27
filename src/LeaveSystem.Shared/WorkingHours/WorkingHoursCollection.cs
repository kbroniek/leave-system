namespace LeaveSystem.Shared.WorkingHours;

public class WorkingHoursCollection
{
    private readonly IEnumerable<WorkingHoursModel> workingHoursCollection;

    public WorkingHoursCollection(IEnumerable<WorkingHoursModel> workingHoursCollection)
    {
        this.workingHoursCollection = workingHoursCollection;
    }

    public TimeSpan GetDuration(string userEmail, DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        var workingHoursFound = workingHoursCollection.Where(w =>
            string.Equals(w.UserEmail, userEmail, StringComparison.CurrentCultureIgnoreCase) &&
            w.DateFrom <= dateFrom && w.DateTo >= dateTo);
        if (!workingHoursFound.Any())
        {
            throw new InvalidOperationException($"Cannot find any working hours that match date from: {dateFrom:O}, date to: {dateTo:O}");
        }
        if (workingHoursFound.GroupBy(w => w.Duration).Count() > 1)
        {
            throw new InvalidOperationException($"Found more than one working hours entry with different duration that match date from: {dateFrom:O}, date to: {dateTo:O}");
        }
        return workingHoursFound.First().Duration;
    }
}
