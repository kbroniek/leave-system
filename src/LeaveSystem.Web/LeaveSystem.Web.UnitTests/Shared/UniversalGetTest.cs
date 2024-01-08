using System.Net;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Shared;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeaveSystem.Web.UnitTests.Shared;

public class UniversalGetTest
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TimeSpanIso8601Converter() }
    };
    [Fact]
    public async Task WhenNoException_ThenReturnData()
    {
        var fakeLimitResponse = new ODataResponse<IEnumerable<LeaveLimitDto>>()
        {
            Data = FakeUserLeaveLimitsDtoProvider.GetAllLimits()
        };
        await this.WhenNoException_ThenReturnData_Helper(fakeLimitResponse);
    }

    private async Task WhenNoException_ThenReturnData_Helper<T>(T response)
    {
        var httpClientMock = HttpClientMockFactory.CreateWithJsonResponse(
            "fakeUrl",
            response, this.jsonSerializerOptions
        );
        var sut = new UniversalHttpService(httpClientMock, new Mock<IToastService>().Object,
            new Mock<ILogger<UniversalHttpService>>().Object);
        var result = await sut.GetAsync<T>("fakeUrl", "fake error occured", this.jsonSerializerOptions);
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task WhenHttpRequestExceptionOccured_ThenInformAboutIt() => await this.WhenHttpRequestExceptionOccured_ThenInformAboutIt_Helper<ODataResponse<IEnumerable<LeaveLimitDto>>>();

    private async Task WhenHttpRequestExceptionOccured_ThenInformAboutIt_Helper<T>()
    {
        const string fakeUrl = "fakeUrl";
        const string fakeErrorMessage = "fake error occured";
        var httpClientMock = HttpClientMockFactory.Create(
            fakeUrl,
            HttpStatusCode.InternalServerError
        );
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.GetAsync<T>(fakeUrl, fakeErrorMessage, this.jsonSerializerOptions);
        result.Should().Be(default);
        toastServiceMock.Verify(m => m.ShowError(fakeErrorMessage, null), Times.Once);
        loggerMock.VerifyLogError(null, Times.Once);
    }
}
