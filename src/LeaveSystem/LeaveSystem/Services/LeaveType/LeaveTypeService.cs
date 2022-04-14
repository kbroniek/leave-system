using LeaveSystem.Db;
using LeaveSystem.Exceptions;
using LeaveSystem.Mappers;

namespace LeaveSystem.Services.LeaveType;
public class LeaveTypeService
{
    private readonly LeaveSystemDbContext dbContext;
    private readonly ILeaveTypeMapper leaveTypeMapper;

    public LeaveTypeService(LeaveSystemDbContext dbContext, ILeaveTypeMapper leaveTypeMapper)
    {
        this.dbContext = dbContext;
        this.leaveTypeMapper = leaveTypeMapper;
    }
    public virtual async Task Create(CreateLeaveType createRequest)
    {
        if (dbContext.LeaveTypes == null)
        {
            throw HttpResponseException.InternalServerError($"The {nameof(dbContext.LeaveTypes)} table in the {nameof(LeaveSystemDbContext)} is null.");
        }
        var entity = leaveTypeMapper.MapTo(createRequest);
        await dbContext.LeaveTypes.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }
}