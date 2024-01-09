namespace LeaveSystem.Web.UnitTests.Shared;

using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.UserLeaveLimits;
using Microsoft.Extensions.Logging;
using Moq;
using TestStuff.Extensions;
using TestStuff.Factories;
using TestStuff.Helpers;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class UniversalEditAsyncTest
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TimeSpanIso8601Converter() }
    };

    [Fact]
    public async Task WhenResponseThrowsException_InformAboutError()
    {
        await this.WhenResponseThrowsException_InformAboutError_Helper(
            new LeaveLimitDto(
                Guid.Parse("e6a1dd62-c83c-4397-af49-45b400949ebe"),
                TimeSpan.FromHours(24),
                TimeSpan.Zero,
                Guid.Parse("32fb893f-ddca-45a3-9da1-be0d46cfcb55"),
                DateTimeOffset.Parse("2023-01-01"),
                DateTimeOffset.Parse("2023-12-31"),
                new UserLeaveLimitPropertyDto("desc"),
                "a89488b9-9cae-402e-8f00-587a3dbe563d"));
    }

    private async Task WhenResponseThrowsException_InformAboutError_Helper<TContent>(TContent data)
    {
        var fakeException = new FakeException("fake exception", "fakeStackTrace");
        var httpClientMock = HttpClientMockFactory.CreateWithException("fakeadd", fakeException);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.EditAsync("/fakeadd", data, "success", this.jsonSerializerOptions);
        result.Should().BeFalse();
        toastServiceMock.Verify(m => m.ShowError("Error occured while adding", null), Times.Once);
        loggerMock.VerifyLogError("fake exception\nfakeStackTrace", Times.Once);
    }

    [Fact]
    public async Task WhenResponseNotSuccessful_InformAboutError()
    {
        await this.WhenResponseNotSuccessful_InformAboutError_Helper(
            new LeaveLimitDto(
                Guid.Parse("e6a1dd62-c83c-4397-af49-45b400949ebe"),
                TimeSpan.FromHours(24),
                TimeSpan.Zero,
                Guid.Parse("32fb893f-ddca-45a3-9da1-be0d46cfcb55"),
                DateTimeOffset.Parse("2023-01-01"),
                DateTimeOffset.Parse("2023-12-31"),
                new UserLeaveLimitPropertyDto("desc"),
                "a89488b9-9cae-402e-8f00-587a3dbe563d"));
    }

    private async Task WhenResponseNotSuccessful_InformAboutError_Helper<TContent>(TContent data)
    {
        var problem =
            JsonSerializer.Serialize(new ProblemDto("", "fake error", 400, "fake error occured", "", "", "1.0.0"),
                this.jsonSerializerOptions);
        var fakeResponse = new StringContent(problem, Encoding.UTF8, "application/json");
        var httpClientMock = HttpClientMockFactory.CreateWithJsonContent("fakeadd", data, HttpStatusCode.BadRequest,
            fakeResponse, this.jsonSerializerOptions);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.EditAsync("/fakeadd", data, "success", this.jsonSerializerOptions);
        result.Should().BeFalse();
        toastServiceMock.Verify(m => m.ShowError("fake error", null), Times.Once);
        loggerMock.VerifyLogError("fake error occured", Times.Once);
    }

    [Fact]
    public async Task WhenResponseStatusSuccessful_ReturnTrue()
    {
        await this.WhenResponseStatusSuccessful_ReturnTrue_Helper(
            new LeaveLimitDto(
                Guid.Parse("e6a1dd62-c83c-4397-af49-45b400949ebe"),
                TimeSpan.FromHours(24),
                TimeSpan.Zero,
                Guid.Parse("32fb893f-ddca-45a3-9da1-be0d46cfcb55"),
                DateTimeOffset.Parse("2023-01-01"),
                DateTimeOffset.Parse("2023-12-31"),
                new UserLeaveLimitPropertyDto("desc"),
                "a89488b9-9cae-402e-8f00-587a3dbe563d"));
    }

    private async Task WhenResponseStatusSuccessful_ReturnTrue_Helper<TContent>(TContent data)
    {
        var httpClientMock =
            HttpClientMockFactory.CreateWithJsonContent("fakeadd", data, HttpStatusCode.Created,
                this.jsonSerializerOptions);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.EditAsync("/fakeadd", data, "success", this.jsonSerializerOptions);
        result.Should().BeTrue();
        toastServiceMock.Verify(m => m.ShowSuccess("success", null), Times.Once);
    }
}
