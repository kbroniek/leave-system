using System.Linq.Expressions;
using LeaveSystem.Shared.LeaveRequests;

namespace LeaveSystem.EventSourcing.LeaveRequests;

public static class LeaveRequestExpressions
{
    public static Expression<Func<LeaveRequest, bool>> IsValidExpression => request =>
        request.Status == LeaveRequestStatus.Accepted || request.Status == LeaveRequestStatus.Pending;
}