namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.UserLeaveLimits;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class AddLimitTest
{
    [Fact]
    public async Task WhenAddingResultedNull_ThenReturnNull()
    {
        var universalHttpServiceMock = GetUniversalHttpServiceMock();
        var fakeEntityToAdd = new AddUserLeaveLimitDto
        {
            AssignedToUserId = "fakeuserid",
            LeaveTypeId = Guid.Parse("54d432e0-68f2-4819-82e1-33107f8dd1de"),
            ValidSince = DateTimeOffset.Parse("2023-04-05"),
            ValidUntil = DateTimeOffset.Parse("2023-04-21"),
            Limit = TimeSpan.FromHours(8),
            OverdueLimit = TimeSpan.FromHours(2),
            Property = new AddUserLeaveLimitPropertiesDto { Description = "fake description" }
        };
        universalHttpServiceMock.Setup(m =>
                m.AddAsync<AddUserLeaveLimitDto, UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
                    "odata/UserLeaveLimits",
                    It.Is<AddUserLeaveLimitDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                    It.IsAny<string>(),
                    It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync((UserLeaveLimitsService.UserLeaveLimitDtoODataResponse?)null);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);

        var result = await sut.AddAsync(fakeEntityToAdd);
        result.Should().BeNull();
        universalHttpServiceMock.Verify(
            m => m.AddAsync<AddUserLeaveLimitDto, UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
                "odata/UserLeaveLimits",
                It.Is<AddUserLeaveLimitDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                "User leave limit added successfully",
                It.Is<JsonSerializerOptions>(
                    o => o.Converters.Any(c => c.GetType() == typeof(TimeSpanIso8601Converter)))));
    }
    [Fact]
    public async Task WhenEntityAdded_ThenReturnResultDto()
    {
        var universalHttpServiceMock = GetUniversalHttpServiceMock();
        var fakeEntityToAdd = new AddUserLeaveLimitDto
        {
            AssignedToUserId = "fakeuserid",
            LeaveTypeId = Guid.Parse("54d432e0-68f2-4819-82e1-33107f8dd1de"),
            ValidSince = DateTimeOffset.Parse("2023-04-05"),
            ValidUntil = DateTimeOffset.Parse("2023-04-21"),
            Limit = TimeSpan.FromHours(8),
            OverdueLimit = TimeSpan.FromHours(2),
            Property = new AddUserLeaveLimitPropertiesDto { Description = "fake description" }
        };
        var expectedResult = new UserLeaveLimitsService.UserLeaveLimitDtoODataResponse
        {
            Id = Guid.Parse("14497429-e3c7-428c-933f-5b69c28a73eb"),
            LeaveTypeId = Guid.Parse("54d432e0-68f2-4819-82e1-33107f8dd1de"),
            ValidSince = DateTimeOffset.Parse("2023-04-05"),
            ValidUntil = DateTimeOffset.Parse("2023-04-21"),
            Limit = TimeSpan.FromHours(8),
            OverdueLimit = TimeSpan.FromHours(2),
            Property = new UserLeaveLimitPropertyDto { Description = "fake description" }
        };
        universalHttpServiceMock.Setup(m =>
                m.AddAsync<AddUserLeaveLimitDto, UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
                    "odata/UserLeaveLimits",
                    It.Is<AddUserLeaveLimitDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                    It.IsAny<string>(),
                    It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(expectedResult);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);

        var result = await sut.AddAsync(fakeEntityToAdd);
        result.Should().BeEquivalentTo(
            new
            {
                Id = Guid.Parse("14497429-e3c7-428c-933f-5b69c28a73eb"),
                LeaveTypeId = Guid.Parse("54d432e0-68f2-4819-82e1-33107f8dd1de"),
                ValidSince = DateTimeOffset.Parse("2023-04-05"),
                ValidUntil = DateTimeOffset.Parse("2023-04-21"),
                Limit = TimeSpan.FromHours(8),
                OverdueLimit = TimeSpan.FromHours(2),
                Property = new { Description = "fake description" }
            });
        universalHttpServiceMock.Verify(
            m => m.AddAsync<AddUserLeaveLimitDto, UserLeaveLimitsService.UserLeaveLimitDtoODataResponse>(
                "odata/UserLeaveLimits",
                It.Is<AddUserLeaveLimitDto>(d => IsDtoEquivalentTo(d, fakeEntityToAdd)),
                "User leave limit added successfully",
                It.Is<JsonSerializerOptions>(
                    o => o.Converters.Any(c => c.GetType() == typeof(TimeSpanIso8601Converter)))));
    }

    private static bool IsDtoEquivalentTo(AddUserLeaveLimitDto firstDto, AddUserLeaveLimitDto secondDto) =>
        firstDto.OverdueLimit == secondDto.OverdueLimit &&
        firstDto.Limit == secondDto.Limit &&
        firstDto.ValidUntil == secondDto.ValidUntil &&
        firstDto.ValidSince == secondDto.ValidSince &&
        firstDto.LeaveTypeId == secondDto.LeaveTypeId &&
        firstDto.AssignedToUserId == secondDto.AssignedToUserId;

    private static Mock<UniversalHttpService> GetUniversalHttpServiceMock() =>
        new(new Mock<HttpClient>().Object, new Mock<IToastService>().Object,
            new Mock<ILogger<UniversalHttpService>>().Object);
}
