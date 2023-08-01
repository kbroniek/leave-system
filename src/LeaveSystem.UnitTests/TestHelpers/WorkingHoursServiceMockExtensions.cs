using System;
using System.Threading;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using Moq;

namespace LeaveSystem.UnitTests.TestHelpers;

internal static class WorkingHoursServiceMockExtensions
{
    internal static void SetupGetUserSingleWorkingHoursDuration(this Mock<WorkingHoursService> source, 
        string createdByUserId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan result)
    {
        source.Setup(x =>
            x.GetUserSingleWorkingHoursDuration(
                createdByUserId,
                dateFrom,
                dateTo,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(result);
    }
}