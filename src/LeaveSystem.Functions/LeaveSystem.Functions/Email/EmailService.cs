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

    public async Task SendEmailAsync(string to, string subject, string htmlContent, string? senderFullName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("Cannot send email: recipient email address is empty");
            return;
        }

        try
        {
            var emailContent = new EmailContent(subject)
            {
                Html = htmlContent
            };

            // Format sender address as "Full Name <sender@domain.com>" if name is provided
            var senderAddress = FormatSenderAddress(senderFullName);

            var emailMessage = new EmailMessage(senderAddress, to, emailContent);

            _logger.LogInformation("Sending email to {To} with subject {Subject} from {Sender}", to, subject, senderAddress);
            var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To}. Operation ID: {OperationId}", to, emailSendOperation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email to {To}", to);
            // Fire-and-forget: log error but don't throw
        }
    }

    public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlContent, string? senderFullName = null, CancellationToken cancellationToken = default)
    {
        var recipientList = recipients?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
        if (recipientList == null || recipientList.Count == 0)
        {
            _logger.LogWarning("Cannot send bulk email: no valid recipients provided");
            return;
        }

        var tasks = recipientList.Select(recipient =>
            SendEmailAsync(recipient, subject, htmlContent, senderFullName, cancellationToken)
        );

        await Task.WhenAll(tasks);
    }

    private string FormatSenderAddress(string? fullName)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return $"{fullName.Trim()} <{_senderAddress}>";
        }
        return _senderAddress;
    }
}
