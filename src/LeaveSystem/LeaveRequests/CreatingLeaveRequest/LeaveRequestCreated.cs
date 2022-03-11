using GoldenEye.Backend.Core.DDD.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LeaveSystem.LeaveRequests.CreatingLeaveRequest
{
    internal class LeaveRequestCreated : IEvent
    {
        public Guid StreamId => LeaveRequestId;

        public Guid LeaveRequestId { get; }

        public DateTime DateFrom { get; }

        public DateTime DateTo { get; }

        public int? Hours { get; }

        public string? Type { get; }

        public string? Remarks { get; }

        [JsonConstructor]
        private LeaveRequestCreated(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
        {
            LeaveRequestId = leaveRequestId;
            DateFrom = dateFrom;
            DateTo = dateTo;
            Hours = hours;
            Type = type;
            Remarks = remarks;
        }
        public static LeaveRequestCreated Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
        {
            return new LeaveRequestCreated(leaveRequestId, dateFrom, dateTo, hours, type, remarks);
        }
    }
}
