using GoldenEye.Backend.Core.Marten.Registration;
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
        services.AddMarten(s => config.GetConnectionString("Marten"));
        //services.AddReservations();
        //services.AddMaintainance();
    }
}
