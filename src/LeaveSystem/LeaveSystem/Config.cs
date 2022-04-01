using GoldenEye.Marten.Registration;
using LeaveSystem.LeaveRequests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
//using Tickets.Maintenance;
//using Tickets.Reservations;

namespace Tickets;

public static class Config
{
    public static void AddLeaveSystemModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddMarten(_ => config.GetConnectionString("Marten"));
        services.AddLeaveRequests();
        //services.AddMaintainance();
    }
}
