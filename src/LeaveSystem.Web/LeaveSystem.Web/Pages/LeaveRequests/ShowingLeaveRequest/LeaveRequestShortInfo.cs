﻿using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequest;

public record class LeaveRequestShortInfo(
    Guid Id,
    DateTimeOffset DateFrom,
    DateTimeOffset DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    LeaveRequestStatus Status,
    FederatedUser? CreatedBy
);
