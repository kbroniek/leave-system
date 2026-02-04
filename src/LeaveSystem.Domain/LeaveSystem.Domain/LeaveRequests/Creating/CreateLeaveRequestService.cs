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
    IEmailService emailService,
    IDecisionMakerRepository decisionMakerRepository,
    IGetUserRepository getUserRepository,
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

        // Send emails asynchronously after successful creation
        if (writeResult.IsSuccess)
        {
            var creatorName = createdBy.Name ?? createdBy.Email ?? "unknown";
            var replyToEmail = !string.IsNullOrWhiteSpace(createdBy.Email)
                ? new IEmailService.EmailAddress(createdBy.Email, createdBy.Name)
                : (IEmailService.EmailAddress?)null;
            await SendLeaveRequestCreatedEmailsAsync(
                writeResult.Value,
                creatorName,
                replyToEmail,
                language,
                baseUrl,
                cancellationToken);
        }

        return writeResult;
    }

    private async Task SendLeaveRequestCreatedEmailsAsync(
        LeaveRequest leaveRequest,
        string creatorName,
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

            var decisionMakerRecipients = new List<IEmailService.EmailAddress>();

            // Get DecisionMaker emails
            var decisionMakersResult = await getUserRepository.GetUsers([.. decisionMakerIds], cancellationToken);
            if (decisionMakersResult.IsSuccess)
            {
                decisionMakerRecipients.AddRange(decisionMakersResult.Value
                    .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                    .Select(u => new IEmailService.EmailAddress(u.Email!, u.Name)));
            }

            var subject = EmailTemplates.GetEmailSubject("New Leave Request Created", language, creatorName);

            // Send email to decision makers (without calendar attachment)
            if (decisionMakerRecipients.Count > 0)
            {
                var htmlContentForDecisionMakers = EmailTemplates.CreateLeaveRequestCreatedEmail(leaveRequest, language: language, baseUrl: baseUrl, includeCalendarNote: false, isForDecisionMaker: true);
                await emailService.SendBulkEmailAsync(decisionMakerRecipients, subject, htmlContentForDecisionMakers, replyToEmail, attachments: null, cancellationToken);
            }

            // Send email to assigned user with calendar attachment (if different from creator)
            var assignedUserResult = await getUserRepository.GetUser(leaveRequest.AssignedTo.Id, cancellationToken);
            if (assignedUserResult.IsSuccess && !string.IsNullOrWhiteSpace(assignedUserResult.Value.Email))
            {
                var assignedUserRecipient = new IEmailService.EmailAddress(assignedUserResult.Value.Email!, assignedUserResult.Value.Name);
                try
                {
                    // Generate calendar attachment
                    var icsContent = CalendarEventGenerator.GenerateIcsFile(leaveRequest, leaveTypeName: null, language: language, baseUrl: baseUrl);
                    var calendarAttachment = new IEmailService.EmailAttachment(
                        $"leave-request-{leaveRequest.Id}.ics",
                        "text/calendar",
                        icsContent);

                    // Include calendar note in email content for assigned user (informational, not requiring decision)
                    var htmlContentForAssignedUser = EmailTemplates.CreateLeaveRequestCreatedEmail(leaveRequest, language: language, baseUrl: baseUrl, includeCalendarNote: true, isForDecisionMaker: false);
                    await emailService.SendEmailAsync(
                        assignedUserRecipient,
                        subject,
                        htmlContentForAssignedUser,
                        replyToEmail,
                        attachments: [calendarAttachment],
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail - send email without attachment as fallback
                    logger?.LogWarning(ex, "Failed to generate calendar attachment for leave request {LeaveRequestId}. Sending email without attachment.", leaveRequest.Id);
                    var htmlContentFallback = EmailTemplates.CreateLeaveRequestCreatedEmail(leaveRequest, language: language, baseUrl: baseUrl, includeCalendarNote: false, isForDecisionMaker: false);
                    await emailService.SendEmailAsync(
                        assignedUserRecipient,
                        subject,
                        htmlContentFallback,
                        replyToEmail,
                        attachments: null,
                        cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while sending leave request created email. LeaveRequestId: {LeaveRequestId}, EmployeeId: {EmployeeId}",
                leaveRequest.Id, leaveRequest.AssignedTo.Id);
        }
    }
}
