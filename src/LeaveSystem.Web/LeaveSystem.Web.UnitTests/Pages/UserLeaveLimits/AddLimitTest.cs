using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

public class AddLimitTest
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
        var serializedProblemDto = JsonSerializer.Serialize(problemDto);
        var responseContent = new StringContent(serializedProblemDto, Encoding.UTF8, "application/json");
        httpClientMock = HttpClientMockFactory.CreateWithJsonContent("odata/UserLeaveLimits", limitToAdd, HttpStatusCode.BadRequest, responseContent);
        toastServiceMock = new Mock<IToastService>();
        loggerMock = new Mock<ILogger<UserLeaveLimitsService>>();
        var sut = GetSut();
        //When
        var result = await sut.AddAsync(limitToAdd);
        //Then
        toastServiceMock.Verify(m => m.ShowError(fakeContentText, null), Times.Once);
        toastServiceMock.Verify(x => x.ShowSuccess(It.IsAny<string>(), null), Times.Never);
        loggerMock.VerifyLogError(fakeDetailsText, Times.Once);
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task WhenDeserializedResponseContentIsNull_ThenShowErrorToastAndReturnNull()
    {
        //Given
        var limitToAdd = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave().ToDto();
        const string fakeContentText = "fake response content";
        const string fakeDetailsText = "fake error in 404 line";
        var problemDto = (ProblemDto) null;
        var serializedProblemDto = JsonSerializer.Serialize(problemDto);
        var responseContent = new StringContent(serializedProblemDto, Encoding.UTF8, "application/json");
        httpClientMock = HttpClientMockFactory.CreateWithJsonContent("odata/UserLeaveLimits", limitToAdd, HttpStatusCode.OK, responseContent);
        toastServiceMock = new Mock<IToastService>();
        loggerMock = new Mock<ILogger<UserLeaveLimitsService>>();
        var sut = GetSut();
        //When
        var result = await sut.AddAsync(limitToAdd);
        //Then
        toastServiceMock.Verify(m => m.ShowError("Unexpected empty result", null), Times.Once);
        toastServiceMock.Verify(x => x.ShowSuccess(It.IsAny<string>(), null), Times.Never);
        result.Should().BeNull();
    }

    [Fact]
    public async Task WhenStatusCodeIsSuccessfulAndResponseContentIsNotNull_ThenReturnAddedEntity()
    {
        //Given
        var limitToAdd = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave().ToDto();
        var serializedAddedLimit = JsonSerializer.Serialize(limitToAdd);
        var responseContent = new StringContent(serializedAddedLimit, Encoding.UTF8, "application/json");
        httpClientMock = HttpClientMockFactory.CreateWithJsonContent("odata/UserLeaveLimits", limitToAdd, HttpStatusCode.OK, responseContent);
        toastServiceMock = new Mock<IToastService>();
        loggerMock = new Mock<ILogger<UserLeaveLimitsService>>();
        var sut = GetSut();
        //When
        var result = await sut.AddAsync(limitToAdd);
        //Then
        toastServiceMock.Verify(m => m.ShowError(It.IsAny<string>(), null), Times.Never);
        toastServiceMock.Verify(x => x.ShowSuccess(It.IsAny<string>(), null), Times.Once);
        result.Should().BeEquivalentTo(limitToAdd);
    }
}