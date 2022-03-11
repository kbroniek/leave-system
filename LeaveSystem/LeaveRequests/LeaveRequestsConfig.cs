using GoldenEye.Backend.Core.Marten.Registration;
using GoldenEye.Backend.Core.Marten.Repositories;
using GoldenEye.Shared.Core.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.LeaveRequests;
internal static class LeaveRequestsConfig
{
    internal static void AddLeaveRequests(this IServiceCollection services)
    {
        services.AddMartenEventSourcedRepository<LeaveRequest>();
    }
}

