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
        language = NormalizeLanguage(language);
        var status = leaveRequest.Status == LeaveRequestStatus.Pending ? "TENTATIVE" : "CONFIRMED";
        var description = BuildDescription(leaveRequest, leaveTypeName, baseUrl, language, includeCancellationMessage: false);

        return BuildIcsFile(
            leaveRequest,
            leaveTypeName,
            language,
            baseUrl,
            method: "PUBLISH",
            status,
            description,
            dtStamp: leaveRequest.CreatedDate,
            sequence: 0);
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
        language = NormalizeLanguage(language);
        var description = BuildDescription(leaveRequest, leaveTypeName, baseUrl, language, includeCancellationMessage: true);

        return BuildIcsFile(
            leaveRequest,
            leaveTypeName,
            language,
            baseUrl,
            method: "CANCEL",
            status: "CANCELLED",
            description,
            dtStamp: DateTimeOffset.UtcNow,
            sequence: 1);
    }

    private static byte[] BuildIcsFile(
        LeaveRequest leaveRequest,
        string? leaveTypeName,
        string language,
        string? baseUrl,
        string method,
        string status,
        string description,
        DateTimeOffset dtStamp,
        int sequence)
    {
        var icsContent = new StringBuilder();

        AppendCalendarHeader(icsContent, method);
        AppendEventHeader(icsContent, leaveRequest);
        AppendEventSummary(icsContent, leaveTypeName, language);
        AppendEventDescription(icsContent, description);
        AppendEventUrl(icsContent, baseUrl, leaveRequest.Id);
        AppendEventStatus(icsContent, status);
        AppendEventDates(icsContent, leaveRequest.DateFrom, leaveRequest.DateTo);
        AppendEventTimestamps(icsContent, leaveRequest.CreatedDate, leaveRequest.LastModifiedDate, dtStamp);
        AppendEventSequence(icsContent, sequence);
        AppendEventFooter(icsContent);
        AppendCalendarFooter(icsContent);

        return Encoding.UTF8.GetBytes(icsContent.ToString());
    }

    private static void AppendCalendarHeader(StringBuilder icsContent, string method)
    {
        icsContent.AppendLine("BEGIN:VCALENDAR");
        icsContent.AppendLine("VERSION:2.0");
        icsContent.AppendLine("PRODID:-//Leave System//Leave Request Calendar//EN");
        icsContent.AppendLine("CALSCALE:GREGORIAN");
        icsContent.AppendLine($"METHOD:{method}");
    }

    private static void AppendEventHeader(StringBuilder icsContent, LeaveRequest leaveRequest)
    {
        icsContent.AppendLine("BEGIN:VEVENT");
        icsContent.AppendLine($"UID:leave-request-{leaveRequest.Id}@leavesystem");
    }

    private static void AppendEventSummary(StringBuilder icsContent, string? leaveTypeName, string language)
    {
        var summary = BuildSummary(leaveTypeName, language);
        icsContent.AppendLine($"SUMMARY:{EscapeIcsText(summary)}");
    }

    private static void AppendEventDescription(StringBuilder icsContent, string description)
    {
        icsContent.AppendLine($"DESCRIPTION:{EscapeIcsText(description)}");
    }

    private static void AppendEventUrl(StringBuilder icsContent, string? baseUrl, Guid leaveRequestId)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            var leaveRequestUrl = BuildLeaveRequestUrl(baseUrl, leaveRequestId);
            icsContent.AppendLine($"URL:{EscapeIcsText(leaveRequestUrl)}");
        }
    }

    private static void AppendEventStatus(StringBuilder icsContent, string status)
    {
        icsContent.AppendLine($"STATUS:{status}");
    }

    private static void AppendEventDates(StringBuilder icsContent, DateOnly dateFrom, DateOnly dateTo)
    {
        var startDate = FormatIcsDate(dateFrom);
        icsContent.AppendLine($"DTSTART;VALUE=DATE:{startDate}");

        // End date: For all-day events, end date should be the day after DateTo
        // (iCalendar all-day events are exclusive of the end date)
        var endDate = FormatIcsDate(dateTo.AddDays(1));
        icsContent.AppendLine($"DTEND;VALUE=DATE:{endDate}");
    }

    private static void AppendEventTimestamps(StringBuilder icsContent, DateTimeOffset createdDate, DateTimeOffset lastModifiedDate, DateTimeOffset dtStamp)
    {
        icsContent.AppendLine($"DTSTAMP:{FormatIcsDateTime(dtStamp)}");
        icsContent.AppendLine($"CREATED:{FormatIcsDateTime(createdDate)}");
        icsContent.AppendLine($"LAST-MODIFIED:{FormatIcsDateTime(lastModifiedDate)}");
    }

    private static void AppendEventSequence(StringBuilder icsContent, int sequence)
    {
        icsContent.AppendLine($"SEQUENCE:{sequence}");
    }

    private static void AppendEventFooter(StringBuilder icsContent)
    {
        icsContent.AppendLine("END:VEVENT");
    }

    private static void AppendCalendarFooter(StringBuilder icsContent)
    {
        icsContent.AppendLine("END:VCALENDAR");
    }

    private static string BuildSummary(string? leaveTypeName, string language)
    {
        var leaveRequestLabel = GetTranslation("Leave Request", language);
        return !string.IsNullOrWhiteSpace(leaveTypeName)
            ? $"{leaveRequestLabel} - {leaveTypeName}"
            : leaveRequestLabel;
    }

    private static string BuildLeaveRequestUrl(string baseUrl, Guid leaveRequestId)
    {
        return $"{baseUrl.TrimEnd('/')}/details/{leaveRequestId}";
    }

    private static string BuildDescription(LeaveRequest leaveRequest, string? leaveTypeName, string? baseUrl = null, string language = "en-US", bool includeCancellationMessage = false)
    {
        var description = new StringBuilder();

        AppendDescriptionField(description, "Leave Request ID", leaveRequest.Id.ToString(), language);
        AppendDescriptionField(description, "Employee", leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id, language);
        AppendDescriptionField(description, "Date From", leaveRequest.DateFrom.ToString("yyyy-MM-dd"), language);
        AppendDescriptionField(description, "Date To", leaveRequest.DateTo.ToString("yyyy-MM-dd"), language);
        AppendDescriptionField(description, "Duration", FormatDuration(leaveRequest.Duration, language), language);

        var workingHoursLabel = GetTranslation("Working Hours", language);
        description.Append($"{workingHoursLabel}: {FormatDuration(leaveRequest.WorkingHours, language)}");

        if (!string.IsNullOrWhiteSpace(leaveTypeName))
        {
            description.AppendLine();
            AppendDescriptionField(description, "Leave Type", leaveTypeName, language, addNewLine: false);
        }

        AppendRemarks(description, leaveRequest, language);

        description.AppendLine();
        AppendDescriptionField(description, "Status", leaveRequest.Status.ToString(), language, addNewLine: false);

        if (includeCancellationMessage)
        {
            description.AppendLine();
            var cancellationMessage = GetTranslation("This leave request has been cancelled or rejected. Please remove this event from your calendar.", language);
            description.Append(cancellationMessage);
        }

        AppendViewLink(description, baseUrl, leaveRequest.Id, language);

        return description.ToString();
    }

    private static void AppendDescriptionField(StringBuilder description, string labelKey, string value, string language, bool addNewLine = true)
    {
        var label = GetTranslation(labelKey, language);
        description.Append($"{label}: {value}");
        if (addNewLine)
        {
            description.AppendLine();
        }
    }

    private static void AppendRemarks(StringBuilder description, LeaveRequest leaveRequest, string language)
    {
        if (leaveRequest.Remarks.Count > 0)
        {
            var latestRemark = leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First();
            if (!string.IsNullOrWhiteSpace(latestRemark.Remarks))
            {
                description.AppendLine();
                AppendDescriptionField(description, "Remarks", latestRemark.Remarks, language, addNewLine: false);
            }
        }
    }

    private static void AppendViewLink(StringBuilder description, string? baseUrl, Guid leaveRequestId, string language)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            description.AppendLine();
            description.AppendLine();
            var viewLabel = GetTranslation("View Leave Request", language);
            var leaveRequestUrl = BuildLeaveRequestUrl(baseUrl, leaveRequestId);
            description.Append($"{viewLabel}: {leaveRequestUrl}");
        }
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
