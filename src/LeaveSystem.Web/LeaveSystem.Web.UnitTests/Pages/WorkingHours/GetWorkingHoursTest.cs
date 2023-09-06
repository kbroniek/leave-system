using System.Globalization;
using System.Text.Json;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using Microsoft.Extensions.Primitives;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class GetWorkingHoursTest
{
    //Todo: Rework test when WorkingHours rework will be done
    private HttpClient httpClient;

    private WorkingHoursService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenGetWorkingHours_ThenReturnExceptedDeserializedResult()
    {
        //Given
        var dto = new WorkingHoursCollectionDto(FakeWorkingHoursModelProvider.GetAll());
        var userIds = new[] { "1", "2", "3", "4", "5" };
        var dateFrom = DateTimeOffsetExtensions.CreateFromDate(2020, 1, 2);
        var dateTo = DateTimeOffsetExtensions.CreateFromDate(2027, 1, 2);
        // var url = $"api/workingHours?{string.Join("", userIds.Select(x => $"UserIds={x}&").ToArray())}DateFrom={dateFrom.ToString("o", CultureInfo.InvariantCulture)}&dateTo={dateTo.ToString("o", CultureInfo.InvariantCulture)}";
        var url =
            "api/workingHours?UserIds=1&UserIds=2&UserIds=3&UserIds=4&UserIds=5&DateFrom=2020-01-02T00%3A00%3A00.0000000%2B00%3A00&dateTo=2027-01-02T00%3A00%3A00.0000000%2B00%3A00";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, dto, new JsonSerializerOptions(), out var mockedHttpValues);
        var sut = GetSut();
        //When
        var result = await sut.GetWorkingHours(userIds,dateFrom, dateTo);
        //Then
        var expectedResult = new WorkingHoursCollection(dto.WorkingHours);
        // result.Should().BeEquivalentTo(expectedResult));
        mockedHttpValues.ShouldMatchCount();
    }
}