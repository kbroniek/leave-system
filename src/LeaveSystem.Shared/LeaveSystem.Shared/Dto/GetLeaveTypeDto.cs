namespace LeaveSystem.Shared.Dto;
using System;

public record GetLeaveTypeDto(Guid Id,
                              string Name,
                              int Order,
                              GetLeaveTypeDto.LeaveTypeDtoProperties? Properties = null,
                              Guid? BaseLeaveTypeId = null)
{
    public record LeaveTypeDtoProperties(string? Color = null,
                                         bool? IncludeFreeDays = null,
                                         double? DefaultLimitDays = null,
                                         LeaveTypeCatalog? Catalog = null);
}
