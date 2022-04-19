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
                Properties = p1.Properties
            };
        }
        public LeaveType MapToExisting(UpdateLeaveType p2, LeaveType p3)
        {
            if (p2 == null)
            {
                return null;
            }
            LeaveType result = p3 ?? new LeaveType();
            
            result.LeaveTypeId = p2.LeaveTypeId;
            result.Description = p2.Description;
            result.Properties = p2.Properties;
            return result;
            
        }
    }
}