using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.Controllers
{
    //TODO: Set permissions to get limits only for current user.
    [Route("api/[controller]")]
    [Authorize]
    public class UserLeaveLimitsController : GenericCrudController<UserLeaveLimit>
    {
        public UserLeaveLimitsController(LeaveSystemDbContext dbContext)
            : base(dbContext)
        { }
    }
}
