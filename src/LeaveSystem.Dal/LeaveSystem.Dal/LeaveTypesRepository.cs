namespace LeaveSystem.Dal;

using LeaveSystem.Dal.Extensions;
using LeaveSystem.Domain.LeaveTypes;
using Microsoft.Azure.Cosmos;

public class LeaveTypesRepository
{
    private readonly Container leaveTypesContainer;

    public LeaveTypesRepository(CosmosClient cosmosClient)
    {
        this.leaveTypesContainer = cosmosClient.GetDatabase("LeaveSystem").GetContainer("LeaveTypes");
    }

    public async Task<IEnumerable<LeaveTypeModel>> GetLeaveTypes() =>
        await leaveTypesContainer.GetItemLinqQueryable<LeaveTypeModel>().ToListAsync();
    public async Task<IEnumerable<LeaveTypeModel>> GetLeaveType(Guid id) =>
        await leaveTypesContainer.GetItemLinqQueryable<LeaveTypeModel>().ToListAsync();
    public async Task UpsertLeaveType(LeaveTypeModel leaveTypeModel) =>
        await leaveTypesContainer.UpsertItemAsync(leaveTypeModel, new PartitionKey(partitionKeyValue: leaveTypeModel.Name));
}
