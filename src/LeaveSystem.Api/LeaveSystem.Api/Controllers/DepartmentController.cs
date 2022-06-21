using LeaveSystem.Db;
using LeaveSystem.Db.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentController : GenericCrudController<Department>
    {
        public DepartmentController(LeaveSystemDbContext dbContext)
            : base(dbContext)
        { }
    }
}
