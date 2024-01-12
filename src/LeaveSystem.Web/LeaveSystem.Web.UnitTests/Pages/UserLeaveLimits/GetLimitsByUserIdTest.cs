namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

using System.Text.Json;
using Moq;
using TestStuff.Providers;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class GetLimitsByUserIdTest
{
    [Fact]
    public async Task WhenDataIsNull_ThenReturnEmptyCollection()
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        const string userId = "174e1d5d-fb0f-4e94-82f5-ef3e1aadfa23";
        var since = DateTimeOffset.Parse("2024-01-01");
        var until = DateTimeOffset.Parse("2024-12-31");
        var data = new ODataResponse<IEnumerable<UserLeaveLimitDto>>();
        var uri = $"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))";
        universalHttpServiceMock.Setup(
                m => m.GetAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>(
                    uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(data);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);
        var result = await sut.GetAsync(userId, since, until);
        result.Should().BeEmpty();
        universalHttpServiceMock.Verify(m => m.GetAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>(uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()), Times.Once);
    }

    [Fact]
    public async Task WhenDataReceived_ThenReturnIt()
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        const string userId = "174e1d5d-fb0f-4e94-82f5-ef3e1aadfa23";
        var since = DateTimeOffset.Parse("2024-01-01");
        var until = DateTimeOffset.Parse("2024-12-31");
        var data = new ODataResponse<IEnumerable<UserLeaveLimitDto>>()
        {
            Data = FakeUserLeaveLimitsDtoProvider.GetAllUserLimits(2024)
        };
        var uri = $"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))";
        universalHttpServiceMock.Setup(
                m => m.GetAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>(
                    uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(data);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);
        var result = await sut.GetAsync(userId, since, until);
        result.Should().BeEquivalentTo(data.Data);
        universalHttpServiceMock.Verify(m => m.GetAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>(uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()), Times.Once);
    }
}
