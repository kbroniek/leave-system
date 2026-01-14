namespace LeaveSystem.Domain.LeaveRequests.Creating;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailAsync(EmailRecipient recipient, string subject, string htmlContent, string? senderFullName = null, CancellationToken cancellationToken = default);
    Task SendBulkEmailAsync(IEnumerable<EmailRecipient> recipients, string subject, string htmlContent, string? senderFullName = null, CancellationToken cancellationToken = default);

    public record EmailRecipient(string Email, string? Name);
}
