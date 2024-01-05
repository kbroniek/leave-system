using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class EditWorkingHoursTest : IDisposable
{
    private HttpClient httpClientMock = null!;
    private readonly IToastService toastServiceMock = Substitute.For<IToastService>();
    private readonly ILogger<WorkingHoursService> logger = Substitute.For<ILogger<WorkingHoursService>>();

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
        httpClientMock = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(baseUrl)
        };
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
        httpClientMock = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(baseUrl)
        };
        var sut = GetSut();
        //When
        var result = await sut.Edit(workingHoursToEdit);
        //Then
        toastServiceMock.Received(1).ShowSuccess(Arg.Any<string>());
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowError(string.Empty);
        logger.DidNotReceiveWithAnyArgs().LogError("");
        result.Should().BeTrue();
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            httpClientMock?.Dispose();
        }
    }
}
