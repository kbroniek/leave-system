using System.Net;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using RichardSzalay.MockHttp;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class AddWorkingHoursAndReturnDtosTest
{
    private HttpClient httpClientMock;
    private IToastService toastServiceMock;
    private const string BaseUrl = "http://localhost:5047";
    private const string RequestUrl = "http://localhost:5047/api/workingHours";

    private WorkingHoursService GetSut() => new(httpClientMock, toastServiceMock);
    
    [Fact]
    public async Task WhenErrorOccuredDuringAdding_ShowErrorToastAndReturnNull()
    {
        //Given
        var workingHoursToAdd = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToAddDto().ToList();
        const string fakeContentText = "fake response content";
        var problemDto =
            new ProblemDto(string.Empty, fakeContentText, 400, string.Empty, string.Empty, "dev", "1.0.0.0");
        var serializedProblemDto = JsonSerializer.Serialize(problemDto);
        var responseContent = new StringContent(serializedProblemDto, Encoding.UTF8, "application/json");
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        foreach (var singleWorkingHours in workingHoursToAdd)
        {
            mockHttpMessageHandler
                .When(RequestUrl)
                .WithJsonContent(singleWorkingHours)
                .Respond(HttpStatusCode.BadRequest, responseContent); 
        }
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(BaseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        var sut = GetSut();
        //When
        var result = await sut.AddAndReturnDtos(workingHoursToAdd);
        //Then
        toastServiceMock.Received(1).ShowError(fakeContentText);
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowSuccess(string.Empty);
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task WhenAddingPassedSuccessfully_ShowSuccessToastAndReturnDtos()
    {
        //Given
        var workingHoursToAdd = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToAddDto().ToList();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var expectedResult = new List<WorkingHoursDto>();
        foreach (var addWorkingHoursDto in workingHoursToAdd)
        {
            var generatedId = Guid.NewGuid();
            var responseContent = new StringContent(generatedId.ToString());
            mockHttpMessageHandler
                .When(RequestUrl)
                .WithJsonContent(addWorkingHoursDto)
                .Respond(HttpStatusCode.Created, responseContent); 
            expectedResult.Add(addWorkingHoursDto.ToWorkingHoursDto(generatedId));
        }
        httpClientMock = new HttpClient(mockHttpMessageHandler);
        httpClientMock.BaseAddress = new Uri(BaseUrl);
        toastServiceMock = Substitute.For<IToastService>();
        var sut = GetSut();
        //When
        var result = await sut.AddAndReturnDtos(workingHoursToAdd);
        //Then
        toastServiceMock.Received(1).ShowSuccess(Arg.Any<string>());
        toastServiceMock.DidNotReceiveWithAnyArgs().ShowError(string.Empty);
        result.Should().BeEquivalentTo(expectedResult);
    }
}