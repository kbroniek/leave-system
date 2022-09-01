using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.EventSourcing.LeaveRequests.ApprovingLeaveRequest;

public class ApproveLeaveRequestValidator
{
    private readonly IDocumentSession documentSession;

    public ApproveLeaveRequestValidator(IDocumentSession documentSession)
    {
        this.documentSession = documentSession;
    }

    //public virtual async Task ValidateState(LeaveRequest creatingLeaveRequest, TimeSpan minDuration, TimeSpan maxDuration, bool? includeFreeDays)
    //{
    //    var leaveRequestFromDb = await documentSession.Events.AggregateStreamAsync<LeaveRequest>(@event.StreamId);
    //}
}

