using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.Services.LeaveType;
public record class CreateLeaveType(Guid LeaveTypeId, string Description, string? Abbreviation) { }
