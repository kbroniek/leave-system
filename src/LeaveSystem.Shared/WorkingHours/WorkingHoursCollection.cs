namespace LeaveSystem.Shared.WorkingHours;

public class WorkingHoursCollection
{
    private readonly IEnumerable<WorkingHoursModel> workingHoursCollection;
    private readonly static TimeSpan defaultWorkingHours = TimeSpan.FromHours(8);

    public WorkingHoursCollection(IEnumerable<WorkingHoursModel> workingHoursCollection)
    {
        this.workingHoursCollection = workingHoursCollection;
    }

    public TimeSpan GetDuration() => GetDuration(workingHoursCollection);
    public TimeSpan GetDuration(DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        var workingHoursFound = workingHoursCollection.Where(w =>
            w.DateFrom <= dateFrom && w.DateTo >= dateTo);
        return GetDuration(workingHoursFound);
    }
    public TimeSpan GetDuration(string userEmail, DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        var workingHoursFound = workingHoursCollection.Where(w =>
            string.Equals(w.UserEmail, userEmail, StringComparison.CurrentCultureIgnoreCase) &&
            w.DateFrom <= dateFrom && w.DateTo >= dateTo);
        return GetDuration(workingHoursFound);
    }

    private static TimeSpan GetDuration(IEnumerable<WorkingHoursModel> workingHours)
    {
        if (!workingHours.Any())
        {
            return defaultWorkingHours;
            // TODO: Not sure if I should throw an error.
            //throw new InvalidOperationException($"Cannot find any working hours that match date from: {dateFrom:O}, date to: {dateTo:O}");
        }
        if (workingHours.GroupBy(w => w.Duration).Count() > 1)
        {
            return defaultWorkingHours;
            // TODO: Not sure if I should throw an error.
            //throw new InvalidOperationException($"Found more than one working hours entry with different duration that match date from: {dateFrom:O}, date to: {dateTo:O}");
        }
        return workingHours.First().Duration;
    }
}
