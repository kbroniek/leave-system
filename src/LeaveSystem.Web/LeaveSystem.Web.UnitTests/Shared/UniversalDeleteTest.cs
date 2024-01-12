namespace LeaveSystem.Web.UnitTests.Shared;

using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Converters;
using Microsoft.Extensions.Logging;
using Moq;
using TestStuff.Extensions;
using TestStuff.Factories;
using TestStuff.Helpers;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class UniversalDeleteTest
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TimeSpanIso8601Converter() }
    };

    [Fact]
    public async Task WhenResponseThrowsException_InformAboutError()
    {
        var fakeException = new InvalidOperationException("fake exception");
        var httpClientMock = HttpClientMockFactory.CreateWithException("fake-delete", fakeException, out var mockedHttpValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.DeleteAsync("/fake-delete", "success", this.jsonSerializerOptions);
        result.Should().BeFalse();
        toastServiceMock.Verify(m => m.ShowError("Error occured while deleting", null), Times.Once);
        loggerMock.VerifyLogError("Error occured while deleting resource", fakeException, Times.Once);
        mockedHttpValues.RequestShouldBeMatched();
    }

    [Fact]
    public async Task WhenResponseNotSuccessful_InformAboutError()
    {
        var problem = new ProblemDto("", "fake error", 400, "fake error occured", "", "", "1.0.0");
        var httpClientMock = HttpClientMockFactory.CreateWithJsonResponse(
            "fake-delete", problem, HttpStatusCode.BadRequest,this.jsonSerializerOptions, out var httpMockedValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.DeleteAsync("/fake-delete","success", this.jsonSerializerOptions);
        result.Should().BeFalse();
        toastServiceMock.Verify(m => m.ShowError("fake error", null), Times.Once);
        loggerMock.VerifyLogError("fake error occured", Times.Once);
        httpMockedValues.RequestShouldBeMatched();
    }

    [Fact]
    public async Task WhenResponseStatusSuccessful_ReturnTrue()
    {
        var httpClientMock = HttpClientMockFactory.CreateWithJsonResponse("fake-delete", HttpStatusCode.OK, this.jsonSerializerOptions, out var httpMockedValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.DeleteAsync("/fake-delete", "success", this.jsonSerializerOptions);
        result.Should().BeTrue();
        toastServiceMock.Verify(m => m.ShowSuccess("success", null), Times.Once);
        httpMockedValues.RequestShouldBeMatched();
    }
}
