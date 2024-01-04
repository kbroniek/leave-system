using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;

namespace LeaveSystem.Web.UnitTests.Pages.LeaveTypes;

public class GetLeaveTypesTest
{
    private HttpClient httpClient = null!;

    private LeaveTypesService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenGettingFromJson_ThenReturnLeaveTypes()
    {
        //Given
        var data = FakeLeaveTypeDtoProvider.GetAll().ToODataResponse();
        var serializableData = new ODataResponse<object>
        {
            Data = data.Data!.Select(d => new
            {
                d.Id,
                d.BaseLeaveTypeId,
                d.Name,
                Properties = new
                {
                    d.Properties.Color,
                    Catalog = d.Properties.Catalog.ToString(),
                    d.Properties.IncludeFreeDays
                }
            })
        };
        var url = "odata/LeaveTypes?$select=Id,BaseLeaveTypeId,Name,Properties&$orderby=Order asc";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, serializableData);
        var sut = GetSut();
        //When
        var result = await sut.GetLeaveTypes();
        //When
        result.Should().BeEquivalentTo(data.Data);
    }

    [Theory]
    [MemberData(nameof(Get_WhenDataIsNull_ThenReturnEmptyCollection_TestData))]
    public async Task WhenDataIsNull_ThenReturnEmptyCollection(ODataResponse<LeaveTypesService.LeaveTypeDto>? serializableData)
    {
        var url = "odata/LeaveTypes?$select=Id,BaseLeaveTypeId,Name,Properties&$orderby=Order asc";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, serializableData);
        var sut = GetSut();
        //When
        var result = await sut.GetLeaveTypes();
        //When
        result.Should().BeEquivalentTo(Enumerable.Empty<object>());
    }

    public static IEnumerable<object?[]> Get_WhenDataIsNull_ThenReturnEmptyCollection_TestData()
    {
        yield return new object[] { new ODataResponse<LeaveTypesService.LeaveTypeDto>() };
        yield return new object?[] { null };
    }
}
