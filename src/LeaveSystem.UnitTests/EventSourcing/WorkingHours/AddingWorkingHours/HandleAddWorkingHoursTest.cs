using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using MediatR;
using Moq;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.AddingWorkingHours;

public class HandleAddWorkingHoursTest
{
    private IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursRepositoryMock;
    private WorkingHoursFactory factoryMock;
    private IDocumentSession documentSessionMock;
    private IRepository<LeaveRequest> leaveRequestRepositoryMock;

    private HandleAddWorkingHours GetSut() => new(workingHoursRepositoryMock, factoryMock, documentSessionMock, leaveRequestRepositoryMock);

    [Theory]
    [MemberData(nameof(Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData))]
    public async Task WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue(IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursEnumerable)
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForFakeoslav();
        factoryMock = Substitute.For<WorkingHoursFactory>();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration));
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
        leaveRequestRepositoryMock = Substitute.For<IRepository<LeaveRequest>>();
        var leaveRequestsMartenQueryable = new MartenQueryableStub<LeaveRequest>(
            FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedEventsWithDifferentIds().Select(
                LeaveRequest.CreatePendingLeaveRequest).ToList()
        );
        leaveRequestsMartenQueryable.ElementAt(2).Accept("",command.AddedBy);
        documentSessionMock.Query<LeaveRequest>()
            .Returns(leaveRequestsMartenQueryable);
        var sut = GetSut();
        var userValidLeaveRequestCount = await leaveRequestsMartenQueryable.CountAsync(x => x.CreatedBy.Id == command.AddedBy.Id && x.Status.IsValid());
        //When
        var result = await sut.Handle(command, CancellationToken.None);
        //Then
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        documentSessionMock.Received(1).Query<LeaveRequest>();
        factoryMock.Received(1).Create(ArgExtensions.IsEquivalentTo<AddWorkingHours>(command));
        await workingHoursRepositoryMock.Received(1).Update(Arg.Any<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(2).SaveChanges();
        await leaveRequestRepositoryMock.Received(userValidLeaveRequestCount).Update(Arg.Any<LeaveRequest>(), Arg.Any<CancellationToken>());
        await leaveRequestRepositoryMock.Received(userValidLeaveRequestCount).SaveChanges();
        result.Should().BeEquivalentTo(Unit.Value);
        workingHoursMartenQueryable.Any(x => x.UserId == command.UserId && x.Status == WorkingHoursStatus.Current)
            .Should().BeFalse();
        leaveRequestsMartenQueryable.Any(x => x.Status.IsValid()).Should().BeFalse();
    }
    
    [Fact]
    public async Task WhenHandlingSuccessful_DeprecateOldWorkingHoursForUserCreateNewWorkingHoursAndReturnUnitValue()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        factoryMock = Substitute.For<WorkingHoursFactory>();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration));
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
        factoryMock.Received(1).Create(ArgExtensions.IsEquivalentTo<AddWorkingHours>(command));
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs().Update(Arg.Any<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).SaveChanges();
        result.Should().BeEquivalentTo(Unit.Value);
        martenQueryable.Any(x => x.UserId == command.UserId && x.Status == WorkingHoursStatus.Current)
            .Should().BeFalse();
    }

    public static IEnumerable<object[]> Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData()
    {
        yield return new object[] { FakeWorkingHoursProvider.GetAll() };
        yield return new object[] { FakeWorkingHoursProvider.GetCurrent() };
    }
}
