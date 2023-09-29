using System.Text.Json;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.LeaveRequests.ShowingLeaveRequests;


//Todo: decide if remove this test
public class GetLeaveRequestsTest
{
    private HttpClient httpClientMock;
    private PagedListResponse<LeaveRequestShortInfo> data;
    private GetLeaveRequestsQuery query;

    public GetLeaveRequestsTest()
    {
        const int year = 2021;
        data = FakeLeaveRequestShortInfoProvider.GetAll(DateTimeOffsetExtensions.CreateFromDate(year, 4, 5))
            .ToPagedListResponse();

        var firstDay = DateTimeOffsetExtensions.GetFirstDayOfYear(year);
        var lastDay = DateTimeOffsetExtensions.GetLastDayOfYear(year);
        query = new GetLeaveRequestsQuery(firstDay, lastDay, 1, 1000);
        httpClientMock = HttpClientMockFactory.CreateWithJsonResponse(query.CreateQueryString("api/leaveRequests"), data, JsonSerializerOptions.Default);
        // httpClientMock.BaseAddress = new Uri(MockHttpHandlerExtensions.BaseFakeUrl);
    }

    private GetLeaveRequestsService GetSut()
    {
        return new GetLeaveRequestsService(httpClientMock);
    }

    [Fact]
    public async Task WhenGetLeaveRequests_ThenReturnDesiredLeaveRequests()
    {
        //Given
        var sut = GetSut();
        //When
        var result = await sut.GetLeaveRequests(query);
        //Then
        result.Should().BeEquivalentTo(data);
    }
}