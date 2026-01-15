namespace LeaveSystem.Functions.Email;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using LeaveSystem.Domain.LeaveRequests.Creating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

internal class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        var connectionString = configuration["CommunicationServicesConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("CommunicationServicesConnectionString is not configured.");
        }

        _emailClient = new EmailClient(connectionString);

        // Get sender address from configuration, fallback to default if not configured
        var configuredSender = configuration["EmailSenderAddress"];
        if (string.IsNullOrWhiteSpace(configuredSender))
        {
            // Fallback to default ACS domain format if not configured
            // This should match the default domain provided by Azure Communication Services
            _senderAddress = "DoNotReply@azurecomm.net";
            _logger.LogWarning("EmailSenderAddress not configured, using default: {DefaultSender}", _senderAddress);
        }
        else
        {
            _senderAddress = configuredSender;
            _logger.LogInformation("Using configured email sender address: {SenderAddress}", _senderAddress);
        }
    }

    public async Task SendEmailAsync(IEmailService.EmailAddress recipient, string subject, string htmlContent, IEmailService.EmailAddress? replyToEmail = null, CancellationToken cancellationToken = default) =>
        await SendBulkEmailAsync([recipient], subject, htmlContent, replyToEmail, cancellationToken);

    public async Task SendBulkEmailAsync(IEnumerable<IEmailService.EmailAddress> recipients, string subject, string htmlContent, IEmailService.EmailAddress? replyToEmail = null, CancellationToken cancellationToken = default)
    {
        var recipientList = recipients?.Where(r => !string.IsNullOrWhiteSpace(r.Email)).ToList();
        if (recipientList == null || recipientList.Count == 0)
        {
            _logger.LogWarning("Cannot send bulk email: no valid recipients provided");
            return;
        }

        try
        {
            var emailContent = new EmailContent(subject)
            {
                Html = htmlContent
            };
            var emailRecipients = new EmailRecipients([.. recipientList.Select(recipient => new EmailAddress(recipient.Email, recipient.Name))]);
            var emailMessage = new EmailMessage(_senderAddress, emailRecipients, emailContent);

            // Set Reply-To header if provided
            if (replyToEmail.HasValue && !string.IsNullOrWhiteSpace(replyToEmail.Value.Email))
            {
                emailMessage.ReplyTo.Add(new EmailAddress(replyToEmail.Value.Email, replyToEmail.Value.Name));
            }

            var recipientsString = string.Join(", ", recipientList.Select(r => r.Email));
            _logger.LogInformation("Sending email to {To} with subject {Subject} from {Sender} (Reply-To: {ReplyTo})", recipientsString, subject, _senderAddress, replyToEmail?.Email ?? "none");
            var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To}. Operation ID: {OperationId}", recipientsString, emailSendOperation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email to {To}", string.Join(", ", recipientList.Select(r => r.Email)));
        }
    }
}
