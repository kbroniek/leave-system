namespace LeaveSystem.Domain.LeaveRequests.Creating;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailAsync(EmailAddress recipient, string subject, string htmlContent, EmailAddress? replyToEmail = null, IEnumerable<EmailAttachment>? attachments = null, CancellationToken cancellationToken = default);
    Task SendBulkEmailAsync(IEnumerable<EmailAddress> recipients, string subject, string htmlContent, EmailAddress? replyToEmail = null, IEnumerable<EmailAttachment>? attachments = null, CancellationToken cancellationToken = default);

    public record struct EmailAddress(string Email, string? Name);

    public record EmailAttachment(string Name, string ContentType, byte[] Content);
}
