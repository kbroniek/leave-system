namespace LeaveSystem.Domain.LeaveRequests.Canceling;
using LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

public class CancelLeaveRequestService(
    ReadService readService,
    WriteService writeService,
    TimeProvider timeProvider,
    IEmailService? emailService,
    IDecisionMakerRepository? decisionMakerRepository,
    IGetUserRepository? getUserRepository)
{
    public async Task<Result<LeaveRequest, Error>> Cancel(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken)
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
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendLeaveRequestCanceledEmailsAsync(
                        writeResult.Value,
                        emailService,
                        decisionMakerRepository,
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

    private static async Task SendLeaveRequestCanceledEmailsAsync(
        LeaveRequest leaveRequest,
        IEmailService emailService,
        IDecisionMakerRepository decisionMakerRepository,
        IGetUserRepository getUserRepository,
        CancellationToken cancellationToken)
    {
        // Get DecisionMaker user IDs
        var decisionMakerIdsResult = await decisionMakerRepository.GetDecisionMakerUserIds(cancellationToken);
        if (decisionMakerIdsResult.IsFailure)
        {
            return;
        }

        var decisionMakerIds = decisionMakerIdsResult.Value.ToList();
        if (decisionMakerIds.Count == 0)
        {
            return;
        }

        // Get DecisionMaker emails
        var decisionMakersResult = await getUserRepository.GetUsers(decisionMakerIds.ToArray(), cancellationToken);
        if (decisionMakersResult.IsFailure)
        {
            return;
        }

        var recipientEmails = decisionMakersResult.Value
            .Where(u => !string.IsNullOrWhiteSpace(u.Email))
            .Select(u => u.Email!)
            .ToList();

        // Send emails
        if (recipientEmails.Count > 0)
        {
            var subject = "Leave Request Canceled";
            var htmlContent = EmailTemplates.CreateLeaveRequestCanceledEmail(leaveRequest);
            await emailService.SendBulkEmailAsync(recipientEmails, subject, htmlContent, cancellationToken);
        }
    }
}
