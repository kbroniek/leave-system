using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.Services.LeaveType;
public record class UpdateLeaveType(Guid LeaveTypeId, string Description, string? Properties) { }
