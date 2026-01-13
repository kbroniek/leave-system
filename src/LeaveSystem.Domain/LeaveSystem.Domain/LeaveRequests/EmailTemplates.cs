namespace LeaveSystem.Domain.LeaveRequests;

using System;
using System.Linq;
using System.Text;

public static class EmailTemplates
{
    public static string CreateLeaveRequestCreatedEmail(LeaveRequest leaveRequest, string? leaveTypeName = null)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head><meta charset='utf-8'><style>");
        html.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
        html.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
        html.AppendLine(".header { background-color: #0078d4; color: white; padding: 20px; text-align: center; }");
        html.AppendLine(".content { background-color: #f9f9f9; padding: 20px; margin-top: 20px; }");
        html.AppendLine(".info-row { margin: 10px 0; }");
        html.AppendLine(".label { font-weight: bold; color: #666; }");
        html.AppendLine(".value { color: #333; }");
        html.AppendLine(".footer { margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }");
        html.AppendLine("</style></head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class='container'>");
        html.AppendLine("<div class='header'><h1>New Leave Request</h1></div>");
        html.AppendLine("<div class='content'>");
        html.AppendLine("<p>A new leave request has been created and requires your review.</p>");
        html.AppendLine("<div class='info-row'><span class='label'>Leave Request ID:</span> <span class='value'>" + leaveRequest.Id + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Employee:</span> <span class='value'>" + EscapeHtml(leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id) + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Date From:</span> <span class='value'>" + leaveRequest.DateFrom.ToString("yyyy-MM-dd") + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Date To:</span> <span class='value'>" + leaveRequest.DateTo.ToString("yyyy-MM-dd") + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Duration:</span> <span class='value'>" + FormatDuration(leaveRequest.Duration) + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Working Hours:</span> <span class='value'>" + FormatDuration(leaveRequest.WorkingHours) + "</span></div>");
        if (!string.IsNullOrWhiteSpace(leaveTypeName))
        {
            html.AppendLine("<div class='info-row'><span class='label'>Leave Type:</span> <span class='value'>" + EscapeHtml(leaveTypeName) + "</span></div>");
        }
        if (leaveRequest.Remarks.Count != 0)
        {
            var latestRemark = leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First();
            html.AppendLine("<div class='info-row'><span class='label'>Remarks:</span> <span class='value'>" + EscapeHtml(latestRemark.Remarks) + "</span></div>");
        }
        html.AppendLine("<div class='info-row'><span class='label'>Status:</span> <span class='value'>" + leaveRequest.Status + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Created Date:</span> <span class='value'>" + leaveRequest.CreatedDate.ToString("yyyy-MM-dd HH:mm") + "</span></div>");
        html.AppendLine("</div>");
        html.AppendLine("<div class='footer'>");
        html.AppendLine("<p>This is an automated notification from the Leave System.</p>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    public static string CreateLeaveRequestDecisionEmail(LeaveRequest leaveRequest, string decision, string decisionMakerName, string? leaveTypeName = null)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head><meta charset='utf-8'><style>");
        html.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
        html.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
        html.AppendLine(".header { background-color: " + (decision == "Accepted" ? "#107c10" : "#d13438") + "; color: white; padding: 20px; text-align: center; }");
        html.AppendLine(".content { background-color: #f9f9f9; padding: 20px; margin-top: 20px; }");
        html.AppendLine(".info-row { margin: 10px 0; }");
        html.AppendLine(".label { font-weight: bold; color: #666; }");
        html.AppendLine(".value { color: #333; }");
        html.AppendLine(".footer { margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }");
        html.AppendLine("</style></head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class='container'>");
        html.AppendLine("<div class='header'><h1>Leave Request " + decision + "</h1></div>");
        html.AppendLine("<div class='content'>");
        html.AppendLine("<p>Your leave request has been <strong>" + decision.ToLower() + "</strong> by " + EscapeHtml(decisionMakerName) + ".</p>");
        html.AppendLine("<div class='info-row'><span class='label'>Leave Request ID:</span> <span class='value'>" + leaveRequest.Id + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Date From:</span> <span class='value'>" + leaveRequest.DateFrom.ToString("yyyy-MM-dd") + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Date To:</span> <span class='value'>" + leaveRequest.DateTo.ToString("yyyy-MM-dd") + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Duration:</span> <span class='value'>" + FormatDuration(leaveRequest.Duration) + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Working Hours:</span> <span class='value'>" + FormatDuration(leaveRequest.WorkingHours) + "</span></div>");
        if (!string.IsNullOrWhiteSpace(leaveTypeName))
        {
            html.AppendLine("<div class='info-row'><span class='label'>Leave Type:</span> <span class='value'>" + EscapeHtml(leaveTypeName) + "</span></div>");
        }
        if (leaveRequest.Remarks.Any())
        {
            var latestRemark = leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First();
            html.AppendLine("<div class='info-row'><span class='label'>Remarks:</span> <span class='value'>" + EscapeHtml(latestRemark.Remarks) + "</span></div>");
        }
        html.AppendLine("<div class='info-row'><span class='label'>Status:</span> <span class='value'>" + leaveRequest.Status + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Decision Date:</span> <span class='value'>" + leaveRequest.LastModifiedDate.ToString("yyyy-MM-dd HH:mm") + "</span></div>");
        html.AppendLine("</div>");
        html.AppendLine("<div class='footer'>");
        html.AppendLine("<p>This is an automated notification from the Leave System.</p>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    public static string CreateLeaveRequestCanceledEmail(LeaveRequest leaveRequest, string? leaveTypeName = null)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head><meta charset='utf-8'><style>");
        html.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
        html.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
        html.AppendLine(".header { background-color: #ff8c00; color: white; padding: 20px; text-align: center; }");
        html.AppendLine(".content { background-color: #f9f9f9; padding: 20px; margin-top: 20px; }");
        html.AppendLine(".info-row { margin: 10px 0; }");
        html.AppendLine(".label { font-weight: bold; color: #666; }");
        html.AppendLine(".value { color: #333; }");
        html.AppendLine(".footer { margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }");
        html.AppendLine("</style></head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class='container'>");
        html.AppendLine("<div class='header'><h1>Leave Request Canceled</h1></div>");
        html.AppendLine("<div class='content'>");
        html.AppendLine("<p>A leave request has been canceled by the employee.</p>");
        html.AppendLine("<div class='info-row'><span class='label'>Leave Request ID:</span> <span class='value'>" + leaveRequest.Id + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Employee:</span> <span class='value'>" + EscapeHtml(leaveRequest.AssignedTo.Name ?? leaveRequest.AssignedTo.Id) + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Date From:</span> <span class='value'>" + leaveRequest.DateFrom.ToString("yyyy-MM-dd") + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Date To:</span> <span class='value'>" + leaveRequest.DateTo.ToString("yyyy-MM-dd") + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Duration:</span> <span class='value'>" + FormatDuration(leaveRequest.Duration) + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Working Hours:</span> <span class='value'>" + FormatDuration(leaveRequest.WorkingHours) + "</span></div>");
        if (!string.IsNullOrWhiteSpace(leaveTypeName))
        {
            html.AppendLine("<div class='info-row'><span class='label'>Leave Type:</span> <span class='value'>" + EscapeHtml(leaveTypeName) + "</span></div>");
        }
        if (leaveRequest.Remarks.Count != 0)
        {
            var latestRemark = leaveRequest.Remarks.OrderByDescending(r => r.CreatedDate).First();
            html.AppendLine("<div class='info-row'><span class='label'>Remarks:</span> <span class='value'>" + EscapeHtml(latestRemark.Remarks) + "</span></div>");
        }
        html.AppendLine("<div class='info-row'><span class='label'>Status:</span> <span class='value'>" + leaveRequest.Status + "</span></div>");
        html.AppendLine("<div class='info-row'><span class='label'>Canceled Date:</span> <span class='value'>" + leaveRequest.LastModifiedDate.ToString("yyyy-MM-dd HH:mm") + "</span></div>");
        html.AppendLine("</div>");
        html.AppendLine("<div class='footer'>");
        html.AppendLine("<p>This is an automated notification from the Leave System.</p>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
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

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.Days > 0)
        {
            return $"{duration.Days} day(s)";
        }
        if (duration.Hours > 0)
        {
            return $"{duration.Hours} hour(s)";
        }
        return $"{duration.Minutes} minute(s)";
    }
}
