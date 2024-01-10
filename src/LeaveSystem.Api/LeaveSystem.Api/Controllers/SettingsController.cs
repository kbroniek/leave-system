using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class SettingsController : GenericCrudController<Setting, string>
{
    public SettingsController(LeaveSystemDbContext dbContext)
        : base(dbContext)
    { }
}
