namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

using System.Text.Json;
using Moq;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class GetSingleLimitTest
{

    [Fact]
    public async Task WhenResponseIsNull_ThenReturnNull()
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var limitId = Guid.Parse("174e1d5d-fb0f-4e94-82f5-ef3e1aadfa23");
        var uri = $"odata/UserLeaveLimits({limitId})?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property";
        universalHttpServiceMock.Setup(
                m => m.GetAsync<UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
                    uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync((UserLeaveLimitsService.UserLeaveLimitDtoODataResponse?)null);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);
        var result = await sut.GetAsync(limitId);
        result.Should().BeNull();
        universalHttpServiceMock.Verify(m => m.GetAsync<UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
                uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task WhenDataIsPresentInResponse_ThenReturnIt()
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var limitId = Guid.Parse("174e1d5d-fb0f-4e94-82f5-ef3e1aadfa23");
        var data = new UserLeaveLimitsService.UserLeaveLimitDtoODataResponse()
        {
            ContextUrl = "fakeCtxUrl",
            Id = limitId,
            Limit = TimeSpan.FromHours(32),
            OverdueLimit = TimeSpan.FromHours(4),
            LeaveTypeId = Guid.Parse("3b555635-08d4-4247-b13d-88cf046c48c9"),
            ValidSince = DateTimeOffset.Parse("2024-01-01"),
            ValidUntil = DateTimeOffset.Parse("2024-12-31"),
            Property = new UserLeaveLimitPropertyDto("desc")
        };
        var uri = $"odata/UserLeaveLimits({limitId})?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property";
        universalHttpServiceMock.Setup(
                m => m.GetAsync<UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
                    uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(data);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);
        var result = await sut.GetAsync(limitId);
        result.Should().BeEquivalentTo(new
        {
            Id = limitId,
            Limit = TimeSpan.FromHours(32),
            OverdueLimit = TimeSpan.FromHours(4),
            LeaveTypeId = Guid.Parse("3b555635-08d4-4247-b13d-88cf046c48c9"),
            ValidSince = DateTimeOffset.Parse("2024-01-01"),
            ValidUntil = DateTimeOffset.Parse("2024-12-31"),
            Property = new UserLeaveLimitPropertyDto("desc")
        });
        universalHttpServiceMock.Verify(m => m.GetAsync<UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
            uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()),
            Times.Once);
    }
}
