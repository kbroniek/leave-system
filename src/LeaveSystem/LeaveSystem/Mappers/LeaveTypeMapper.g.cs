using LeaveSystem.Api.Domains;
using LeaveSystem.Mappers;
using LeaveSystem.Services.LeaveType;

namespace LeaveSystem.Mappers
{
    public partial class LeaveTypeMapper : ILeaveTypeMapper
    {
        public LeaveType MapTo(CreateLeaveType p1)
        {
            return p1 == null ? null : new LeaveType()
            {
                LeaveTypeId = p1.LeaveTypeId,
                Description = p1.Description,
                Abbreviation = p1.Abbreviation
            };
        }
    }
}