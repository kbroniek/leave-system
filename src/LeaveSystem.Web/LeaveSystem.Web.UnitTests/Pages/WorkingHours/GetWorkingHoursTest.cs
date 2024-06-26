using System.Text.Json;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using FakeWorkingHoursProvider = LeaveSystem.UnitTests.Providers.FakeWorkingHoursProvider;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.WorkingHours;
using Moq;
using Web.Shared;

public class GetWorkingHoursTest
{
    public static IEnumerable<object[]> WhenDataReceived_ThenReturnIt_MemberData
    {
        get
        {
            yield return new object[] { EnumerableExtensions.ToPagedListResponse(FakeWorkingHoursProvider.GetAll(new DateService().UtcNowWithoutTime()).ToDto()) };
            yield return new object[] { null! };
        }
    }
    [Theory]
    [MemberData(nameof(WhenDataReceived_ThenReturnIt_MemberData))]
    public async Task WhenDataReceived_ThenReturnIt(PagedListResponse<WorkingHoursDto> data)
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var query = GetWorkingHoursQuery.GetDefault();
        var uri = query.CreateQueryString("api/workingHours");
        universalHttpServiceMock.Setup(
                m => m.GetAsync<PagedListResponse<WorkingHoursDto>>(
                    uri, "Error occured during getting working hours", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(data);
        var sut = new WorkingHoursService(universalHttpServiceMock.Object);
        var result = await sut.GetWorkingHoursAsync(query);
        result.Should().BeEquivalentTo(data);
        universalHttpServiceMock.Verify(m => m.GetAsync<PagedListResponse<WorkingHoursDto>>(
            uri, "Error occured during getting working hours", It.IsAny<JsonSerializerOptions>()), Times.Once);
    }
}
