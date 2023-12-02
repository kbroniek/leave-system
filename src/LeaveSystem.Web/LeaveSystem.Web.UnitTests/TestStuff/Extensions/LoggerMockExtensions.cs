using Castle.Core.Logging;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class LoggerMockExtensions
{
    public static void VerifyLogError<T>(this Mock<ILogger<T>> loggerMock, string? message, Func<Times> times)
        => loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString() == message && type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            times);
}