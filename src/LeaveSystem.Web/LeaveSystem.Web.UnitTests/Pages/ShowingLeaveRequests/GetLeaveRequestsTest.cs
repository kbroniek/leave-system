using System.Net.Http.Json;
using System.Text.Json;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using RichardSzalay.MockHttp;
using MockHttpMessageHandlerExtensions = RichardSzalay.MockHttp.MockHttpMessageHandlerExtensions;

namespace LeaveSystem.Web.UnitTests.Pages.ShowingLeaveRequests;


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
            .ToPagedListResponse(1000);
        var mockHttp = new MockHttpMessageHandler();
        var firstDay = DateTimeOffsetExtensions.GetFirstDayOfYear(year);
        var lastDay = DateTimeOffsetExtensions.GetLastDayOfYear(year);
        query = new GetLeaveRequestsQuery(firstDay, lastDay, 1, 1000);
        mockHttp.WhenWithBaseUrl(query.CreateQueryString("api/leaveRequests"))
            .RespondWithJson(data);
        httpClientMock = Substitute.For<HttpClient>(mockHttp);
        httpClientMock.BaseAddress = new Uri(MockHttpHandlerExtensions.BaseFakeUrl);
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