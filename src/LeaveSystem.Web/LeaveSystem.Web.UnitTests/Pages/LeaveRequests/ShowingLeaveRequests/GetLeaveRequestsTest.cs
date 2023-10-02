using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.LeaveRequests.ShowingLeaveRequests;

public class GetLeaveRequestsTest
{
    private HttpClient httpClientMock;
    private PagedListResponse<LeaveRequestShortInfo> data;
    private GetLeaveRequestsQuery query;

    private GetLeaveRequestsService GetSut()
    {
        return new GetLeaveRequestsService(httpClientMock);
    }

    [Fact]
    public async Task WhenGetLeaveRequests_ThenReturnDesiredLeaveRequests()
    {
        //Given
        const int year = 2021;
        data = FakeLeaveRequestShortInfoProvider.GetAll(DateTimeOffsetExtensions.CreateFromDate(year, 4, 5))
            .ToPagedListResponse(1000);
        var mockHttp = new MockHttpMessageHandler();
        var firstDay = DateTimeOffsetExtensions.GetFirstDayOfYear(year);
        var lastDay = DateTimeOffsetExtensions.GetLastDayOfYear(year);
        query = new GetLeaveRequestsQuery(firstDay, lastDay, 1, 1000);
        mockHttp.WhenWithBaseUrl(query.CreateQueryString("api/leaveRequests"))
            .RespondWithJson(data);
        httpClientMock = Substitute.For<HttpClient>(mockHttp);
        httpClientMock.BaseAddress = new Uri(MockHttpHandlerExtensions.BaseFakeUrl);
        var sut = GetSut();
        //When
        var result = await sut.GetLeaveRequests(query);
        //Then
        result.Should().BeEquivalentTo(data);
    }
}