namespace LeaveSystem.Domain.LeaveRequests.Rejecting;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.Extensions.Logging;

public class RejectLeaveRequestService(
    ReadService readService,
    WriteService writeService,
    IEmailService? emailService,
    IGetUserRepository? getUserRepository,
    ILogger<RejectLeaveRequestService>? logger)
{
    public async Task<Result<LeaveRequest, Error>> Reject(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken, string? language = null, string? baseUrl = null)
    {
        var resultFindById = await readService.FindById<LeaveRequest>(leaveRequestId, cancellationToken);
        if (!resultFindById.IsSuccess)
        {
            return resultFindById;
        }
        var resultAccept = resultFindById.Value.Reject(leaveRequestId, remarks, acceptedBy, createdDate);
        if (!resultAccept.IsSuccess)
        {
            return resultAccept;
        }
        var writeResult = await writeService.Write(resultAccept.Value, cancellationToken);

        // Send email asynchronously (fire-and-forget) after successful rejection
        if (writeResult.IsSuccess && emailService != null && getUserRepository != null)
        {
            var emailLanguage = language;
            var decisionMakerName = acceptedBy.Name ?? acceptedBy.Email;
            var replyToEmail = !string.IsNullOrWhiteSpace(acceptedBy.Email)
                ? new IEmailService.EmailAddress(acceptedBy.Email, acceptedBy.Name)
                : (IEmailService.EmailAddress?)null;
            var serviceLogger = logger;
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendLeaveRequestRejectedEmailAsync(
                        writeResult.Value,
                        decisionMakerName,
                        replyToEmail,
                        emailService,
                        getUserRepository,
                        emailLanguage,
                        baseUrl,
                        serviceLogger,
                        cancellationToken);
                }
                catch
                {
                    // Silently ignore errors in fire-and-forget email sending
                }
            }, cancellationToken);
        }

        return writeResult;
    }

    private static async Task SendLeaveRequestRejectedEmailAsync(
        LeaveRequest leaveRequest,
        string decisionMakerName,
        IEmailService.EmailAddress? replyToEmail,
        IEmailService emailService,
        IGetUserRepository getUserRepository,
        string? language,
        string? baseUrl,
        ILogger<RejectLeaveRequestService>? logger,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get leave request owner email
            var ownerResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
            if (ownerResult.IsFailure || string.IsNullOrWhiteSpace(ownerResult.Value.Email))
            {
                return;
            }

            var subject = EmailTemplates.GetEmailSubject("Leave Request Rejected", language, decisionMakerName);
            var htmlContent = EmailTemplates.CreateLeaveRequestDecisionEmail(
                leaveRequest, "Rejected", decisionMakerName, language: language, baseUrl: baseUrl);
            var recipient = new IEmailService.EmailAddress(ownerResult.Value.Email!, ownerResult.Value.Name);
            await emailService.SendEmailAsync(recipient, subject, htmlContent, replyToEmail, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while sending leave request rejected email. LeaveRequestId: {LeaveRequestId}, EmployeeId: {EmployeeId}",
                leaveRequest.Id, leaveRequest.AssignedTo.Id);
        }
    }
}
