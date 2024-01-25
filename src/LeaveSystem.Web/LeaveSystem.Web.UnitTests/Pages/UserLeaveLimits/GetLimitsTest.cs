namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

using System.Text.Json;
using Moq;
using TestStuff.Providers;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class GetLimitsTest
{
    [Fact]
    public async Task WhenDataIsNull_ThenReturnEmptyCollection()
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var since = DateTimeOffset.Parse("2024-01-01");
        var until = DateTimeOffset.Parse("2024-12-31");
        var data = new ODataResponse<IEnumerable<LeaveLimitDto>>();
        var uri = $"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property,AssignedToUserId&$filter=not(AssignedToUserId eq null) and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))";
        universalHttpServiceMock.Setup(
                m => m.GetAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>(
                    uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(data);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);
        var result = await sut.GetAsync(since, until);
        result.Should().BeEmpty();
        universalHttpServiceMock.Verify(m => m.GetAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>(uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()), Times.Once);
    }

    [Fact]
    public async Task WhenDataReceived_ThenReturnIt()
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var since = DateTimeOffset.Parse("2024-01-01");
        var until = DateTimeOffset.Parse("2024-12-31");
        var data = new ODataResponse<IEnumerable<LeaveLimitDto>>()
        {
            Data = FakeUserLeaveLimitsDtoProvider.GetAllLimits()
        };
        var uri =$"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property,AssignedToUserId&$filter=not(AssignedToUserId eq null) and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))";
        universalHttpServiceMock.Setup(
                m => m.GetAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>(
                    uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(data);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);
        var result = await sut.GetAsync(since, until);
        result.Should().BeEquivalentTo(data.Data);
        universalHttpServiceMock.Verify(m => m.GetAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>(uri, "Can't get user leave limit", It.IsAny<JsonSerializerOptions>()), Times.Once);
    }
}
