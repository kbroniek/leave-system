using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class EditWorkingHoursTest
{
    private HttpClient httpClientMock;
    private IToastService toastServiceMock;
    private ILogger<WorkingHoursService> logger;

    private WorkingHoursService GetSut() => new(httpClientMock, toastServiceMock, logger);

    [Fact]
    public async Task WhenErrorOccuredDuringEditing_ShowErrorToastAndReturnFalse()
    {
        //Given
        var workingHoursToEdit = FakeWorkingHoursProvider.GetCurrentForBen(DateTimeOffset.Now).ToDto();
        const string fakeContentText = "fake response content";
        const string fakeDetailText = "fake error in 404 line";  
        var problemDto =
            new ProblemDto(string.Empty, fakeContentText, 400, fakeDetailText, string.Empty, "dev", "1.0.0.0");
        var serializedProblemDto = JsonSerializer.Serialize(problemDto);
        var responseContent = new StringContent(serializedProblemDto, Encoding.UTF8, "application/json");
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        const string baseUrl = "http://localhost:5047";
        mockHttpMessageHandler
            .When($"{baseUrl}/api/workingHours/{workingHoursToEdit.Id}/modify")
            .WithJsonContent(workingHoursToEdit)
            .Respond(HttpStatusCode.BadRequest, responseContent);
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(baseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        logger = Substitute.For<ILogger<WorkingHoursService>>();
        var sut = GetSut();
        //When
        var result = await sut.Edit(workingHoursToEdit);
        //Then
        toastServiceMock.Received(1).ShowError(fakeContentText);
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowSuccess(string.Empty);
        logger.ReceivedWithAnyArgs(1).LogError("");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenEditingPassedSuccessfully_ShowSuccessToastAndReturnTrue()
    {
        //Given
        var workingHoursToEdit = FakeWorkingHoursProvider.GetCurrentForBen(DateTimeOffset.Now).ToDto();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        const string baseUrl = "http://localhost:5047";
        mockHttpMessageHandler
                .When($"{baseUrl}/api/workingHours/{workingHoursToEdit.Id}/modify")
                .WithJsonContent(workingHoursToEdit)
                .Respond(HttpStatusCode.NoContent);
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(baseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        logger = Substitute.For<ILogger<WorkingHoursService>>();
        var sut = GetSut();
        //When
        var result = await sut.Edit(workingHoursToEdit);
        //Then
        toastServiceMock.Received(1).ShowSuccess(Arg.Any<string>());
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowError(string.Empty);
        logger.DidNotReceiveWithAnyArgs().LogError("");
        result.Should().BeTrue();
    }
}