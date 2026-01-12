namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

public class CreateLeaveRequestService(
    IReadEventsRepository readEventsRepository,
    CreateLeaveRequestValidator createLeaveRequestValidator,
    WriteService writeService,
    IEmailService? emailService,
    IDecisionMakerRepository? decisionMakerRepository,
    IGetUserRepository? getUserRepository)
{
    public async Task<Result<LeaveRequest, Error>> CreateAsync(
        Guid leaveRequestId, DateOnly dateFrom, DateOnly dateTo,
        TimeSpan duration, Guid leaveTypeId, string? remarks,
        LeaveRequestUserDto createdBy, LeaveRequestUserDto assignedTo,
        TimeSpan workingHours, DateTimeOffset createdDate,
        CancellationToken cancellationToken)
    {
        var streamEnumerable = readEventsRepository.ReadStreamAsync(leaveRequestId, cancellationToken).WithCancellation(cancellationToken);
        var enumerator = streamEnumerable.GetAsyncEnumerator();
        // If MoveNextAsync() returns false, the enumerator is empty.
        if (await enumerator.MoveNextAsync())
        {
            return new Error("The resource already exists.", System.Net.HttpStatusCode.Conflict, ErrorCodes.RESOURCE_EXISTS);
        }
        var validateResult = await createLeaveRequestValidator.Validate(
            leaveRequestId, dateFrom, dateTo,
            duration, leaveTypeId, workingHours,
            assignedTo.Id, cancellationToken);
        if (validateResult.IsFailure)
        {
            return validateResult.Error;
        }
        var leaveRequest = new LeaveRequest();
        var result = leaveRequest.Pending(
            leaveRequestId, dateFrom, dateTo,
            duration, leaveTypeId, remarks,
            createdBy, assignedTo, workingHours,
            createdDate);
        if (result.IsFailure)
        {
            return result;
        }
        var writeResult = await writeService.Write(result.Value, cancellationToken);

        // Send emails asynchronously (fire-and-forget) after successful creation
        if (writeResult.IsSuccess && emailService != null && decisionMakerRepository != null && getUserRepository != null)
        {
            // Get DecisionMaker user IDs
            var decisionMakerIdsResult = await decisionMakerRepository.GetDecisionMakerUserIds(cancellationToken);
            if (decisionMakerIdsResult.IsSuccess)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendLeaveRequestCreatedEmailsAsync(
                            writeResult.Value,
                            emailService,
                            decisionMakerIdsResult.Value,
                            getUserRepository,
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

    private static async Task SendLeaveRequestCreatedEmailsAsync(
        LeaveRequest leaveRequest,
        IEmailService emailService,
        IReadOnlyCollection<string> decisionMakerIds,
        IGetUserRepository getUserRepository,
        CancellationToken cancellationToken)
    {
        var recipientEmails = new List<string>();

        // Get DecisionMaker emails
        if (decisionMakerIds.Count > 0)
        {
            var decisionMakersResult = await getUserRepository.GetUsers([.. decisionMakerIds], cancellationToken);
            if (decisionMakersResult.IsSuccess)
            {
                recipientEmails.AddRange(decisionMakersResult.Value
                    .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                    .Select(u => u.Email!));
            }
        }

        // If created on behalf (assignedTo != createdBy), also send to assigned user
        if (leaveRequest.AssignedTo.Id != leaveRequest.CreatedBy.Id)
        {
            var assignedUserResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
            if (assignedUserResult.IsSuccess && !string.IsNullOrWhiteSpace(assignedUserResult.Value.Email))
            {
                recipientEmails.Add(assignedUserResult.Value.Email!);
            }
        }

        // Send emails
        if (recipientEmails.Count > 0)
        {
            var subject = "New Leave Request Created";
            var htmlContent = EmailTemplates.CreateLeaveRequestCreatedEmail(leaveRequest);
            await emailService.SendBulkEmailAsync(recipientEmails, subject, htmlContent, cancellationToken);
        }
    }
}
