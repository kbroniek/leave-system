using System.Linq.Dynamic.Core;
using System.Text.Json;
using FluentValidation;
using GoldenEye.Objects.General;
using LeaveSystem.Shared;
using LeaveSystem.Shared.FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Db.Entities;
public class UserLeaveLimit : IHaveId<Guid>
{
    private DateTimeOffset? validSince;
    private DateTimeOffset? validUntil;
    public Guid Id { get; set; }
    public TimeSpan? Limit { get; set; }
    public TimeSpan? OverdueLimit { get; set; }
    public string? AssignedToUserId { get; set; }
    public virtual LeaveType LeaveType { get; set; }
    public Guid LeaveTypeId { get; set; }

    public DateTimeOffset? ValidSince
    {
        get => validSince;
        set => validSince = value?.GetDayWithoutTime();
    }

    public DateTimeOffset? ValidUntil
    {
        get => validUntil;
        set => validUntil = value?.GetDayWithoutTime();
    }

    public UserLeaveLimitProperties? Property { get; set; }

    public class UserLeaveLimitProperties
    {
        public string? Description { get; set; }
    }
}

public class UserLeaveLimitValidator : AbstractValidator<UserLeaveLimit>
{
    private readonly LeaveSystemDbContext dbContext;
    private readonly ILogger<UserLeaveLimitValidator> logger;
    public UserLeaveLimitValidator(LeaveSystemDbContext dbContext, ILogger<UserLeaveLimitValidator> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        RuleFor(x => x.ValidSince)
            .LessThan(x => x.ValidUntil)
            .When(x => x.ValidUntil is not null || x.ValidSince is not null)
            .WithErrorCode(FvErrorCodes.ArgumentOutOfRange);
        RuleFor(x => x.ValidUntil)
            .Equal(x => x.ValidSince)
            .When(x => x.ValidUntil is null && x.ValidSince is null);
        RuleFor(x => x.LeaveTypeId)
            .MustAsync((leaveTypeId, cancellation) =>
                this.dbContext.LeaveTypes.AnyAsync(ull => ull.Id == leaveTypeId, cancellation))
            .WithMessage("Leave type with provided Id not exists");
        RuleFor(x => x.AssignedToUserId).NotNull().NotEmpty();
        RuleFor(x => x)
            .MustAsync(async (limit, cancellation) =>
            {
                var overlappingLimits = await GetAllLimitThatOverlapsPeriodAsync(
                    limit.Id, limit.LeaveTypeId, limit.AssignedToUserId!, limit.ValidSince, limit.ValidUntil,
                    cancellation);
                if (!overlappingLimits.Any())
                {
                    return true;
                }

                var overlappingLimitsJson = JsonSerializer.Serialize(overlappingLimits, new JsonSerializerOptions { WriteIndented = true });
                logger.LogError("Following Limits overlapping this limit:\n{OverlappingLimits}", overlappingLimitsJson);
                return false;
            })
            .WithMessage("Cannot create a new limit in this time. The other limit is overlapping with this date.");
    }
    
    private Task<List<UserLeaveLimit>> GetAllLimitThatOverlapsPeriodAsync(
        Guid id,
        Guid leaveTypeId,
        string userId,
        DateTimeOffset? dateFrom, 
        DateTimeOffset? dateTo,
        CancellationToken cancellationToken)
    {
        return dbContext.UserLeaveLimits.Where(
            ull =>
                ull.Id != id &&
                ull.LeaveTypeId == leaveTypeId && 
                ull.AssignedToUserId == userId &&
                !(
                    // checking if periods can't overlap
                    (!ull.ValidSince.HasValue && !ull.ValidUntil.HasValue) ||
                    (!dateFrom.HasValue && !dateTo.HasValue) ||
                    (!ull.ValidSince.HasValue && !dateFrom.HasValue) ||
                    (!ull.ValidUntil.HasValue && !dateTo.HasValue)
                ) && (
                    // checking if periods overlaps
                    (!ull.ValidSince.HasValue && ull.ValidUntil >= dateFrom && ull.ValidUntil >= dateTo) ||
                    (!dateFrom.HasValue && dateTo >= ull.ValidSince && dateTo >= ull.ValidUntil) ||
                    (!ull.ValidUntil.HasValue && ull.ValidSince >= dateFrom && ull.ValidSince >= dateTo) ||
                    (!dateTo.HasValue && dateFrom >= ull.ValidSince && dateFrom >= ull.ValidUntil) ||
                    (ull.ValidSince <= dateTo && dateFrom <= ull.ValidUntil)
                )).ToListAsync(cancellationToken);
    }
}
