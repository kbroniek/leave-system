using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.UnitTests.TestStuff.Converters;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

public class EditLimitTest
{
    private HttpClient httpClientMock;
    private Mock<IToastService> toastServiceMock = new();
    private Mock<ILogger<UserLeaveLimitsService>> loggerMock = new();

    private UserLeaveLimitsService GetSut() => new(httpClientMock, toastServiceMock.Object, loggerMock.Object);
    
    [Fact]
    public async Task WhenNotSuccessfulStatusCodeAfterAdding_ShowErrorToastAndLogErrorAndReturnNull()
    {
        //Given
        var limitToAdd = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave().ToDto();
        const string fakeContentText = "fake response content";
        const string fakeDetailsText = "fake error in 404 line";
        var problemDto =
            new ProblemDto(string.Empty, fakeContentText, 400, fakeDetailsText, string.Empty, "dev", "1.0.0.0");
        var serializedProblemDto = JsonSerializer.Serialize(problemDto, FakeJsonSerializerOptionsProvider.GetWithTimespanConverter());
        var responseContent = new StringContent(serializedProblemDto, Encoding.UTF8, "application/json");
        httpClientMock = HttpClientMockFactory.CreateWithJsonContent($"odata/UserLeaveLimits({limitToAdd.Id})", limitToAdd, HttpStatusCode.BadRequest, responseContent, FakeJsonSerializerOptionsProvider.GetWithTimespanConverter());
        toastServiceMock = new Mock<IToastService>();
        loggerMock = new Mock<ILogger<UserLeaveLimitsService>>();
        var sut = GetSut();
        //When
        var result = await sut.EditAsync(limitToAdd);
        //Then
        toastServiceMock.Verify(m => m.ShowError(fakeContentText, null), Times.Once);
        toastServiceMock.Verify(x => x.ShowSuccess(It.IsAny<string>(), null), Times.Never);
        loggerMock.VerifyLogError(fakeDetailsText, Times.Once);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenStatusCodeIsSuccessfulAndResponseContentIsNotNull_ThenReturnAddedEntity()
    {
        //Given
        var limitToAdd = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave().ToDto();
        var serializedAddedLimit = JsonSerializer.Serialize(limitToAdd, FakeJsonSerializerOptionsProvider.GetWithTimespanConverter());
        var responseContent = new StringContent(serializedAddedLimit, Encoding.UTF8, "application/json");
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new TimeSpanToStringConverter()
            },
        };
        httpClientMock = HttpClientMockFactory.CreateWithJsonContent($"odata/UserLeaveLimits({limitToAdd.Id})", limitToAdd, HttpStatusCode.OK, responseContent, options);
        toastServiceMock = new Mock<IToastService>();
        loggerMock = new Mock<ILogger<UserLeaveLimitsService>>();
        var sut = GetSut();
        //When
        var result = await sut.EditAsync(limitToAdd);
        //Then
        toastServiceMock.Verify(m => m.ShowError(It.IsAny<string>(), null), Times.Never);
        toastServiceMock.Verify(x => x.ShowSuccess(It.IsAny<string>(), null), Times.Once);
        result.Should().BeTrue();
    }
}