using System.Text.Json;
using Blazored.Toast.Services;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

using LeaveSystem.Shared.WorkingHours;
using Moq;
using Web.Pages.WorkingHours;
using Web.Shared;

public class AddWorkingHoursTest
{
    [Fact]
    public async Task WhenAddingResultedNull_ThenReturnNull()
    {
        var universalHttpServiceMock = GetUniversalHttpServiceMock();
        var fakeEntityToAdd = new AddWorkingHoursDto(
            "0ACAF979-236C-465B-BA45-BF3FDED9C9EC",
            DateTimeOffset.Parse("2023-05-01"),
            DateTimeOffset.Parse("2023-12-01"),
            TimeSpan.FromHours(4)
        );
        universalHttpServiceMock.Setup(m =>
                m.PostAsync<AddWorkingHoursDto, WorkingHoursDto>(
                    "api/workingHours",
                    It.Is<AddWorkingHoursDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                    It.IsAny<string>(),
                    It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync((WorkingHoursDto?)null);
        var sut = new WorkingHoursService(universalHttpServiceMock.Object);

        var result = await sut.AddAsync(fakeEntityToAdd);
        result.Should().BeNull();
        universalHttpServiceMock.Verify(
            m => m.PostAsync<AddWorkingHoursDto, WorkingHoursDto>(
                "api/workingHours",
                It.Is<AddWorkingHoursDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                "Added working hours successfully",
                It.IsAny<JsonSerializerOptions>()));
    }

    [Fact]
    public async Task WhenEntityAdded_ThenReturnResultDto()
    {
        var universalHttpServiceMock = GetUniversalHttpServiceMock();
        var fakeEntityToAdd = new AddWorkingHoursDto(
            "0ACAF979-236C-465B-BA45-BF3FDED9C9EC",
            DateTimeOffset.Parse("2023-05-01"),
            DateTimeOffset.Parse("2023-12-01"),
            TimeSpan.FromHours(4)
        );
        var expectedResult = new WorkingHoursDto(
            "0ACAF979-236C-465B-BA45-BF3FDED9C9EC",
            DateTimeOffset.Parse("2023-05-01"),
            DateTimeOffset.Parse("2023-12-01"),
            TimeSpan.FromHours(4),
            Guid.Parse("96103DF6-3E23-44AB-8B57-5E25FAB06AA2")
        );
        universalHttpServiceMock.Setup(m =>
                m.PostAsync<AddWorkingHoursDto, WorkingHoursDto>(
                    "api/workingHours",
                    It.Is<AddWorkingHoursDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                    It.IsAny<string>(),
                    It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(expectedResult);
        var sut = new WorkingHoursService(universalHttpServiceMock.Object);

        var result = await sut.AddAsync(fakeEntityToAdd);
        result.Should().BeEquivalentTo(
            new
            {
                UserId = "0ACAF979-236C-465B-BA45-BF3FDED9C9EC",
                DateFrom = DateTimeOffset.Parse("2023-05-01"),
                DateTo = DateTimeOffset.Parse("2023-12-01"),
                Duration = TimeSpan.FromHours(4),
                Id = Guid.Parse("96103DF6-3E23-44AB-8B57-5E25FAB06AA2")
            });
        universalHttpServiceMock.Verify(
            m => m.PostAsync<AddWorkingHoursDto, WorkingHoursDto>(
                "api/workingHours",
                It.Is<AddWorkingHoursDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                "Added working hours successfully",
                It.IsAny<JsonSerializerOptions>()));
    }

    private static bool IsDtoEquivalentTo(AddWorkingHoursDto firstDto, AddWorkingHoursDto secondDto) =>
        firstDto.DateFrom == secondDto.DateFrom &&
        firstDto.DateTo == secondDto.DateTo &&
        firstDto.Duration == secondDto.Duration &&
        firstDto.UserId == secondDto.UserId;

    private static Mock<UniversalHttpService> GetUniversalHttpServiceMock() =>
        new(new Mock<HttpClient>().Object, new Mock<IToastService>().Object,
            new Mock<ILogger<UniversalHttpService>>().Object);
}
