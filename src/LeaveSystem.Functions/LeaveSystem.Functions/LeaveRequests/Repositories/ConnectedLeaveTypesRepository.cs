namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class ConnectedLeaveTypesRepository(CosmosClient cosmosClient, string databaseName, string containerId) : IConnectedLeaveTypesRepository
{
    public async ValueTask<Result<(IEnumerable<Guid> nestedLeaveTypeIds, Guid? baseLeaveTypeId), Error>> GetConnectedLeaveTypeIds(
        Guid leaveTypeId, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var iterator = container.GetItemLinqQueryable<Entity>()
            .Where(x => x.BaseLeaveTypeId == leaveTypeId || x.Id == leaveTypeId)
            .ToFeedIterator();
        var leaveTypes = await iterator.ExecuteQuery(cancellationToken);

        if (leaveTypes.Count == 0)
        {
            return new Error($"Cannot find the leave type. Id={leaveTypeId}", System.Net.HttpStatusCode.NotFound);
        }

        var nestedLeaveTypes = leaveTypes
            .Where(l => l.BaseLeaveTypeId == leaveTypeId)
            .Select(x => x.Id)
            .ToList();
        var currentLeaveType = leaveTypes.FirstOrDefault(l => l.Id == leaveTypeId);
        return currentLeaveType is null ?
            new Error($"Cannot find the leave type. Id={leaveTypeId}", System.Net.HttpStatusCode.NotFound) :
            (nestedLeaveTypes, currentLeaveType.BaseLeaveTypeId);
    }

    private sealed record Entity(Guid Id, Guid? BaseLeaveTypeId);
}
