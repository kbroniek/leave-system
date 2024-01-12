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
using Moq.Protected;
using TestStuff.Extensions;
using TestStuff.Factories;
using Web.Pages.UserLeaveLimits;
using Web.Shared;

public class UniversalAddAsyncTest
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TimeSpanIso8601Converter() }
    };

    [Fact]
    public async Task WhenResponseThrowsException_InformAboutError()
    {
        await this.WhenResponseThrowsException_InformAboutError_Helper<AddUserLeaveLimitDto, LeaveLimitDto>(
            new AddUserLeaveLimitDto());
    }

    private async Task WhenResponseThrowsException_InformAboutError_Helper<TContent, TResponse>(TContent data)
    {
        var fakeException = new InvalidOperationException("fake exception");
        var httpClientMock = HttpClientMockFactory
            .CreateWithException("fake-add", fakeException, out var mockedHttpValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.AddAsync<TContent, TResponse>("/fake-add", data, "success", this.jsonSerializerOptions);
        result.Should().Be(default);
        toastServiceMock.Verify(m => m.ShowError("Error occured while adding", null), Times.Once);
        loggerMock.VerifyLogError($"Error occured while adding resource of type {typeof(TContent)}", fakeException,
            Times.Once);
        mockedHttpValues.RequestShouldBeMatched();
    }

    [Fact]
    public async Task WhenResponseNotSuccessful_InformAboutError()
    {
        await this.WhenResponseNotSuccessful_InformAboutError_Helper<AddUserLeaveLimitDto, LeaveLimitDto>(
            new AddUserLeaveLimitDto());
    }

    private async Task WhenResponseNotSuccessful_InformAboutError_Helper<TContent, TResponse>(TContent data)
    {
        var problem =
            JsonSerializer.Serialize(new ProblemDto("", "fake error", 400, "fake error occured", "", "", "1.0.0"),
                this.jsonSerializerOptions);
        var fakeResponse = new StringContent(problem, Encoding.UTF8, "application/json");
        var httpClientMock = HttpClientMockFactory.CreateWithJsonContent("fake-add", data, HttpStatusCode.BadRequest,
            fakeResponse, this.jsonSerializerOptions, out var mockedHttpValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.AddAsync<TContent, TResponse>("/fake-add", data, "success", this.jsonSerializerOptions);
        result.Should().Be(default);
        toastServiceMock.Verify(m => m.ShowError("fake error", null), Times.Once);
        loggerMock.VerifyLogError("fake error occured", Times.Once);
        mockedHttpValues.RequestShouldBeMatched();
    }

    [Fact]
    public async Task WhenResponseStatusSuccessful_ReturnCreatedDto()
    {
        await this.WhenResponseStatusSuccessful_ReturnCreatedDto_Helper(new AddUserLeaveLimitDto(),
            new UserLeaveLimitsService.UserLeaveLimitDtoODataResponse()
            {
                ContextUrl = "fakeCtx",
                Id = Guid.Parse("760B54B0-E381-4679-8474-9C0D84FDAED2"),
                Limit = TimeSpan.FromHours(8),
                OverdueLimit = TimeSpan.Zero,
                LeaveTypeId = Guid.Parse("65ECE3F3-C7EF-4330-A6EE-D339C4BB6085"),
                ValidSince = DateTimeOffset.Parse("2023-01-01"),
                ValidUntil = DateTimeOffset.Parse("2023-12-31"),
                Property = new UserLeaveLimitPropertyDto("desc")
            });
    }

    private async Task WhenResponseStatusSuccessful_ReturnCreatedDto_Helper<TContent, TResponse>(TContent data,
        TResponse response)
    {
        var fakeResponse = new StringContent(JsonSerializer.Serialize(response, this.jsonSerializerOptions),
            Encoding.UTF8, "application/json");
        var httpClientMock = HttpClientMockFactory.CreateWithJsonContent(
            "fake-add", data, HttpStatusCode.Created, fakeResponse, this.jsonSerializerOptions,
            out var mockedHttpValues);
        var toastServiceMock = new Mock<IToastService>();
        var loggerMock = new Mock<ILogger<UniversalHttpService>>();
        var sut = new UniversalHttpService(httpClientMock, toastServiceMock.Object, loggerMock.Object);
        var result = await sut.AddAsync<TContent, TResponse>("/fake-add", data, "success", this.jsonSerializerOptions);
        result.Should().BeEquivalentTo(response);
        toastServiceMock.Verify(m => m.ShowSuccess("success", null), Times.Once);
        mockedHttpValues.RequestShouldBeMatched();
    }
}
