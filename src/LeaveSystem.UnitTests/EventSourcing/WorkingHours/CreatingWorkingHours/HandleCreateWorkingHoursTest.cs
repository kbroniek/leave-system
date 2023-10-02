using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using MediatR;
using NSubstitute;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.CreatingWorkingHours;

public class HandleCreateWorkingHoursTest
{
    private IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursRepositoryMock;
    private WorkingHoursFactory factoryMock;
    private IDocumentSession documentSessionMock;

    private HandleCreateWorkingHours GetSut() => new(workingHoursRepositoryMock, factoryMock, documentSessionMock);

    [Theory]
    [MemberData(nameof(Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData))]
    public async Task WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue(IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursEnumerable)
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForFakeoslav();
        factoryMock = Substitute.For<WorkingHoursFactory>();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration, command.CreatedBy));
        factoryMock.Create(command).Returns(
            fakeWorkingHours
        );
        workingHoursRepositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        documentSessionMock = Substitute.For<IDocumentSession>();
        var workingHoursMartenQueryable = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHoursEnumerable.ToList()
        );
        documentSessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(workingHoursMartenQueryable);
        var sut = GetSut(); 
        //When
        var result = await sut.Handle(command, CancellationToken.None);
        //Then
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        factoryMock.Received(1).Create(ArgExtensions.IsEquivalentTo<CreateWorkingHours>(command));
        await workingHoursRepositoryMock.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).SaveChanges();
        result.Should().BeEquivalentTo(Unit.Value);
    }
    
    [Fact]
    public async Task WhenHandlingSuccessful_DeprecateOldWorkingHoursForUserCreateNewWorkingHoursAndReturnUnitValue()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        factoryMock = Substitute.For<WorkingHoursFactory>();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration, command.CreatedBy));
        factoryMock.Create(command).Returns(
            fakeWorkingHours
        );
        workingHoursRepositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        documentSessionMock = Substitute.For<IDocumentSession>();
        var workingHoursEnumerable = FakeWorkingHoursProvider.GetDeprecated().ToList();
        var martenQueryable = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHoursEnumerable.ToList()
        );
        documentSessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryable);
        var sut = GetSut();
        //When
        var result = await sut.Handle(command, CancellationToken.None);
        //Then
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        factoryMock.Received(1).Create(ArgExtensions.IsEquivalentTo<CreateWorkingHours>(command));
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs().Update(Arg.Any<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).SaveChanges();
        result.Should().BeEquivalentTo(Unit.Value);
        var now = DateTimeOffset.Now.GetDayWithoutTime();
        martenQueryable.Any(x => x.UserId == command.UserId && x.GetStatus(now) == WorkingHoursStatus.Current)
            .Should().BeFalse();
    }

    public static IEnumerable<object[]> Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData()
    {
        var now = DateTimeOffset.Now;
        yield return new object[] { new [] { FakeWorkingHoursProvider.GetCurrentForBen(now), FakeWorkingHoursProvider.GetCurrentForPhilip(now)}.Union(FakeWorkingHoursProvider.GetDeprecated()) };
        yield return new object[] { new [] { FakeWorkingHoursProvider.GetCurrentForBen(now), FakeWorkingHoursProvider.GetCurrentForPhilip(now)} };
    }
}
