namespace LeaveSystem.Domain.LeaveRequests.Rejecting;
using LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

public class RejectLeaveRequestService(
    ReadService readService,
    WriteService writeService,
    IEmailService? emailService,
    IGetUserRepository? getUserRepository)
{
    public async Task<Result<LeaveRequest, Error>> Reject(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken, string? language = null)
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
            // Capture language before async task
            var emailLanguage = language;
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendLeaveRequestRejectedEmailAsync(
                        writeResult.Value,
                        acceptedBy.Name ?? acceptedBy.Id,
                        emailService,
                        getUserRepository,
                        emailLanguage,
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
        IEmailService emailService,
        IGetUserRepository getUserRepository,
        string? language,
        CancellationToken cancellationToken)
    {
        // Get leave request owner email
        var ownerResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
        if (ownerResult.IsFailure || string.IsNullOrWhiteSpace(ownerResult.Value.Email))
        {
            return;
        }

        var subject = "Leave Request Rejected";
        var htmlContent = EmailTemplates.CreateLeaveRequestDecisionEmail(
            leaveRequest, "Rejected", decisionMakerName, language: language);
        await emailService.SendEmailAsync(ownerResult.Value.Email!, subject, htmlContent, cancellationToken);
    }
}
