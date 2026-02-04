namespace LeaveSystem.Domain.LeaveRequests;

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using LeaveSystem.Shared.LeaveRequests;

public static class CalendarEventGenerator
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["en-US"] = new Dictionary<string, string>
        {
            ["Leave Request"] = "Leave",
            ["Leave Request ID"] = "Leave Request ID",
            ["Employee"] = "Employee",
            ["Date From"] = "Date From",
            ["Date To"] = "Date To",
            ["Duration"] = "Duration",
            ["Working Hours"] = "Working Hours",
            ["Leave Type"] = "Leave Type",
            ["Remarks"] = "Remarks",
            ["Status"] = "Status",
            ["View Leave Request"] = "View Leave Request",
            ["This leave request has been cancelled or rejected. Please remove this event from your calendar."] = "This leave request has been cancelled or rejected. Please remove this event from your calendar.",
            ["day"] = "day",
            ["days"] = "days",
            ["hour"] = "hour",
            ["hours"] = "hours",
            ["minute"] = "minute",
            ["minutes"] = "minutes"
        },
        ["pl-PL"] = new Dictionary<string, string>
        {
            ["Leave Request"] = "Urlop",
            ["Leave Request ID"] = "ID Wniosku",
            ["Employee"] = "Pracownik",
            ["Date From"] = "Data Od",
            ["Date To"] = "Data Do",
            ["Duration"] = "Czas Trwania",
            ["Working Hours"] = "Godziny Robocze",
            ["Leave Type"] = "Typ Urlopu",
            ["Remarks"] = "Uwagi",
            ["Status"] = "Status",
            ["View Leave Request"] = "Zobacz Wniosek o Urlop",
            ["This leave request has been cancelled or rejected. Please remove this event from your calendar."] = "Ten wniosek o urlop został anulowany lub odrzucony. Usuń to wydarzenie z kalendarza.",
            ["day"] = "dzień",
            ["days"] = "dni",
            ["hour"] = "godzina",
            ["hours"] = "godzin",
            ["minute"] = "minuta",
            ["minutes"] = "minut"
        }
    };

    /// <summary>
    /// Generates an RFC 5545 compliant iCalendar (.ics) file content for a leave request.
    /// </summary>
    /// <param name="leaveRequest">The leave request to generate calendar event for.</param>
    /// <param name="leaveTypeName">Optional leave type name to include in the event summary.</param>
    /// <param name="language">Optional language code for localization (en-US, pl-PL).</param>
    /// <param name="baseUrl">Optional base URL for creating a link to view the leave request details.</param>
    /// <returns>Byte array containing the .ics file content encoded in UTF-8.</returns>
    public static byte[] GenerateIcsFile(LeaveRequest leaveRequest, string? leaveTypeName = null, string? language = null, string? baseUrl = null)
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

        // Normalize language
        language = NormalizeLanguage(language);

        // Event summary (title)
        var leaveRequestLabel = GetTranslation("Leave Request", language);
        var summary = !string.IsNullOrWhiteSpace(leaveTypeName)
            ? $"{leaveRequestLabel} - {EscapeIcsText(leaveTypeName)}"
            : leaveRequestLabel;
        icsContent.AppendLine($"SUMMARY:{EscapeIcsText(summary)}");

        // Event description
        var description = BuildDescription(leaveRequest, leaveTypeName, baseUrl, language);
        icsContent.AppendLine($"DESCRIPTION:{EscapeIcsText(description)}");

        // URL to view leave request details (if baseUrl is provided)
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            var leaveRequestUrl = $"{baseUrl.TrimEnd('/')}/details/{leaveRequest.Id}";
            icsContent.AppendLine($"URL:{EscapeIcsText(leaveRequestUrl)}");
        }

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

    /// <summary>
    /// Generates an RFC 5545 compliant cancellation iCalendar (.ics) file content for a leave request.
    /// This is used to remove or cancel an existing calendar event.
    /// </summary>
    /// <param name="leaveRequest">The leave request to generate cancellation event for.</param>
    /// <param name="leaveTypeName">Optional leave type name to include in the event summary.</param>
    /// <param name="language">Optional language code for localization (currently not used but reserved for future use).</param>
    /// <param name="baseUrl">Optional base URL for creating a link to view the leave request details.</param>
    /// <returns>Byte array containing the cancellation .ics file content encoded in UTF-8.</returns>
    public static byte[] GenerateCancellationIcsFile(LeaveRequest leaveRequest, string? leaveTypeName = null, string? language = null, string? baseUrl = null)
    {
        var icsContent = new StringBuilder();

        // iCalendar header with METHOD:CANCEL for cancellation
        icsContent.AppendLine("BEGIN:VCALENDAR");
        icsContent.AppendLine("VERSION:2.0");
        icsContent.AppendLine("PRODID:-//Leave System//Leave Request Calendar//EN");
        icsContent.AppendLine("CALSCALE:GREGORIAN");
        icsContent.AppendLine("METHOD:CANCEL");

        // Event
        icsContent.AppendLine("BEGIN:VEVENT");

        // Unique identifier - MUST be the same as the original event for calendar clients to match and remove it
        icsContent.AppendLine($"UID:leave-request-{leaveRequest.Id}@leavesystem");

        // Normalize language
        language = NormalizeLanguage(language);

        // Event summary (title) - same as original
        var leaveRequestLabel = GetTranslation("Leave Request", language);
        var summary = !string.IsNullOrWhiteSpace(leaveTypeName)
            ? $"{leaveRequestLabel} - {EscapeIcsText(leaveTypeName)}"
            : leaveRequestLabel;
        icsContent.AppendLine($"SUMMARY:{EscapeIcsText(summary)}");

        // Event description - indicate cancellation
        var description = BuildCancellationDescription(leaveRequest, leaveTypeName, baseUrl, language);
        icsContent.AppendLine($"DESCRIPTION:{EscapeIcsText(description)}");

        // URL to view leave request details (if baseUrl is provided)
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            var leaveRequestUrl = $"{baseUrl.TrimEnd('/')}/details/{leaveRequest.Id}";
            icsContent.AppendLine($"URL:{EscapeIcsText(leaveRequestUrl)}");
        }

        // Status: CANCELLED for cancelled/rejected events
        icsContent.AppendLine("STATUS:CANCELLED");

        // All-day event: Use DATE value type (same as original)
        var startDate = FormatIcsDate(leaveRequest.DateFrom);
        icsContent.AppendLine($"DTSTART;VALUE=DATE:{startDate}");

        var endDate = FormatIcsDate(leaveRequest.DateTo.AddDays(1));
        icsContent.AppendLine($"DTEND;VALUE=DATE:{endDate}");

        // Updated timestamps
        var now = FormatIcsDateTime(DateTimeOffset.UtcNow);
        icsContent.AppendLine($"DTSTAMP:{now}");
        icsContent.AppendLine($"CREATED:{FormatIcsDateTime(leaveRequest.CreatedDate)}");
        icsContent.AppendLine($"LAST-MODIFIED:{FormatIcsDateTime(leaveRequest.LastModifiedDate)}");

        // Sequence number - increment from original (use 1 as we don't track sequence)
        icsContent.AppendLine("SEQUENCE:1");

        // Event end
        icsContent.AppendLine("END:VEVENT");

        // Calendar end
        icsContent.AppendLine("END:VCALENDAR");

        // Convert to UTF-8 byte array
        return Encoding.UTF8.GetBytes(icsContent.ToString());
    }

    private static string BuildDescription(LeaveRequest leaveRequest, string? leaveTypeName, string? baseUrl = null, string language = "en-US")
    {
        var description = new StringBuilder();

        var leaveRequestIdLabel = GetTranslation("Leave Request ID", language);
        description.Append($"{leaveRequestIdLabel}: {leaveRequest.Id}");
        description.AppendLine();

        var employeeLabel = GetTranslation("Employee", language);
        description.Append($"{employeeLabel}: {leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id}");
        description.AppendLine();

        var dateFromLabel = GetTranslation("Date From", language);
        description.Append($"{dateFromLabel}: {leaveRequest.DateFrom:yyyy-MM-dd}");
        description.AppendLine();

        var dateToLabel = GetTranslation("Date To", language);
        description.Append($"{dateToLabel}: {leaveRequest.DateTo:yyyy-MM-dd}");
        description.AppendLine();

        var durationLabel = GetTranslation("Duration", language);
        description.Append($"{durationLabel}: {FormatDuration(leaveRequest.Duration, language)}");
        description.AppendLine();

        var workingHoursLabel = GetTranslation("Working Hours", language);
        description.Append($"{workingHoursLabel}: {FormatDuration(leaveRequest.WorkingHours, language)}");

        if (!string.IsNullOrWhiteSpace(leaveTypeName))
        {
            description.AppendLine();
            var leaveTypeLabel = GetTranslation("Leave Type", language);
            description.Append($"{leaveTypeLabel}: {leaveTypeName}");
        }

        if (leaveRequest.Remarks.Count > 0)
        {
            var latestRemark = leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First();
            if (!string.IsNullOrWhiteSpace(latestRemark.Remarks))
            {
                description.AppendLine();
                var remarksLabel = GetTranslation("Remarks", language);
                description.Append($"{remarksLabel}: {latestRemark.Remarks}");
            }
        }

        description.AppendLine();
        var statusLabel = GetTranslation("Status", language);
        description.Append($"{statusLabel}: {leaveRequest.Status}");

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            description.AppendLine();
            description.AppendLine();
            var viewLabel = GetTranslation("View Leave Request", language);
            description.Append($"{viewLabel}: {baseUrl.TrimEnd('/')}/details/{leaveRequest.Id}");
        }

        return description.ToString();
    }

    private static string BuildCancellationDescription(LeaveRequest leaveRequest, string? leaveTypeName, string? baseUrl = null, string language = "en-US")
    {
        var description = new StringBuilder();

        var leaveRequestIdLabel = GetTranslation("Leave Request ID", language);
        description.Append($"{leaveRequestIdLabel}: {leaveRequest.Id}");
        description.AppendLine();

        var employeeLabel = GetTranslation("Employee", language);
        description.Append($"{employeeLabel}: {leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id}");
        description.AppendLine();

        var dateFromLabel = GetTranslation("Date From", language);
        description.Append($"{dateFromLabel}: {leaveRequest.DateFrom:yyyy-MM-dd}");
        description.AppendLine();

        var dateToLabel = GetTranslation("Date To", language);
        description.Append($"{dateToLabel}: {leaveRequest.DateTo:yyyy-MM-dd}");
        description.AppendLine();

        var durationLabel = GetTranslation("Duration", language);
        description.Append($"{durationLabel}: {FormatDuration(leaveRequest.Duration, language)}");
        description.AppendLine();

        var workingHoursLabel = GetTranslation("Working Hours", language);
        description.Append($"{workingHoursLabel}: {FormatDuration(leaveRequest.WorkingHours, language)}");

        if (!string.IsNullOrWhiteSpace(leaveTypeName))
        {
            description.AppendLine();
            var leaveTypeLabel = GetTranslation("Leave Type", language);
            description.Append($"{leaveTypeLabel}: {leaveTypeName}");
        }

        if (leaveRequest.Remarks.Count > 0)
        {
            var latestRemark = leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First();
            if (!string.IsNullOrWhiteSpace(latestRemark.Remarks))
            {
                description.AppendLine();
                var remarksLabel = GetTranslation("Remarks", language);
                description.Append($"{remarksLabel}: {latestRemark.Remarks}");
            }
        }

        description.AppendLine();
        var statusLabel = GetTranslation("Status", language);
        description.Append($"{statusLabel}: {leaveRequest.Status}");
        description.AppendLine();

        var cancellationMessage = GetTranslation("This leave request has been cancelled or rejected. Please remove this event from your calendar.", language);
        description.Append(cancellationMessage);

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            description.AppendLine();
            description.AppendLine();
            var viewLabel = GetTranslation("View Leave Request", language);
            description.Append($"{viewLabel}: {baseUrl.TrimEnd('/')}/details/{leaveRequest.Id}");
        }

        return description.ToString();
    }

    private static string FormatDuration(TimeSpan duration, string language = "en-US")
    {
        if (duration.Days > 0)
        {
            var dayKey = duration.Days == 1 ? "day" : "days";
            var dayWord = GetTranslation(dayKey, language);
            return $"{duration.Days} {dayWord}";
        }
        if (duration.Hours > 0)
        {
            var hourKey = duration.Hours == 1 ? "hour" : "hours";
            var hourWord = GetTranslation(hourKey, language);
            return $"{duration.Hours} {hourWord}";
        }
        var minuteKey = duration.Minutes == 1 ? "minute" : "minutes";
        var minuteWord = GetTranslation(minuteKey, language);
        return $"{duration.Minutes} {minuteWord}";
    }

    private static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "en-US";
        }

        language = language.Trim();
        return language switch
        {
            "pl" or "pl-PL" or "Polish" => "pl-PL",
            "en" or "en-US" or "English" => "en-US",
            _ => "en-US"
        };
    }

    private static string GetTranslation(string key, string language)
    {
        if (Translations.TryGetValue(language, out var languageTranslations))
        {
            if (languageTranslations.TryGetValue(key, out var translation))
            {
                return translation;
            }
        }

        return key;
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
