using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using Marten.Linq.Includes;

namespace LeaveSystem.Web.UnitTests.Pages.LeaveTypes;

public class GetLeaveTypeTest
{
    private HttpClient httpClient;

    private LeaveTypesService GetSut() => new (httpClient);
    
    [Fact]
    public async Task WhenGettingTypeWithThisId_ThenReturnIt()
    {
        //Given
        var fakeLeaveTypeId = Guid.NewGuid();
        var data = new
        {
            Id = fakeLeaveTypeId,
            BaseLeaveTypeId = (Guid?)null,
            Name = "urlop wypoczynkowy",
            Properties = new
            {
                Color = "yellow",
                Catalog = "Holiday",
                IncludeFreeDays = (bool?)null
            }
        };
        var url = $"odata/LeaveTypes({fakeLeaveTypeId})?$select=Id,BaseLeaveTypeId,Name,Properties";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, data);
        var sut = GetSut();
        //When
        var result = await sut.GetLeaveType(fakeLeaveTypeId);
        //When
        result.Should().BeEquivalentTo(new
        {
            Id = fakeLeaveTypeId,
            BaseLeaveTypeId = (Guid?)null,
            Name = "urlop wypoczynkowy",
            Properties = new
            {
                Color = "yellow",
                Catalog = LeaveTypeCatalog.Holiday,
                IncludeFreeDays = (bool?)null
            }
        });
    }

    [Fact]
    public async Task WhenRequestReturnedNoResults_ThenReturnNull()
    {
        //Given
        var fakeLeaveTypeId = Guid.NewGuid();
        var url = $"odata/LeaveTypes({fakeLeaveTypeId})?$select=Id,BaseLeaveTypeId,Name,Properties";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse<LeaveTypesService.LeaveTypeDto>(url, null);
        var sut = GetSut();
        //When
        var result = await sut.GetLeaveType(fakeLeaveTypeId);
        //When
        result.Should().BeNull();
    }
}