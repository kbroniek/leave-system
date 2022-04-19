using LeaveSystem.Api.Domains;
using LeaveSystem.Services.LeaveType;
using Mapster;

namespace LeaveSystem.Mappers
{
    [Mapper]
    public interface ILeaveTypeMapper
    {
        public LeaveType MapTo(CreateLeaveType contact);
        public LeaveType MapToExisting(UpdateLeaveType contact, LeaveType leaveType);
    }
}
