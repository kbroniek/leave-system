using GoldenEye.Extensions.Collections;
using LeaveSystem.Db;
using LeaveSystem.Exceptions;
using LeaveSystem.Mappers;
using Microsoft.EntityFrameworkCore;

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
    public virtual async Task Create(CreateLeaveType createRequest, CancellationToken cancellation = default)
    {
        var leaveType = GetDataSet();
        var entity = leaveTypeMapper.MapTo(createRequest);
        await leaveType.AddAsync(entity, cancellation);
        await dbContext.SaveChangesAsync(cancellation);
    }

    public virtual Task<List<Api.Domains.LeaveType>> GetAll(int? page, int? pageSize, CancellationToken cancellation = default)
    {
        var leaveType = GetDataSet();
        return leaveType.Page(page, pageSize).ToListAsync(cancellation);
    }

    public virtual Task<Api.Domains.LeaveType> Get(Guid id, CancellationToken cancellation = default)
    {
        var leaveType = GetDataSet();
        return FindLeaveType(id, leaveType, cancellation);
    }

    public virtual async Task<Api.Domains.LeaveType> Update(UpdateLeaveType updateRequest, CancellationToken cancellation = default)
    {
        var leaveType = GetDataSet();
        var leaveTypeEntity = await FindLeaveType(updateRequest.LeaveTypeId, leaveType, cancellation);
        leaveTypeMapper.MapToExisting(updateRequest, leaveTypeEntity);
        await dbContext.SaveChangesAsync();
        return leaveTypeEntity;
    }

    public virtual async Task Remove(Guid id, CancellationToken cancellation = default)
    {
        var leaveType = GetDataSet();
        var leaveTypeEntity = await FindLeaveType(id, leaveType, cancellation);
        leaveType.Remove(leaveTypeEntity);
        await dbContext.SaveChangesAsync();
    }

    private DbSet<Api.Domains.LeaveType> GetDataSet()
    {
        if (dbContext.LeaveTypes == null)
        {
            throw HttpResponseException.InternalServerError($"The {nameof(dbContext.LeaveTypes)} table in the {nameof(LeaveSystemDbContext)} is null.");
        }
        return dbContext.LeaveTypes;
    }

    private static async Task<Api.Domains.LeaveType> FindLeaveType(Guid id, DbSet<Api.Domains.LeaveType> leaveType, CancellationToken cancellation)
    {
        var leaveTypeEntity = await leaveType.FindAsync(new object[] { id }, cancellation);
        if (leaveTypeEntity == null)
        {
            throw HttpResponseException.NotFound($"Cannot find the leave type by id {id}");
        }

        return leaveTypeEntity;
    }
}