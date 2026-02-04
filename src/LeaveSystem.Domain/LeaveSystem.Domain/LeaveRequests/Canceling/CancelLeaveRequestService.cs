namespace LeaveSystem.Domain.LeaveRequests.Canceling;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.Extensions.Logging;

public class CancelLeaveRequestService(
    ReadService readService,
    WriteService writeService,
    TimeProvider timeProvider,
    IEmailService emailService,
    IDecisionMakerRepository decisionMakerRepository,
    IGetUserRepository getUserRepository,
    ILogger<CancelLeaveRequestService>? logger)
{
    public async Task<Result<LeaveRequest, Error>> Cancel(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken, string? language = null, string? baseUrl = null)
    {
        var resultFindById = await readService.FindById<LeaveRequest>(leaveRequestId, cancellationToken);
        if (!resultFindById.IsSuccess)
        {
            return resultFindById;
        }
        if (resultFindById.Value.DateFrom < DateOnly.FromDateTime(timeProvider.GetUtcNow().Date))
        {
            return new Error("Canceling of past leave requests is not allowed.", System.Net.HttpStatusCode.Forbidden, ErrorCodes.PAST_LEAVE_MODIFICATION);
        }
        var resultAccept = resultFindById.Value.Cancel(leaveRequestId, remarks, acceptedBy, createdDate);
        if (!resultAccept.IsSuccess)
        {
            return resultAccept;
        }
        var writeResult = await writeService.Write(resultAccept.Value, cancellationToken);

        // Send emails asynchronously (fire-and-forget) after successful cancellation
        if (writeResult.IsSuccess)
        {
            var decisionMakerName = acceptedBy.Name ?? acceptedBy.Email ?? "unknown";
            var replyToEmail = !string.IsNullOrWhiteSpace(acceptedBy.Email)
                ? new IEmailService.EmailAddress(acceptedBy.Email, acceptedBy.Name)
                : (IEmailService.EmailAddress?)null;
            await SendLeaveRequestCanceledEmailsAsync(
                writeResult.Value,
                decisionMakerName,
                replyToEmail,
                language,
                baseUrl,
                cancellationToken);
        }

        return writeResult;
    }

    private async Task SendLeaveRequestCanceledEmailsAsync(
        LeaveRequest leaveRequest,
        string decisionMakerName,
        IEmailService.EmailAddress? replyToEmail,
        string? language,
        string? baseUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            var decisionMakerIdsResult = await decisionMakerRepository.GetDecisionMakerUserIds(cancellationToken);
            if (decisionMakerIdsResult.IsFailure)
            {
                return;
            }
            var decisionMakerIds = decisionMakerIdsResult.Value;
            if (decisionMakerIds.Count == 0)
            {
                return;
            }

            // Get DecisionMaker emails
            var decisionMakersResult = await getUserRepository.GetUsers([.. decisionMakerIds], cancellationToken);
            if (decisionMakersResult.IsFailure)
            {
                return;
            }

            var recipients = decisionMakersResult.Value
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .Select(u => new IEmailService.EmailAddress(u.Email!, u.Name))
                .ToList();

            // Send emails to decision makers
            if (recipients.Count > 0)
            {
                var subject = EmailTemplates.GetEmailSubject("Leave Request Canceled", language, decisionMakerName);
                var htmlContent = EmailTemplates.CreateLeaveRequestCanceledEmail(leaveRequest, language: language, baseUrl: baseUrl, includeCalendarRemovalNote: false);
                await emailService.SendBulkEmailAsync(recipients, subject, htmlContent, replyToEmail, attachments: null, cancellationToken);
            }

            // Send cancellation email to assigned user with calendar removal (if they originally received calendar event)
            try
            {
                var assignedUserResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
                if (assignedUserResult.IsSuccess && !string.IsNullOrWhiteSpace(assignedUserResult.Value.Email))
                {
                    // Generate cancellation .ics file
                    var icsContent = CalendarEventGenerator.GenerateCancellationIcsFile(leaveRequest, leaveTypeName: null, language: language, baseUrl: baseUrl);
                    var calendarAttachment = new IEmailService.EmailAttachment(
                        $"leave-request-{leaveRequest.Id}-cancellation.ics",
                        "text/calendar",
                        icsContent);

                    // Send email with cancellation .ics attachment
                    var subject = EmailTemplates.GetEmailSubject("Leave Request Canceled", language, decisionMakerName);
                    var htmlContent = EmailTemplates.CreateLeaveRequestCanceledEmail(leaveRequest, language: language, baseUrl: baseUrl, includeCalendarRemovalNote: true);
                    var assignedUserRecipient = new IEmailService.EmailAddress(assignedUserResult.Value.Email!, assignedUserResult.Value.Name);
                    await emailService.SendEmailAsync(
                        assignedUserRecipient,
                        subject,
                        htmlContent,
                        replyToEmail,
                        attachments: [calendarAttachment],
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail - send email without attachment as fallback
                logger.LogWarning(ex, "Failed to generate or send cancellation calendar attachment for leave request {LeaveRequestId}. Sending email without attachment.", leaveRequest.Id);
                try
                {
                    var assignedUserResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
                    if (assignedUserResult.IsSuccess && !string.IsNullOrWhiteSpace(assignedUserResult.Value.Email))
                    {
                        var subject = EmailTemplates.GetEmailSubject("Leave Request Canceled", language, decisionMakerName);
                        var htmlContent = EmailTemplates.CreateLeaveRequestCanceledEmail(leaveRequest, language: language, baseUrl: baseUrl, includeCalendarRemovalNote: false);
                        var assignedUserRecipient = new IEmailService.EmailAddress(assignedUserResult.Value.Email!, assignedUserResult.Value.Name);
                        await emailService.SendEmailAsync(
                            assignedUserRecipient,
                            subject,
                            htmlContent,
                            replyToEmail,
                            attachments: null,
                            cancellationToken);
                    }
                }
                catch (Exception fallbackEx)
                {
                    logger?.LogError(fallbackEx, "Failed to send cancellation email to assigned user. LeaveRequestId: {LeaveRequestId}", leaveRequest.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while sending leave request canceled email. LeaveRequestId: {LeaveRequestId}, EmployeeId: {EmployeeId}",
                leaveRequest.Id, leaveRequest.AssignedTo.Id);
        }
    }
}
