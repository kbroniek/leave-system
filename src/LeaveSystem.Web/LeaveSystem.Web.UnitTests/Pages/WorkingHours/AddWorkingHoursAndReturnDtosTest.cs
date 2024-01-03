using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class AddWorkingHoursAndReturnDtosTest
{
    private HttpClient httpClientMock;
    private IToastService toastServiceMock;
    private ILogger<WorkingHoursService> logger;
    private const string BaseUrl = "http://localhost:5047";
    private const string RequestUrl = "http://localhost:5047/api/workingHours";

    private WorkingHoursService GetSut() => new(httpClientMock, toastServiceMock, logger);

    [Fact]
    public async Task WhenErrorOccuredDuringAdding_ShowErrorToastAndReturnNull()
    {
        //Given
        var workingHoursToAdd = FakeWorkingHoursProvider.GetCurrentForBen(DateTimeOffset.Now).ToAddWorkingHoursDto();
        const string fakeContentText = "fake response content";
        const string fakeDetailsText = "fake error in 404 line";
        var problemDto =
            new ProblemDto(string.Empty, fakeContentText, 400, fakeDetailsText, string.Empty, "dev", "1.0.0.0");
        var serializedProblemDto = JsonSerializer.Serialize(problemDto);
        var responseContent = new StringContent(serializedProblemDto, Encoding.UTF8, "application/json");
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler
            .When(RequestUrl)
            .WithJsonContent(workingHoursToAdd)
            .Respond(HttpStatusCode.BadRequest, responseContent);
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(BaseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        logger = Substitute.For<ILogger<WorkingHoursService>>();
        var sut = GetSut();
        //When
        var result = await sut.Add(workingHoursToAdd);
        //Then
        toastServiceMock.Received(1).ShowError(fakeContentText);
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowSuccess(string.Empty);
        logger.ReceivedWithAnyArgs(1).LogError("");
        result.Should().BeNull();
    }

    [Fact]
    public async Task WhenAddingPassedSuccessfully_ShowSuccessToastAndReturnDtos()
    {
        //Given
        var workingHoursToAdd = FakeWorkingHoursProvider.GetCurrentForBen(DateTimeOffset.Now).ToAddWorkingHoursDto();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var generatedId = Guid.NewGuid();
        var expectedResult = workingHoursToAdd.ToWorkingHoursDto(generatedId);
        var responseContent = new StringContent(JsonSerializer.Serialize(expectedResult), Encoding.UTF8, "application/json");
        mockHttpMessageHandler
            .When(RequestUrl)
            .WithJsonContent(workingHoursToAdd)
            .Respond(HttpStatusCode.Created, responseContent);
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(BaseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        logger = Substitute.For<ILogger<WorkingHoursService>>();
        var sut = GetSut();
        //When
        var result = await sut.Add(workingHoursToAdd);
        //Then
        toastServiceMock.Received(1).ShowSuccess(Arg.Any<string>());
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowError(string.Empty);
        logger.DidNotReceiveWithAnyArgs().LogError("");
        result.Should().BeEquivalentTo(expectedResult);
    }
}