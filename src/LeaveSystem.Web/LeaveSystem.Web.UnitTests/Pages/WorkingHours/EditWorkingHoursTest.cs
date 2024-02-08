using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.WorkingHours;
using Moq;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class EditWorkingHoursTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenEntityUpdated_ThenReturnUpdatingResult(bool updateResult)
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var fakeEntityToEdit = new WorkingHoursDto(
            "0ACAF979-236C-465B-BA45-BF3FDED9C9EC",
            DateTimeOffset.Parse("2023-05-01"),
            DateTimeOffset.Parse("2023-12-01"),
            TimeSpan.FromHours(4),
            Guid.Parse("96103DF6-3E23-44AB-8B57-5E25FAB06AA2")
        );
        universalHttpServiceMock.Setup(m =>
                m.PutAsync(
                    $"api/workingHours/{fakeEntityToEdit.Id}/modify",
                    It.Is<WorkingHoursDto>(d => IsDtoEquivalentTo(d, fakeEntityToEdit)),
                    It.IsAny<string>(),
                    It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(updateResult);
        var sut = new WorkingHoursService(universalHttpServiceMock.Object);

        var result = await sut.EditAsync(fakeEntityToEdit);
        result.Should().Be(updateResult);
        universalHttpServiceMock.Verify(
            m => m.PutAsync(
                $"api/workingHours/{fakeEntityToEdit.Id}/modify",
                It.Is<WorkingHoursDto>(d => IsDtoEquivalentTo(d, fakeEntityToEdit)),
                "Edited working hours successfully",
                It.IsAny<JsonSerializerOptions>()));
    }

    private static bool IsDtoEquivalentTo(WorkingHoursDto firstDto, WorkingHoursDto secondDto) =>
        firstDto.DateFrom == secondDto.DateFrom &&
        firstDto.DateTo == secondDto.DateTo &&
        firstDto.Duration == secondDto.Duration &&
        firstDto.UserId == secondDto.UserId &&
        firstDto.Id == secondDto.Id;
}
