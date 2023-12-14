using FluentValidation;
using GoldenEye.Objects.General;
using LeaveSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Db.Entities;

public class LeaveType : IHaveId<Guid>
{
    public Guid Id { get; set; }
    public Guid? BaseLeaveTypeId { get; set; }
    public string Name { get; set; }
    public LeaveTypeProperties? Properties { get; set; }
    public virtual LeaveType? BaseLeaveType { get; set; }
    public virtual ICollection<LeaveType>? ConstraintedLeaveTypes { get; set; }
    public virtual ICollection<UserLeaveLimit>? UserLeaveLimits { get; set; }
    public int Order { get; set; }
    public class LeaveTypeProperties
    {
        public string? Color { get; set; }
        public bool? IncludeFreeDays { get; set; }
        public TimeSpan? DefaultLimit { get; set; }
        public LeaveTypeCatalog? Catalog { get; set; }
    }
}

public class LeaveTypeValidator : AbstractValidator<LeaveType>
{
    private readonly LeaveSystemDbContext dbContext;

    public LeaveTypeValidator(LeaveSystemDbContext dbContext)
    {
        this.dbContext = dbContext;
        RuleFor(x => x.BaseLeaveTypeId)
            .MustAsync(async (baseLeaveTypeId, cancellation) =>
            {
                var baseLeaveTypeExists = await this.dbContext.LeaveTypes.AnyAsync(lt => lt.BaseLeaveTypeId == baseLeaveTypeId, cancellation);
                return !baseLeaveTypeExists;
            })
            .WithMessage("Base leave type with provided Id not exists")
            .When(x => x.BaseLeaveTypeId is not null);
        RuleFor(x => x.Order)
            .MustAsync((order, cancellation) => this.dbContext.LeaveTypes.AnyAsync(x => x.Order == order, cancellation))
            .WithMessage("Leave type with this order already exists");
        RuleFor(x => x.Name)
            .NotNull().NotEmpty();
    }
}
