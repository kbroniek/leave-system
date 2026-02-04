namespace LeaveSystem.Domain.LeaveRequests.Accepting;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.Extensions.Logging;

public class AcceptLeaveRequestService(
    ReadService readService,
    WriteService writeService,
    CreateLeaveRequestValidator validator,
    IEmailService emailService,
    IGetUserRepository getUserRepository,
    ILogger<AcceptLeaveRequestService>? logger)
{
    public async Task<Result<LeaveRequest, Error>> Accept(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken, string? language = null, string? baseUrl = null)
    {
        var resultFindById = await readService.FindById<LeaveRequest>(leaveRequestId, cancellationToken);
        if (resultFindById.IsFailure)
        {
            return resultFindById;
        }
        var leaveRequest = resultFindById.Value;
        // Cannot accept overlapping limit or when user doesn't have limits.
        var validateResult = await validator.Validate(
            leaveRequestId, leaveRequest.DateFrom, leaveRequest.DateTo,
            leaveRequest.Duration, leaveRequest.LeaveTypeId, leaveRequest.WorkingHours,
            leaveRequest.AssignedTo.Id, cancellationToken);
        if (validateResult.IsFailure)
        {
            return validateResult.Error;
        }
        var resultAccept = leaveRequest.Accept(leaveRequestId, remarks, acceptedBy, createdDate);
        if (resultAccept.IsFailure)
        {
            return resultAccept;
        }
        var writeResult = await writeService.Write(resultAccept.Value, cancellationToken);

        // Send email asynchronously after successful acceptance
        if (writeResult.IsSuccess)
        {
            var decisionMakerName = acceptedBy.Name ?? acceptedBy.Email ?? "unknown";
            var replyToEmail = !string.IsNullOrWhiteSpace(acceptedBy.Email)
                ? new IEmailService.EmailAddress(acceptedBy.Email, acceptedBy.Name)
                : (IEmailService.EmailAddress?)null;
            await SendLeaveRequestAcceptedEmailAsync(
                writeResult.Value,
                decisionMakerName,
                replyToEmail,
                language,
                baseUrl,
                cancellationToken);
        }

        return writeResult;
    }

    private async Task SendLeaveRequestAcceptedEmailAsync(
        LeaveRequest leaveRequest,
        string decisionMakerName,
        IEmailService.EmailAddress? replyToEmail,
        string? language,
        string? baseUrl,
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

            var subject = EmailTemplates.GetEmailSubject("Leave Request Accepted", language, decisionMakerName);
            var htmlContent = EmailTemplates.CreateLeaveRequestDecisionEmail(
                leaveRequest, "Accepted", decisionMakerName, language: language, baseUrl: baseUrl);
            var recipient = new IEmailService.EmailAddress(ownerResult.Value.Email!, ownerResult.Value.Name);
            await emailService.SendEmailAsync(recipient, subject, htmlContent, replyToEmail, attachments: null, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while sending leave request accepted email. LeaveRequestId: {LeaveRequestId}, EmployeeId: {EmployeeId}",
                leaveRequest.Id, leaveRequest.AssignedTo.Id);
        }
    }
}
