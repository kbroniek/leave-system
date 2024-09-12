namespace LeaveSystem.Shared.Dto;
using System;
using System.Text.Json.Serialization;

public record LeaveTypeDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("order")] int Order,
    [property: JsonPropertyName("properties")] LeaveTypeDto.PropertiesDto? Properties)
{
    public record PropertiesDto(
        [property: JsonPropertyName("color")] string? Color,
        [property: JsonPropertyName("catalog")] LeaveTypeCatalog? Catalog,
        [property: JsonPropertyName("includeFreeDays")] bool? IncludeFreeDays,
        [property: JsonPropertyName("defaultLimitDays")] int? DefaultLimitDays);
}
