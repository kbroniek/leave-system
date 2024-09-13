namespace LeaveSystem.Shared.Dto;
using System;
using System.Text.Json.Serialization;
using static LeaveSystem.Shared.Dto.LeaveTypeDto;

public record LeaveTypeDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("order")] int Order,
    [property: JsonPropertyName("properties")] PropertiesDto? Properties,
    [property: JsonPropertyName("state")] LeaveTypeState State)
{
    public record PropertiesDto(
        [property: JsonPropertyName("color")] string? Color,
        [property: JsonPropertyName("catalog")] LeaveTypeCatalog? Catalog,
        [property: JsonPropertyName("includeFreeDays")] bool? IncludeFreeDays,
        [property: JsonPropertyName("defaultLimitDays")] int? DefaultLimitDays);

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveTypeState
    {
        Active,
        Inactive,
    };
}
