using LeaveSystem.Shared.Date;
using Moq;

namespace LeaveSystem.Api.UnitTests.Providers;

internal class FakeDateServiceProvider
{
    internal static DateService GetDateService()
    {
        var dateServiceMock = new Mock<DateService>();
        dateServiceMock.Setup(x => x.UtcNowWithoutTime())
            .Returns(DateTimeOffset.Parse("2023-12-15T09:40:41.8087272+00:00"));
        return dateServiceMock.Object;
    }
}
