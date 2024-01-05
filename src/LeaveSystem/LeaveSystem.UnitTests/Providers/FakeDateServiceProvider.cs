using LeaveSystem.Shared.Date;
using Moq;
using System;

namespace LeaveSystem.UnitTests.Providers;

internal class FakeDateServiceProvider
{
    internal static DateService GetDateService()
    {
        var dateServiceMock = new Mock<DateService>();
        dateServiceMock.Setup(x => x.UtcNowWithoutTime())
            .Returns(DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        return dateServiceMock.Object;
    }
}
