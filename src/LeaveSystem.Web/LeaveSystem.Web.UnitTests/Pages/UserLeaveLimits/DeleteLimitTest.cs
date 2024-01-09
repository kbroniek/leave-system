namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

using System.Text.Json;
using LeaveSystem.Shared.Converters;
using Moq;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class DeleteLimitTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenEntityUpdated_ThenReturnUpdatingResult(bool deleteResult)
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var fakeLimitId = Guid.Parse("22ba1b02-2e35-40ae-9191-20b88f8fba49");
        universalHttpServiceMock.Setup(m =>
                m.DeleteAsync(
                    $"odata/UserLeaveLimits({fakeLimitId})",
                    It.IsAny<string>(),
                    It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(deleteResult);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);

        var result = await sut.DeleteAsync(fakeLimitId);
        result.Should().Be(deleteResult);
        universalHttpServiceMock.Verify(
            m => m.DeleteAsync(
                $"odata/UserLeaveLimits({fakeLimitId})",
                "Successfully deleted leave limit",
                It.Is<JsonSerializerOptions>(
                    o => o.Converters.Any(c => c.GetType() == typeof(TimeSpanIso8601Converter)))));
    }
}
