using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.UnitTests.TestStuff.Converters;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;

namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

public class GetLimitsByUserIdTest
{
    private HttpClient httpClient;

    public UserLeaveLimitsService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenGettingResponse_ThenReturnDeserializedLimits()
    {
        //Given
        var userId = Guid.NewGuid().ToString();
        var since = DateTimeOffsetExtensions.CreateFromDate(2020, 1, 3);
        var until = DateTimeOffsetExtensions.CreateFromDate(2021, 5, 24);
        var data = new[]
        {
            new
            {
                Limit = TimeSpan.FromHours(16), 
                OverdueLimit = TimeSpan.FromHours(4), 
                LeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId, 
                ValidSince = DateTimeOffsetExtensions.CreateFromDate(2020, 1, 4),
                ValidUntil = DateTimeOffsetExtensions.CreateFromDate(2020, 3, 6), 
                Property = new
                {
                    Description = "fake desc"
                },
            },
            new
            {
                Limit = TimeSpan.FromHours(8), 
                OverdueLimit = TimeSpan.FromHours(16), 
                LeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId, 
                ValidSince = DateTimeOffsetExtensions.CreateFromDate(2021, 1, 4),
                ValidUntil = DateTimeOffsetExtensions.CreateFromDate(2021, 4, 6), 
                Property = new
                {
                    Description = "fake desc"
                }
            },
            new
            {
                Limit = TimeSpan.FromHours(16), 
                OverdueLimit = TimeSpan.FromHours(4), 
                LeaveTypeId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid, 
                ValidSince = DateTimeOffsetExtensions.CreateFromDate(2020, 9, 4),
                ValidUntil = DateTimeOffsetExtensions.CreateFromDate(2020, 12, 6), 
                Property = new
                {
                    Description = "fake desc"
                }
            }
        }.ToODataResponse();
        var url =
            $"odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, data, new JsonSerializerOptions { Converters = { new TimeSpanToStringConverter() }});
        var sut = GetSut();
        //When
        var result = await sut.GetLimits(userId, since, until);
        //Then
        result.Should().BeEquivalentTo(data.Data);
    }

    [Theory]
    [MemberData(nameof(Get_WhenResponseDataIsNull_ThenReturnEmptyCollection_TestData))]
    public async Task WhenResponseDataIsNull_ThenReturnEmptyCollection(ODataResponse<UserLeaveLimitsService.UserLeaveLimitDto> userLeaveLimits)
    {
        //Given
        var userId = Guid.NewGuid().ToString();
        var since = DateTimeOffsetExtensions.CreateFromDate(2020, 1, 3);
        var until = DateTimeOffsetExtensions.CreateFromDate(2021, 5, 24);
        var url =
            $"odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, userLeaveLimits, new JsonSerializerOptions { Converters = { new TimeSpanToStringConverter() }});
        var sut = GetSut();
        //When
        var result = await sut.GetLimits(userId, since, until);
        //Then
        result.Should().BeEquivalentTo(Enumerable.Empty<object>());
    }

    public static IEnumerable<object[]> Get_WhenResponseDataIsNull_ThenReturnEmptyCollection_TestData()
    {
        yield return new object[] { null };
        yield return new object[] { new ODataResponse<UserLeaveLimitsService.UserLeaveLimitDto>() };
    }


}