namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

using System.Text.Json;
using LeaveSystem.Shared.Converters;
using Moq;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class EditLimitTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenEntityUpdated_ThenReturnUpdatingResult(bool updateResult)
    {
        var universalHttpServiceMock = new Mock<UniversalHttpService>(null!, null!, null!);
        var fakeEntityToEdit = new UserLeaveLimitDto()
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
                m.PatchAsync(
                    $"odata/UserLeaveLimits({fakeEntityToEdit.Id})",
                    It.Is<UserLeaveLimitDto>(d => IsDtoEquivalentTo(d, fakeEntityToEdit)),
                    It.IsAny<string>(),
                    It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(updateResult);
        var sut = new UserLeaveLimitsService(universalHttpServiceMock.Object);

        var result = await sut.EditAsync(fakeEntityToEdit);
        result.Should().Be(updateResult);
        universalHttpServiceMock.Verify(
            m => m.PatchAsync(
                $"odata/UserLeaveLimits({fakeEntityToEdit.Id})",
                It.Is<UserLeaveLimitDto>(d => IsDtoEquivalentTo(d, fakeEntityToEdit)),
                "User leave limit edited successfully",
                It.Is<JsonSerializerOptions>(
                    o => o.Converters.Any(c => c.GetType() == typeof(TimeSpanIso8601Converter)))));
    }

    private static bool IsDtoEquivalentTo(UserLeaveLimitDto firstDto, UserLeaveLimitDto secondDto) =>
        firstDto.OverdueLimit == secondDto.OverdueLimit &&
        firstDto.Limit == secondDto.Limit &&
        firstDto.ValidUntil == secondDto.ValidUntil &&
        firstDto.ValidSince == secondDto.ValidSince &&
        firstDto.LeaveTypeId == secondDto.LeaveTypeId &&
        firstDto.Id == secondDto.Id;
}
