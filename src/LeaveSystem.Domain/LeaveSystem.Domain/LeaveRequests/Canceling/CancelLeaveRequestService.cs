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
    IEmailService? emailService,
    IDecisionMakerRepository? decisionMakerRepository,
    IGetUserRepository? getUserRepository,
    ILogger<CancelLeaveRequestService>? logger)
{
    public async Task<Result<LeaveRequest, Error>> Cancel(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken, string? language = null)
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
        if (writeResult.IsSuccess && emailService != null && decisionMakerRepository != null && getUserRepository != null)
        {
            var emailLanguage = language;
            var decisionMakerName = acceptedBy.Name ?? acceptedBy.Email;
            var serviceLogger = logger;
            // Get DecisionMaker user IDs
            var decisionMakerIdsResult = await decisionMakerRepository.GetDecisionMakerUserIds(cancellationToken);
            if (decisionMakerIdsResult.IsSuccess)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendLeaveRequestCanceledEmailsAsync(
                            writeResult.Value,
                            emailService,
                            decisionMakerIdsResult.Value,
                            decisionMakerName,
                            getUserRepository,
                            emailLanguage,
                            serviceLogger,
                            cancellationToken);
                    }
                    catch
                    {
                        // Silently ignore errors in fire-and-forget email sending
                    }
                }, cancellationToken);
            }
        }

        return writeResult;
    }

    private static async Task SendLeaveRequestCanceledEmailsAsync(
        LeaveRequest leaveRequest,
        IEmailService emailService,
        IReadOnlyCollection<string> decisionMakerIds,
        string decisionMakerName,
        IGetUserRepository getUserRepository,
        string? language,
        ILogger<CancelLeaveRequestService>? logger,
        CancellationToken cancellationToken)
    {
        try
        {
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
                .Select(u => new IEmailService.EmailRecipient(u.Email!, u.Name))
                .ToList();

            // Send emails
            if (recipients.Count > 0)
            {
                var subject = EmailTemplates.GetEmailSubject("Leave Request Canceled", language);
                var htmlContent = EmailTemplates.CreateLeaveRequestCanceledEmail(leaveRequest, language: language);
                await emailService.SendBulkEmailAsync(recipients, subject, htmlContent, decisionMakerName, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while sending leave request canceled email. LeaveRequestId: {LeaveRequestId}, EmployeeId: {EmployeeId}",
                leaveRequest.Id, leaveRequest.AssignedTo.Id);
        }
    }
}
