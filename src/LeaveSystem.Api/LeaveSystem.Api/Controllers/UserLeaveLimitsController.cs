using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserLeaveLimitsController : GenericCrudController<UserLeaveLimit>
    {
        public UserLeaveLimitsController(LeaveSystemDbContext dbContext)
            : base(dbContext)
        { }
    }
}
