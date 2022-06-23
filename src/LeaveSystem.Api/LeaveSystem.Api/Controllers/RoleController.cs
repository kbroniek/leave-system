﻿using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : GenericCrudController<LeaveType>
    {
        public RoleController(LeaveSystemDbContext dbContext)
            : base(dbContext)
        { }
    }
}
