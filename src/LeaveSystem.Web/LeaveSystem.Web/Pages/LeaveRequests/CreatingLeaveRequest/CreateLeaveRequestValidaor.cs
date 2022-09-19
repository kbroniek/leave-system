using FluentValidation;

namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
public class CreateLeaveRequestValidaor : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestValidaor()
    {
        RuleFor(p => p.DateFrom)
            .NotNull().WithMessage("You must enter the date from");
        RuleFor(p => p.DateTo)
            .NotNull().WithMessage("You must enter the date to");
        RuleFor(p => new { p.DateFrom, p.DateTo }).Must(p => p.DateFrom <= p.DateTo)
            .WithMessage("Date to must be greater than date from");
        RuleFor(p => p.LeaveTypeId)
            .NotNull().WithMessage("You must enter the type");
        RuleFor(p => p.Duration)
            .Must(p => p > TimeSpan.Zero && p < TimeSpan.FromDays(366))
            .WithMessage("Hours should be at range (0-366 days).");
    }
}
