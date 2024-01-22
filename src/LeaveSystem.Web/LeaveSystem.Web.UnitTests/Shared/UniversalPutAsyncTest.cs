namespace LeaveSystem.Web.UnitTests.Shared;

using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.WorkingHours;
using Microsoft.Extensions.Logging;
using Moq;
using TestStuff.Extensions;
using TestStuff.Factories;
using TestStuff.Helpers;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class UniversalPutAsyncTest
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TimeSpanIso8601Converter() }
    };

    [Fact]
    public async Task WhenResponseThrowsException_InformAboutError()
    {
        await this.WhenResponseThrowsException_InformAboutError_Helper(
            new WorkingHoursDto(
                "a89488b9-9cae-402e-8f00-587a3dbe563d",
                DateTimeOffset.Parse("2023-01-01"),
                DateTimeOffset.Parse("2023-12-31"),
                TimeSpan.FromHours(8),
                Guid.Parse("32fb893f-ddca-45a3-9da1-be0d46cfcb55")
            )
        );
    }

    private async Task WhenResponseThrowsException_InformAboutError_Helper<TContent>(TContent data)
    {
        var fakeException = new InvalidOperationException("fake exception");
        var httpClientMock =
            HttpClientMockFactory.CreateWithException("fake-edit", fakeException, out var mockedHttpValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.PutAsync("/fake-edit", data, "success", this.jsonSerializerOptions);
        result.Should().BeFalse();
        toastServiceMock.Verify(m => m.ShowError("Error occured while editing", null), Times.Once);
        loggerMock.VerifyLogError($"Error occured while editing resource of type {typeof(TContent)}", fakeException, Times.Once);
        mockedHttpValues.RequestShouldBeMatched();
    }

    [Fact]
    public async Task WhenResponseNotSuccessful_InformAboutError()
    {
        await this.WhenResponseNotSuccessful_InformAboutError_Helper(
            new WorkingHoursDto(
                "a89488b9-9cae-402e-8f00-587a3dbe563d",
                DateTimeOffset.Parse("2023-01-01"),
                DateTimeOffset.Parse("2023-12-31"),
                TimeSpan.FromHours(8),
                Guid.Parse("32fb893f-ddca-45a3-9da1-be0d46cfcb55")
            )
        );
    }

    private async Task WhenResponseNotSuccessful_InformAboutError_Helper<TContent>(TContent data)
    {
        var problem =
            JsonSerializer.Serialize(new ProblemDto("", "fake error", 400, "fake error occured", "", "", "1.0.0"),
                this.jsonSerializerOptions);
        var fakeResponse = new StringContent(problem, Encoding.UTF8, "application/json");
        var httpClientMock = HttpClientMockFactory.CreateWithJsonContent(
            "fake-edit", data, HttpStatusCode.BadRequest, fakeResponse, this.jsonSerializerOptions,
            out var mockedHttpValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.PutAsync("/fake-edit", data, "success", this.jsonSerializerOptions);
        result.Should().BeFalse();
        toastServiceMock.Verify(m => m.ShowError("fake error", null), Times.Once);
        loggerMock.VerifyLogError("fake error occured", Times.Once);
        mockedHttpValues.RequestShouldBeMatched();
    }

    [Fact]
    public async Task WhenResponseStatusSuccessful_ReturnTrue()
    {
        await this.WhenResponseStatusSuccessful_ReturnTrue_Helper(
            new WorkingHoursDto(
                "a89488b9-9cae-402e-8f00-587a3dbe563d",
                DateTimeOffset.Parse("2023-01-01"),
                DateTimeOffset.Parse("2023-12-31"),
                TimeSpan.FromHours(8),
                Guid.Parse("32fb893f-ddca-45a3-9da1-be0d46cfcb55")
            )
        );
    }

    private async Task WhenResponseStatusSuccessful_ReturnTrue_Helper<TContent>(TContent data)
    {
        var httpClientMock =
            HttpClientMockFactory.CreateWithJsonContent(
                "fake-edit", data, HttpStatusCode.Created, this.jsonSerializerOptions, out var mockedHttpValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.PutAsync("/fake-edit", data, "success", this.jsonSerializerOptions);
        result.Should().BeTrue();
        toastServiceMock.Verify(m => m.ShowSuccess("success", null), Times.Once);
        mockedHttpValues.RequestShouldBeMatched();
    }
}
