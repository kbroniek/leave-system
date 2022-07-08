using Ardalis.GuardClauses;
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

        public TimeSpan Duration { get; }

        public Guid Type { get; }

        public string? Remarks { get; }

        public FederatedUser CreatedBy { get; }

        [JsonConstructor]
        private LeaveRequestCreated(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, TimeSpan duration, Guid type, string? remarks, FederatedUser createdBy)
        {
            LeaveRequestId = leaveRequestId;
            DateFrom = dateFrom;
            DateTo = dateTo;
            Duration = duration;
            Type = type;
            Remarks = remarks;
            CreatedBy = createdBy;
        }
        public static LeaveRequestCreated Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, TimeSpan duration, Guid type, string? remarks, FederatedUser createdBy)
        {
            leaveRequestId = Guard.Against.Default(leaveRequestId);
            dateFrom = Guard.Against.Default(dateFrom);
            dateTo = Guard.Against.Default(dateTo);
            type = Guard.Against.Default(type);
            duration = Guard.Against.Default(duration);

            dateFrom = Guard.Against.OutOfSQLDateRange(dateFrom);
            dateTo = Guard.Against.OutOfSQLDateRange(dateTo);

            var now = DateTime.UtcNow;
            var firstDay = new DateTime(now.Year, 1, 1);
            var lastDay = new DateTime(now.Year, 12, 31);
            Guard.Against.OutOfRange(dateFrom, nameof(dateFrom), firstDay, lastDay);
            Guard.Against.OutOfRange(dateTo, nameof(dateTo), firstDay, lastDay);

            if(firstDay > lastDay)
            {
                throw new ArgumentOutOfRangeException("Date from has to be less than date to.");
            }

            return new(leaveRequestId, dateFrom, dateTo, duration, type, remarks, createdBy);
        }
    }
}
