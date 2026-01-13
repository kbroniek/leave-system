namespace LeaveSystem.Domain.LeaveRequests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

public static class EmailTemplates
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["en-US"] = new Dictionary<string, string>
        {
            ["Accepted"] = "Accepted",
            ["Rejected"] = "Rejected",
            ["accepted"] = "accepted",
            ["rejected"] = "rejected",
            ["day"] = "day",
            ["days"] = "days",
            ["hour"] = "hour",
            ["hours"] = "hours",
            ["minute"] = "minute",
            ["minutes"] = "minutes"
        },
        ["pl-PL"] = new Dictionary<string, string>
        {
            ["Accepted"] = "Zaakceptowany",
            ["Rejected"] = "Odrzucony",
            ["accepted"] = "zaakceptowany",
            ["rejected"] = "odrzucony",
            ["day"] = "dzień",
            ["days"] = "dni",
            ["hour"] = "godzina",
            ["hours"] = "godzin",
            ["minute"] = "minuta",
            ["minutes"] = "minut"
        }
    };

    public static string CreateLeaveRequestCreatedEmail(LeaveRequest leaveRequest, string? leaveTypeName = null, string? language = null)
    {
        language = NormalizeLanguage(language);
        var template = LoadTemplate("LeaveRequestCreated", language);
        return ReplacePlaceholders(template, new Dictionary<string, string>
        {
            ["LEAVE_REQUEST_ID"] = leaveRequest.Id.ToString(),
            ["EMPLOYEE_NAME"] = EscapeHtml(leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id),
            ["DATE_FROM"] = leaveRequest.DateFrom.ToString("yyyy-MM-dd"),
            ["DATE_TO"] = leaveRequest.DateTo.ToString("yyyy-MM-dd"),
            ["DURATION"] = FormatDuration(leaveRequest.Duration, language),
            ["WORKING_HOURS"] = FormatDuration(leaveRequest.WorkingHours, language),
            ["LEAVE_TYPE_ROW"] = !string.IsNullOrWhiteSpace(leaveTypeName)
                ? GetLeaveTypeRow(leaveTypeName, language)
                : string.Empty,
            ["REMARKS_ROW"] = leaveRequest.Remarks.Count != 0
                ? GetRemarksRow(leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First().Remarks, language)
                : string.Empty,
            ["STATUS"] = leaveRequest.Status.ToString(),
            ["CREATED_DATE"] = leaveRequest.CreatedDate.ToString("yyyy-MM-dd HH:mm")
        });
    }

    public static string CreateLeaveRequestDecisionEmail(LeaveRequest leaveRequest, string decision, string decisionMakerName, string? leaveTypeName = null, string? language = null)
    {
        language = NormalizeLanguage(language);
        var template = LoadTemplate("LeaveRequestDecision", language);
        var decisionLower = decision.ToLower();
        var translatedDecision = Translations[language].TryGetValue(decision, out var trans) ? trans : decision;
        var translatedDecisionLower = Translations[language].TryGetValue(decisionLower, out var transLower) ? transLower : decisionLower;
        var headerColor = decision == "Accepted" ? "#107c10" : "#d13438";

        return ReplacePlaceholders(template, new Dictionary<string, string>
        {
            ["HEADER_COLOR"] = headerColor,
            ["DECISION"] = translatedDecision,
            ["DECISION_LOWER"] = translatedDecisionLower,
            ["DECISION_MAKER_NAME"] = EscapeHtml(decisionMakerName),
            ["LEAVE_REQUEST_ID"] = leaveRequest.Id.ToString(),
            ["DATE_FROM"] = leaveRequest.DateFrom.ToString("yyyy-MM-dd"),
            ["DATE_TO"] = leaveRequest.DateTo.ToString("yyyy-MM-dd"),
            ["DURATION"] = FormatDuration(leaveRequest.Duration, language),
            ["WORKING_HOURS"] = FormatDuration(leaveRequest.WorkingHours, language),
            ["LEAVE_TYPE_ROW"] = !string.IsNullOrWhiteSpace(leaveTypeName)
                ? GetLeaveTypeRow(leaveTypeName, language)
                : string.Empty,
            ["REMARKS_ROW"] = leaveRequest.Remarks.Any()
                ? GetRemarksRow(leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First().Remarks, language)
                : string.Empty,
            ["STATUS"] = leaveRequest.Status.ToString(),
            ["DECISION_DATE"] = leaveRequest.LastModifiedDate.ToString("yyyy-MM-dd HH:mm")
        });
    }

    public static string CreateLeaveRequestCanceledEmail(LeaveRequest leaveRequest, string? leaveTypeName = null, string? language = null)
    {
        language = NormalizeLanguage(language);
        var template = LoadTemplate("LeaveRequestCanceled", language);
        return ReplacePlaceholders(template, new Dictionary<string, string>
        {
            ["LEAVE_REQUEST_ID"] = leaveRequest.Id.ToString(),
            ["EMPLOYEE_NAME"] = EscapeHtml(leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id),
            ["DATE_FROM"] = leaveRequest.DateFrom.ToString("yyyy-MM-dd"),
            ["DATE_TO"] = leaveRequest.DateTo.ToString("yyyy-MM-dd"),
            ["DURATION"] = FormatDuration(leaveRequest.Duration, language),
            ["WORKING_HOURS"] = FormatDuration(leaveRequest.WorkingHours, language),
            ["LEAVE_TYPE_ROW"] = !string.IsNullOrWhiteSpace(leaveTypeName)
                ? GetLeaveTypeRow(leaveTypeName, language)
                : string.Empty,
            ["REMARKS_ROW"] = leaveRequest.Remarks.Count != 0
                ? GetRemarksRow(leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First().Remarks, language)
                : string.Empty,
            ["STATUS"] = leaveRequest.Status.ToString(),
            ["CANCELED_DATE"] = leaveRequest.LastModifiedDate.ToString("yyyy-MM-dd HH:mm")
        });
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

    private static string LoadTemplate(string templateName, string language)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // .NET SDK converts hyphens in folder names to underscores in embedded resource names
        var languageForResource = language.Replace("-", "_");
        var resourceName = $"LeaveSystem.Domain.LeaveRequests.Templates.{languageForResource}.{templateName}.html";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        // Fallback to en-US if template not found
        if (language != "en-US")
        {
            return LoadTemplate(templateName, "en-US");
        }

        // If still not found, list available resources for debugging
        var availableResources = assembly.GetManifestResourceNames();
        var availableResourcesList = string.Join(", ", availableResources);
        throw new InvalidOperationException(
            $"Template '{templateName}' for language '{language}' not found. " +
            $"Tried resource name: {resourceName}. " +
            $"Available resources: {availableResourcesList}");
    }

    private static string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
    {
        var result = template;
        foreach (var placeholder in placeholders)
        {
            result = result.Replace($"{{{placeholder.Key}}}", placeholder.Value);
        }
        return result;
    }

    private static string GetLeaveTypeRow(string leaveTypeName, string language)
    {
        var label = language == "pl-PL" ? "Typ Urlopu:" : "Leave Type:";
        return $"<div class='info-row'><span class='label'>{label}</span> <span class='value'>{EscapeHtml(leaveTypeName)}</span></div>";
    }

    private static string GetRemarksRow(string remarks, string language)
    {
        var label = language == "pl-PL" ? "Uwagi:" : "Remarks:";
        return $"<div class='info-row'><span class='label'>{label}</span> <span class='value'>{EscapeHtml(remarks)}</span></div>";
    }

    private static string EscapeHtml(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    private static string FormatDuration(TimeSpan duration, string language)
    {
        var translations = Translations[language];
        var dayKey = duration.Days == 1 ? "day" : "days";
        var hourKey = duration.Hours == 1 ? "hour" : "hours";
        var minuteKey = duration.Minutes == 1 ? "minute" : "minutes";

        if (duration.Days > 0)
        {
            var dayWord = translations.TryGetValue(dayKey, out var dayTrans) ? dayTrans : dayKey;
            return $"{duration.Days} {dayWord}";
        }
        if (duration.Hours > 0)
        {
            var hourWord = translations.TryGetValue(hourKey, out var hourTrans) ? hourTrans : hourKey;
            return $"{duration.Hours} {hourWord}";
        }
        var minuteWord = translations.TryGetValue(minuteKey, out var minTrans) ? minTrans : minuteKey;
        return $"{duration.Minutes} {minuteWord}";
    }
}
