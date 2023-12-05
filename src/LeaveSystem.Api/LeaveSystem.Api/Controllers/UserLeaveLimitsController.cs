using Ardalis.GuardClauses;
using FluentValidation;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Date;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;

namespace LeaveSystem.Api.Controllers
{
    //TODO: Set permissions to get limits only for current user.
    [Route("api/[controller]")]
    [Authorize]
    public class UserLeaveLimitsController : GenericCrudController<UserLeaveLimit, Guid>
    {
        private readonly CurrentDateService currentDateService;
        private readonly IValidator<UserLeaveLimit> validator;

        public UserLeaveLimitsController(LeaveSystemDbContext dbContext, CurrentDateService currentDateService, IValidator<UserLeaveLimit> validator)
            : base(dbContext)
        {
            this.currentDateService = currentDateService;
            this.validator = validator;
        }

        [HttpPost]
        public override async Task<IActionResult> Post([FromBody] UserLeaveLimit entity, CancellationToken cancellationToken = default)
        {
            await validator.ValidateAsync(entity, cancellationToken);
            return await base.Post(entity, cancellationToken);
        }

        public override async Task<IActionResult> Patch(Guid key, Delta<UserLeaveLimit> update, CancellationToken cancellationToken = default)
        {
            ValidateDelta(update);
            return await base.Patch(key, update, cancellationToken);
        }

        protected void ValidateEntity(UserLeaveLimit entity)
        {
            var now = currentDateService.GetWithoutTime();
            var firstDay = now.GetFirstDayOfYear();
            var lastDay = now.GetLastDayOfYear();
            if (entity.ValidSince is not null)
            {
                Guard.Against.OutOfRange(entity.ValidSince.Value, nameof(entity.ValidSince), firstDay, lastDay);
            }
            if (entity.ValidUntil is not null)
            {
                Guard.Against.OutOfRange(entity.ValidUntil.Value, nameof(entity.ValidUntil), firstDay, lastDay);
            }
            if (entity.ValidSince > entity.ValidUntil)
            {
                throw new ArgumentOutOfRangeException(nameof(entity.ValidUntil),
                    "Valid until date has to be less than valid since date.");
            }
        }

        protected void ValidateDelta(Delta<UserLeaveLimit> delta)
        {
            var validSincePropertyExists = delta.TryGetPropertyValue("ValidSince", out var validSinceValue);
            var validSince = validSinceValue as DateTimeOffset?;
            var validUntilPropertyExists = delta.TryGetPropertyValue("ValidUntil", out var validUntilValue);
            var validUntil = validUntilValue as DateTimeOffset?;
            if (validSincePropertyExists && validUntilPropertyExists && validSince > validUntil)
            {
                throw new ArgumentOutOfRangeException(nameof(UserLeaveLimit.ValidUntil),
                    "Valid until date has to be less than valid since date.");
            }
        }
    }
}