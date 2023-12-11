using System.Text.Json;
using Ardalis.GuardClauses;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Settings;
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
    public class SettingsController : ODataController
    {
        private readonly GenericCrudService<Setting, string> crudService;

        public SettingsController(GenericCrudService<Setting, string> crudService)
        {
            this.crudService = crudService;
        }
        
        [HttpGet]
        [EnableQuery]
        public IQueryable<Setting>? Get()
        {
            return crudService.Get();
        }

        // [HttpGet]
        // [EnableQuery]
        // public SingleResult<Setting> Get([FromODataUri] string key)
        // {
        //     var entity = crudService.GetAsQueryable(key);
        //     return SingleResult.Create(entity);
        // }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddSettingDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new Setting
            {
                Id = Guard.Against.NullOrWhiteSpace(dto.Id),
                Value = Guard.Against.NillAndDefault(dto.Value),
                Category = dto.Category

            };
            var createdEntity = await crudService.AddAsync(entity, cancellationToken);
            return Created(createdEntity);
        }
    }
}
