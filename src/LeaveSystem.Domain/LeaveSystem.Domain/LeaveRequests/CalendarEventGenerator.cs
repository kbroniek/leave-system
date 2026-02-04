namespace LeaveSystem.Domain.LeaveRequests;

using System;
using System.Globalization;
using System.Linq;
using System.Text;

public static class CalendarEventGenerator
{
    /// <summary>
    /// Generates an RFC 5545 compliant iCalendar (.ics) file content for a leave request.
    /// </summary>
    /// <param name="leaveRequest">The leave request to generate calendar event for.</param>
    /// <param name="leaveTypeName">Optional leave type name to include in the event summary.</param>
    /// <param name="language">Optional language code for localization (currently not used but reserved for future use).</param>
    /// <returns>Byte array containing the .ics file content encoded in UTF-8.</returns>
    public static byte[] GenerateIcsFile(LeaveRequest leaveRequest, string? leaveTypeName = null, string? language = null)
    {
        var icsContent = new StringBuilder();

        // iCalendar header
        icsContent.AppendLine("BEGIN:VCALENDAR");
        icsContent.AppendLine("VERSION:2.0");
        icsContent.AppendLine("PRODID:-//Leave System//Leave Request Calendar//EN");
        icsContent.AppendLine("CALSCALE:GREGORIAN");
        icsContent.AppendLine("METHOD:PUBLISH");

        // Event
        icsContent.AppendLine("BEGIN:VEVENT");

        // Unique identifier based on leave request ID
        icsContent.AppendLine($"UID:leave-request-{leaveRequest.Id}@leavesystem");

        // Event summary (title)
        var summary = !string.IsNullOrWhiteSpace(leaveTypeName)
            ? $"Leave Request - {EscapeIcsText(leaveTypeName)}"
            : "Leave Request";
        icsContent.AppendLine($"SUMMARY:{EscapeIcsText(summary)}");

        // Event description
        var description = BuildDescription(leaveRequest, leaveTypeName);
        icsContent.AppendLine($"DESCRIPTION:{EscapeIcsText(description)}");

        // Status: TENTATIVE for pending requests
        var status = leaveRequest.Status == LeaveRequestStatus.Pending ? "TENTATIVE" : "CONFIRMED";
        icsContent.AppendLine($"STATUS:{status}");

        // All-day event: Use DATE value type (no time component)
        // Start date (DateFrom)
        var startDate = FormatIcsDate(leaveRequest.DateFrom);
        icsContent.AppendLine($"DTSTART;VALUE=DATE:{startDate}");

        // End date: For all-day events, end date should be the day after DateTo
        // (iCalendar all-day events are exclusive of the end date)
        var endDate = FormatIcsDate(leaveRequest.DateTo.AddDays(1));
        icsContent.AppendLine($"DTEND;VALUE=DATE:{endDate}");

        // Created timestamp
        var created = FormatIcsDateTime(leaveRequest.CreatedDate);
        icsContent.AppendLine($"DTSTAMP:{created}");
        icsContent.AppendLine($"CREATED:{created}");

        // Last modified timestamp
        var lastModified = FormatIcsDateTime(leaveRequest.LastModifiedDate);
        icsContent.AppendLine($"LAST-MODIFIED:{lastModified}");

        // Sequence number (starts at 0)
        icsContent.AppendLine("SEQUENCE:0");

        // Event end
        icsContent.AppendLine("END:VEVENT");

        // Calendar end
        icsContent.AppendLine("END:VCALENDAR");

        // Convert to UTF-8 byte array
        return Encoding.UTF8.GetBytes(icsContent.ToString());
    }

    private static string BuildDescription(LeaveRequest leaveRequest, string? leaveTypeName)
    {
        var description = new StringBuilder();
        description.Append($"Leave Request ID: {leaveRequest.Id}");
        description.AppendLine();
        description.Append($"Employee: {leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id}");
        description.AppendLine();
        description.Append($"Date From: {leaveRequest.DateFrom:yyyy-MM-dd}");
        description.AppendLine();
        description.Append($"Date To: {leaveRequest.DateTo:yyyy-MM-dd}");
        description.AppendLine();
        description.Append($"Duration: {FormatDuration(leaveRequest.Duration)}");
        description.AppendLine();
        description.Append($"Working Hours: {FormatDuration(leaveRequest.WorkingHours)}");

        if (!string.IsNullOrWhiteSpace(leaveTypeName))
        {
            description.AppendLine();
            description.Append($"Leave Type: {leaveTypeName}");
        }

        if (leaveRequest.Remarks.Count > 0)
        {
            var latestRemark = leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First();
            if (!string.IsNullOrWhiteSpace(latestRemark.Remarks))
            {
                description.AppendLine();
                description.Append($"Remarks: {latestRemark.Remarks}");
            }
        }

        description.AppendLine();
        description.Append($"Status: {leaveRequest.Status}");

        return description.ToString();
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.Days > 0)
        {
            return $"{duration.Days} {(duration.Days == 1 ? "day" : "days")}";
        }
        if (duration.Hours > 0)
        {
            return $"{duration.Hours} {(duration.Hours == 1 ? "hour" : "hours")}";
        }
        return $"{duration.Minutes} {(duration.Minutes == 1 ? "minute" : "minutes")}";
    }

    private static string FormatIcsDate(DateOnly date)
    {
        // Format as YYYYMMDD for DATE value type
        return date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private static string FormatIcsDateTime(DateTimeOffset dateTime)
    {
        // Format as YYYYMMDDTHHmmssZ for UTC datetime
        var utcDateTime = dateTime.ToUniversalTime();
        return utcDateTime.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
    }

    private static string EscapeIcsText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Escape special characters in iCalendar text
        // Replace backslashes first, then commas, semicolons, and newlines
        return text
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace(";", "\\;")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }
}
