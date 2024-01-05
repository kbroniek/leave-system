using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.LeaveRequests.ShowingLeaveRequests;

public class GetLeaveRequestsTest
{
    [Fact]
    public async Task WhenGetLeaveRequests_ThenReturnDesiredLeaveRequests()
    {
        //Given
        const int year = 2021;
        var data = FakeLeaveRequestShortInfoProvider.GetAll(DateTimeOffsetExtensions.CreateFromDate(year, 4, 5))
            .ToPagedListResponse();
        var mockHttp = new MockHttpMessageHandler();
        var firstDay = DateTimeOffsetExtensions.GetFirstDayOfYear(year);
        var lastDay = DateTimeOffsetExtensions.GetLastDayOfYear(year);
        var query = new GetLeaveRequestsQuery(firstDay, lastDay, 1, 1000);
        mockHttp.WhenWithBaseUrl(query.CreateQueryString("api/leaveRequests"))
            .RespondWithJson(data);
        var httpClientMock = Substitute.For<HttpClient>(mockHttp);
        httpClientMock.BaseAddress = new Uri(MockHttpHandlerExtensions.BaseFakeUrl);
        var sut = new GetLeaveRequestsService(httpClientMock);
        //When
        var result = await sut.GetLeaveRequests(query);
        //Then
        result.Should().BeEquivalentTo(data);
    }
}
