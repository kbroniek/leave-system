namespace LeaveSystem.Domain.LeaveRequests.Creating;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken = default);
    Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlContent, CancellationToken cancellationToken = default);
}
