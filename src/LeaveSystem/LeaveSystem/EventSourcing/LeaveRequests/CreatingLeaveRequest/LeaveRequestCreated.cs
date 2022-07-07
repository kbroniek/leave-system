using GoldenEye.Events;
using LeaveSystem.Db;
using System.Text.Json.Serialization;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest
{
    internal class LeaveRequestCreated : IEvent
    {
        public Guid StreamId => LeaveRequestId;

        public Guid LeaveRequestId { get; }

        public DateTime DateFrom { get; }

        public DateTime DateTo { get; }

        public int? Hours { get; }

        public Guid? Type { get; }

        public string? Remarks { get; }

        public FederatedUser CreatedBy { get; }

        [JsonConstructor]
        private LeaveRequestCreated(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks, FederatedUser createdBy)
        {
            LeaveRequestId = leaveRequestId;
            DateFrom = dateFrom;
            DateTo = dateTo;
            Hours = hours;
            Type = type;
            Remarks = remarks;
            CreatedBy = createdBy;
        }
        public static LeaveRequestCreated Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks, FederatedUser createdBy)
            => new(leaveRequestId, dateFrom, dateTo, hours, type, remarks, createdBy);
    }
}
