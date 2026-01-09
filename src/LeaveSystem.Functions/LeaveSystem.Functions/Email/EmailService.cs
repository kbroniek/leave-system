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
        var connectionString = configuration["CommunicationServicesConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("CommunicationServicesConnectionString is not configured.");
        }

        _emailClient = new EmailClient(connectionString);
        _senderAddress = "DoNotReply@leave-system.azurecomm.net"; // Default ACS domain, can be configured
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken = default)
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

            var emailMessage = new EmailMessage(_senderAddress, new EmailAddress(to), emailContent);

            _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
            var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To}. Operation ID: {OperationId}", to, emailSendOperation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email to {To}", to);
            // Fire-and-forget: log error but don't throw
        }
    }

    public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        var recipientList = recipients?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
        if (recipientList == null || recipientList.Count == 0)
        {
            _logger.LogWarning("Cannot send bulk email: no valid recipients provided");
            return;
        }

        var tasks = recipientList.Select(recipient =>
            SendEmailAsync(recipient, subject, htmlContent, cancellationToken)
        );

        await Task.WhenAll(tasks);
    }
}
