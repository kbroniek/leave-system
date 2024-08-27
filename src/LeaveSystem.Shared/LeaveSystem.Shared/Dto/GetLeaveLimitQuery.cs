namespace LeaveSystem.Shared.Dto;
using System;

public record GetLeaveLimitQuery(DateOnly? DateFrom, DateOnly? DateTo);
