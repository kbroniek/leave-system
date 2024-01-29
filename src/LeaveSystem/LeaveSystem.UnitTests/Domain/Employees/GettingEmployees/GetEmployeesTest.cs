using LeaveSystem.Domain.Employees.GettingEmployees;
using LeaveSystem.Domain.Users;
using LeaveSystem.GraphApi;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using Moq;

namespace LeaveSystem.UnitTests.Domain.Employees;

public class GetEmployeesTest
{
    [Fact]
    public async Task WhenGetWithEmptyRoles_ThenThrowAnError()
    {
        //Given
        var fakeId = "12c8ef05-a4a5-4984-bfdc-c1c7116f4a0a";
        var getGraphUserServiceMock = new Mock<GetGraphUserService>(MockBehavior.Strict, null!, RoleAttributeNameResolver.Create("fake"));
        var calledBy = new FederatedUser(fakeId, null, null, Enumerable.Empty<string>());

        var sut = new HandleGetEmployees(getGraphUserServiceMock.Object);
        //When
        var act = () => sut.Handle(new GetEmployees(calledBy), CancellationToken.None);
        //Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You are not permitted to access this resource");
    }
    [Fact]
    public async Task WhenGetAsEmployee_ThenReturnOneEployee()
    {
        //Given
        var fakeId = "12c8ef05-a4a5-4984-bfdc-c1c7116f4a0b";
        var expected = new FederatedUser(fakeId, "fakeEmail@test.com", "fakeName", new string[] { RoleType.Employee.ToString() });
        var getGraphUserServiceMock = new Mock<GetGraphUserService>(MockBehavior.Strict, null!, RoleAttributeNameResolver.Create("fake"));
        getGraphUserServiceMock
            .Setup(x => x.Get(It.Is<string[]>(x => x.Contains(fakeId)), CancellationToken.None))
            .ReturnsAsync(new FederatedUser[]
            {
                expected,
                new(fakeId, "global@test.com", "fakeNameGlobal", new string[] { RoleType.GlobalAdmin.ToString() }),
                new(fakeId, "DecisionMaker@test.com", "fakeNameDecisionMaker", new string[] { RoleType.DecisionMaker.ToString() })
            });
        var calledBy = new FederatedUser(fakeId, null, null, new string[] { RoleType.Employee.ToString() });

        var sut = new HandleGetEmployees(getGraphUserServiceMock.Object);
        //When
        var result = await sut.Handle(new GetEmployees(calledBy), CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(new FederatedUser[] { expected });
    }

    [Theory]
    [MemberData(nameof(AllManagerRoles_TestData))]
    public async Task WhenGetAsManager_ThenReturnAllEployee(RoleType? employeeRoleType, RoleType roleType)
    {
        //Given
        var fakeId = "12c8ef05-a4a5-4984-bfdc-c1c7116f4a0c";
        var expected = new FederatedUser(fakeId, "fakeEmail@test.com", "fakeName", new string[] { RoleType.Employee.ToString() });
        var getGraphUserServiceMock = new Mock<GetGraphUserService>(MockBehavior.Strict, null!, RoleAttributeNameResolver.Create("fake"));
        getGraphUserServiceMock
            .Setup(x => x.Get(CancellationToken.None))
            .ReturnsAsync(new FederatedUser[]
            {
                expected,
                new(fakeId, "custom@test.com", "fakeNameCustom", new string[] { roleType.ToString() }),
                new(fakeId, "DecisionMaker@test.com", "fakeNameDecisionMaker", new string[] { RoleType.DecisionMaker.ToString() })
            });
        var calledBy = employeeRoleType is not null ?
            new FederatedUser(fakeId, null, null, new string[] { employeeRoleType.ToString()!, roleType.ToString() }) :
            new FederatedUser(fakeId, null, null, new string[] { roleType.ToString() });

        var sut = new HandleGetEmployees(getGraphUserServiceMock.Object);
        //When
        var result = await sut.Handle(new GetEmployees(calledBy), CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(new FederatedUser[] { expected });
    }


    public static TheoryData<RoleType?, RoleType> AllManagerRoles_TestData()
    {
        var managers = Enum.GetValues<RoleType>().Where(x => x != RoleType.Employee).ToArray();
        var data = new TheoryData<RoleType?, RoleType>();
        foreach (var m in managers)
        {
            data.Add(RoleType.Employee, m);
            data.Add(null, m);
        }
        return data;
    }
}
