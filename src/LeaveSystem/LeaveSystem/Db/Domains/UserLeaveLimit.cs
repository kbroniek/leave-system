using GoldenEye.Objects.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.Db.Domains;
public class UserLeaveLimit : IHaveId<Guid>
{
    public Guid UserLeaveLimitId { get; set; }
    public TimeSpan Limit { get; set; }
    public TimeSpan? OverdueLimit { get; set; }
    public FederatedUser? User { get; set; }
    public LeaveType? LeaveType { get; set; }
    public Guid? LeaveTypeId { get; set; }
    public DateTime ValidSince { get; set; }
    public DateTime ValidUntil { get; set; }
    public UserLeaveLimitProperties? Property { get; set; }
    public Guid Id => UserLeaveLimitId;

    public class UserLeaveLimitProperties
    {
        public string? Description { get; set; }
    }
}
