namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.Extensions.Logging;

public class CreateLeaveRequestService(
    IReadEventsRepository readEventsRepository,
    CreateLeaveRequestValidator createLeaveRequestValidator,
    WriteService writeService,
    IEmailService? emailService,
    IDecisionMakerRepository? decisionMakerRepository,
    IGetUserRepository? getUserRepository,
    ILogger<CreateLeaveRequestService>? logger)
{
    public async Task<Result<LeaveRequest, Error>> CreateAsync(
        Guid leaveRequestId, DateOnly dateFrom, DateOnly dateTo,
        TimeSpan duration, Guid leaveTypeId, string? remarks,
        LeaveRequestUserDto createdBy, LeaveRequestUserDto assignedTo,
        TimeSpan workingHours, DateTimeOffset createdDate,
        CancellationToken cancellationToken,
        string? language = null, string? baseUrl = null)
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
            var emailLanguage = language;
            var creatorName = createdBy.Name ?? createdBy.Email;
            var replyToEmail = createdBy.Email;
            var serviceLogger = logger;
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
                            creatorName,
                            replyToEmail,
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
        }

        return writeResult;
    }

    private static async Task SendLeaveRequestCreatedEmailsAsync(
        LeaveRequest leaveRequest,
        IEmailService emailService,
        IReadOnlyCollection<string> decisionMakerIds,
        string creatorName,
        string? replyToEmail,
        IGetUserRepository getUserRepository,
        string? language,
        string? baseUrl,
        ILogger<CreateLeaveRequestService>? logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var recipients = new List<IEmailService.EmailRecipient>();

            // Get DecisionMaker emails
            if (decisionMakerIds.Count > 0)
            {
                var decisionMakersResult = await getUserRepository.GetUsers([.. decisionMakerIds], cancellationToken);
                if (decisionMakersResult.IsSuccess)
                {
                    recipients.AddRange(decisionMakersResult.Value
                        .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                        .Select(u => new IEmailService.EmailRecipient(u.Email!, u.Name)));
                }
            }

            // If created on behalf (assignedTo != createdBy), also send to assigned user
            if (leaveRequest.AssignedTo.Id != leaveRequest.CreatedBy.Id)
            {
                var assignedUserResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
                if (assignedUserResult.IsSuccess && !string.IsNullOrWhiteSpace(assignedUserResult.Value.Email))
                {
                    recipients.Add(new IEmailService.EmailRecipient(assignedUserResult.Value.Email!, assignedUserResult.Value.Name));
                }
            }

            // Send emails
            if (recipients.Count > 0)
            {
                var subject = EmailTemplates.GetEmailSubject("New Leave Request Created", language, creatorName);
                var htmlContent = EmailTemplates.CreateLeaveRequestCreatedEmail(leaveRequest, language: language, baseUrl: baseUrl);
                await emailService.SendBulkEmailAsync(recipients, subject, htmlContent, replyToEmail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while sending leave request created email. LeaveRequestId: {LeaveRequestId}, EmployeeId: {EmployeeId}",
                leaveRequest.Id, leaveRequest.AssignedTo.Id);
        }
    }
}
