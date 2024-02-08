namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

using System.Text.Json;
using LeaveSystem.Shared.WorkingHours;
using Moq;
using Web.Pages.WorkingHours;
using Web.Shared;

public class GetUserWorkingHoursTest
{
    public static IEnumerable<object[]> WhenDataReceived_ThenReturnIt_MemberData
    {
        get
        {
            yield return new object[]
            {
                new WorkingHoursDto(
                    "0ACAF979-236C-465B-BA45-BF3FDED9C9EC",
                    DateTimeOffset.Parse("2023-05-01"),
                    DateTimeOffset.Parse("2023-12-01"),
                    TimeSpan.FromHours(4),
                    Guid.Parse("96103DF6-3E23-44AB-8B57-5E25FAB06AA2")
                )
            };
            yield return new object[] { null! };
        }
    }

    [Theory]
    [MemberData(nameof(WhenDataReceived_ThenReturnIt_MemberData))]
    public async Task WhenDataReceived_ThenReturnIt(WorkingHoursDto data)
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var userId = "2EE35409-33A9-4A32-9E63-1F218382446D";
        var uri = $"api/workingHours/{userId}";
        universalHttpServiceMock.Setup(
                m => m.GetAsync<WorkingHoursDto>(
                    uri, "Error occured during getting working hours", It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(data);
        var sut = new WorkingHoursService(universalHttpServiceMock.Object);
        var result = await sut.GetUserWorkingHoursAsync(userId);
        result.Should().BeEquivalentTo(data);
        universalHttpServiceMock.Verify(m => m.GetAsync<WorkingHoursDto>(
            uri, "Error occured during getting working hours", It.IsAny<JsonSerializerOptions>()), Times.Once);
    }
}
