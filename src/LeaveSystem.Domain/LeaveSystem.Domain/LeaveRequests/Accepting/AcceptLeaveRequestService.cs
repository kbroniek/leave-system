namespace LeaveSystem.Domain.LeaveRequests.Accepting;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

public class AcceptLeaveRequestService(
    ReadService readService,
    WriteService writeService,
    CreateLeaveRequestValidator validator,
    IEmailService? emailService,
    IGetUserRepository? getUserRepository)
{
    public async Task<Result<LeaveRequest, Error>> Accept(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken)
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
        
        // Send email asynchronously (fire-and-forget) after successful acceptance
        if (writeResult.IsSuccess && emailService != null && getUserRepository != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendLeaveRequestAcceptedEmailAsync(
                        writeResult.Value,
                        acceptedBy.Name ?? acceptedBy.Id,
                        emailService,
                        getUserRepository,
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

    private static async Task SendLeaveRequestAcceptedEmailAsync(
        LeaveRequest leaveRequest,
        string decisionMakerName,
        IEmailService emailService,
        IGetUserRepository getUserRepository,
        CancellationToken cancellationToken)
    {
        // Get leave request owner email
        var ownerResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
        if (ownerResult.IsFailure || string.IsNullOrWhiteSpace(ownerResult.Value.Email))
        {
            return;
        }

        var subject = "Leave Request Accepted";
        var htmlContent = EmailTemplates.CreateLeaveRequestDecisionEmail(
            leaveRequest, "Accepted", decisionMakerName);
        await emailService.SendEmailAsync(ownerResult.Value.Email!, subject, htmlContent, cancellationToken);
    }
}
