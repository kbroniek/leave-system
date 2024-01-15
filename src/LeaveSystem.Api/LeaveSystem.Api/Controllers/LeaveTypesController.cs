using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace LeaveSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveTypesController : ODataController
    {
        private readonly GenericCrudService<LeaveType, Guid> crudService;

        public LeaveTypesController(GenericCrudService<LeaveType, Guid> crudService)
        {
            this.crudService = crudService;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<LeaveType>? Get()
        {
            return crudService.Get();
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public SingleResult<LeaveType> Get([FromODataUri] Guid key)
        {
            var entity = crudService.GetSingleAsQueryable(key);
            return SingleResult.Create(entity);
        }
    }
}
