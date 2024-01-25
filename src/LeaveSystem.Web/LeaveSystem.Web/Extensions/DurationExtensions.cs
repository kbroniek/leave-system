namespace LeaveSystem.Web.Extensions;

public static class DurationExtensions
{
	public static string GetReadableTimeSpan(this TimeSpan duration, TimeSpan workingHours)
	{
		var days = workingHours.TotalHours == 0 ?
			duration.TotalHours :
			duration.TotalHours / workingHours.TotalHours;
		if (days == 0)
		{
			return "0d";
		}
		var result = "";
		var isNegative = days < 0;
		if (isNegative)
		{
			days = -days;
		}
		var hours = days * workingHours.TotalHours;
		var modHours = hours % workingHours.TotalHours;
		if (modHours > 0)
		{
			result = Math.Round(modHours * 100) / 100 + "h ";
		}
		var modDays = (hours - modHours) / workingHours.TotalHours;
		if (modDays > 0)
		{
			result = Math.Round(modDays * 100) / 100 + "d " + result;
		}
		if (isNegative)
		{
			result = "-" + result;
		}
		return result;
	}

	public static int GetDurationDays(this TimeSpan duration, TimeSpan workingHours)
	{
		return (int)(workingHours.TotalHours == 0 ?
			duration.TotalHours :
			duration.TotalHours / workingHours.TotalHours);
	}

	public static TimeSpan GetDurationFromDays(int days, TimeSpan workingHours)
	{
		return workingHours * days;
	}
}

