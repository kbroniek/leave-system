namespace LeaveSystem.Shared.Dto;
using System;

public record GetLeaveTypeDto(Guid Id,
                              string Name,
                              int Order,
                              GetLeaveTypeDto.LeaveTypePropertiesDto? Properties = null)
{
    public record LeaveTypePropertiesDto(string? Color = null,
                                         LeaveTypeCatalog? Catalog = null);
}
