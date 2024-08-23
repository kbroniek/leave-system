namespace LeaveSystem.Domain.LeaveRequests.Getting;
using System;
using System.Collections.Generic;

public interface IGetLeaveRequestRepository
{
    IAsyncEnumerable<object> ReadStreamAsync(Guid streamId);
}
