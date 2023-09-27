using System.Net;
using Blazored.Toast.Services;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class EditWorkingHoursTest
{
    private HttpClient httpClientMock;
    private IToastService toastServiceMock;

    private WorkingHoursService GetSut() => new(httpClientMock, toastServiceMock);

    [Fact]
    public async Task WhenErrorOccuredDuringEditing_ShowErrorToastAndReturnFalse()
    {
        //Given
        var workingHoursToEdit = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToDto().ToList();
        const string fakeContentText = "fake response content";
        var responseContent = new StringContent(fakeContentText);
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        const string baseUrl = "http://localhost:5047";
        foreach (var singleWorkingHours in workingHoursToEdit)
        {
            mockHttpMessageHandler
                .When($"{baseUrl}/api/workingHours/{singleWorkingHours.Id}/modify")
                .WithJsonContent(singleWorkingHours)
                .Respond(HttpStatusCode.BadRequest, responseContent); 
        }
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(baseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        var sut = GetSut();
        //When
        var result = await sut.Edit(workingHoursToEdit);
        //Then
        toastServiceMock.Received(1).ShowError(fakeContentText);
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowSuccess(string.Empty);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenEditingPassedSuccessfully_ShowSuccessToastAndReturnTrue()
    {
        //Given
        var workingHoursToEdit = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToDto().ToList();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        const string baseUrl = "http://localhost:5047";
        foreach (var singleWorkingHours in workingHoursToEdit)
        {
            mockHttpMessageHandler
                .When($"{baseUrl}/api/workingHours/{singleWorkingHours.Id}/modify")
                .WithJsonContent(singleWorkingHours)
                .Respond(HttpStatusCode.NoContent); 
        }
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(baseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        var sut = GetSut();
        //When
        var result = await sut.Edit(workingHoursToEdit);
        //Then
        toastServiceMock.Received(1).ShowSuccess(Arg.Any<string>());
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowError(string.Empty);
        result.Should().BeTrue();
    }
}