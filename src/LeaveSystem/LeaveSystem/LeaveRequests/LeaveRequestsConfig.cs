using GoldenEye.Marten.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.LeaveRequests;
internal static class LeaveRequestsConfig
{
    internal static void AddLeaveRequests(this IServiceCollection services)
    {
        services.AddMartenEventSourcedRepository<LeaveRequest>();
    }
}

