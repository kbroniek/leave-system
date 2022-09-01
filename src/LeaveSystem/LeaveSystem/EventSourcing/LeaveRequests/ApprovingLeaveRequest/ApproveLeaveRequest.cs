using Ardalis.GuardClauses;
using GoldenEye.Commands;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.EventSourcing.LeaveRequests.ApprovingLeaveRequest;

public class ApproveLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }
    public string? Remarks { get; }
    public FederatedUser ApprovedBy { get; }

    private ApproveLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser approvedBy)
    {
        ApprovedBy = approvedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static ApproveLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? approvedBy)
    {
        leaveRequestId = Guard.Against.Nill(leaveRequestId);
        approvedBy = Guard.Against.Nill(approvedBy);
        return new(leaveRequestId.Value, remarks, approvedBy);
    }
}

