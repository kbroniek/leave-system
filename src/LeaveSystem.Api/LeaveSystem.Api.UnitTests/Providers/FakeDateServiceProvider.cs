using LeaveSystem.Shared.Date;
using Moq;
using System;

namespace LeaveSystem.Api.UnitTests.Providers;

internal class FakeDateServiceProvider
{
    internal static CurrentDateService GetDateService()
    {
        var dateServiceMock = new Mock<CurrentDateService>();
        dateServiceMock.Setup(x => x.GetWithoutTime())
            .Returns(DateTimeOffset.Parse("2023-12-15T09:40:41.8087272+01:00"));
        return dateServiceMock.Object;
    }
}
