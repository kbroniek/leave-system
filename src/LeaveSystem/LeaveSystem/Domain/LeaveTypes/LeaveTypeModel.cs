namespace LeaveSystem.Domain.LeaveTypes;
using System;
using LeaveSystem.Shared;
using Newtonsoft.Json;

/// <summary>
/// This is because of cosmos DB serialization issues.
/// </summary>
public interface IHaveId
{
    [JsonProperty(PropertyName = "id")]
    public Guid Id { get; }
}

public record LeaveTypeModel(Guid? BaseLeaveTypeId, string Name, LeaveTypeModel.LeaveTypeProperties? Properties, int Order) : IHaveId
{
    public Guid Id { get; }
    [JsonConstructor]
    public LeaveTypeModel(Guid id, Guid? baseLeaveTypeId, string name, LeaveTypeProperties? properties, int order) :
        this(baseLeaveTypeId, name, properties, order) => this.Id = id;

    public record LeaveTypeProperties(string? Color, bool? IncludeFreeDays, TimeSpan? DefaultLimit, LeaveTypeCatalog? Catalog);
}
