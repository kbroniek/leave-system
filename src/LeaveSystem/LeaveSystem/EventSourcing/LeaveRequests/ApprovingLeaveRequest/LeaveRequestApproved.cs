using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.EventSourcing.LeaveRequests.ApprovingLeaveRequest
{
    internal class LeaveRequestApproved : IEvent
    {
        public Guid StreamId => LeaveRequestId;

        public Guid LeaveRequestId { get; }

        public string? Remarks { get; }

        public FederatedUser ApprovedBy { get; }


        private LeaveRequestApproved(Guid leaveRequestId, string? remarks, FederatedUser approvedBy)
        {
            ApprovedBy = approvedBy;
            Remarks = remarks;
            LeaveRequestId = leaveRequestId;
        }

        public static LeaveRequestApproved Create(Guid leaveRequestId, string? remarks, FederatedUser approvedBy)
        {
            leaveRequestId = Guard.Against.Default(leaveRequestId);
            approvedBy = Guard.Against.Nill(approvedBy);
            Guard.Against.InvalidEmail(approvedBy.Email, $"{nameof(approvedBy)}.{nameof(approvedBy.Email)}");
            return new(leaveRequestId, remarks, approvedBy);
        }
    }
}
