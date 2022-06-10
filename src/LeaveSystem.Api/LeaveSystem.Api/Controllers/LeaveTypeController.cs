using LeaveSystem.Api.Domains;
using LeaveSystem.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveTypeController : GenericCrudController<LeaveType>
    {
        public LeaveTypeController(LeaveSystemDbContext dbContext)
            : base(dbContext)
        { }
    }
}
