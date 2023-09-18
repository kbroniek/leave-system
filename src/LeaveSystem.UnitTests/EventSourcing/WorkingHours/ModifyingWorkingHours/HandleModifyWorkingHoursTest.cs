using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;
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

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.ModifyingWorkingHours;

public class HandleModifyWorkingHoursTest
{
    private IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursRepositoryMock;
    private IDocumentSession documentSessionMock;
    private IRepository<LeaveRequest> leaveRequestRepositoryMock;

    private HandleModifyWorkingHours GetSut() =>
        new(workingHoursRepositoryMock, documentSessionMock, leaveRequestRepositoryMock);

    [Theory]
    [MemberData(nameof(Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData))]
    public async Task WhenHandlingSuccessful_PassAllMethodsDeprecateLeaveRequestsAndReturnUnitValue(
        IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursEnumerable)
    {
        //Given
        var command = ModifyWorkingHours.Create(
            Guid.NewGuid(),
            FakeUserProvider.FakseoslavId,
            DateTimeOffsetExtensions.CreateFromDate(2020, 6, 2),
            DateTimeOffsetExtensions.CreateFromDate(2023, 10, 11),
            TimeSpan.FromHours(8),
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        var now = DateTimeOffset.Now;
        var fakeWorkingHours = FakeWorkingHoursProvider.GetCurrentForFakeoslav(now);
        workingHoursRepositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        workingHoursRepositoryMock.FindById(command.WorkingHoursId).Returns(fakeWorkingHours);
        documentSessionMock = Substitute.For<IDocumentSession>();
        var workingHoursMartenQueryable = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHoursEnumerable.ToList()
        );
        documentSessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(workingHoursMartenQueryable);
        var leaveRequestsMartenQueryable = new MartenQueryableStub<LeaveRequest>(
            FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedEventsWithDifferentIds()
                .Select(LeaveRequest.CreatePendingLeaveRequest).ToArray()
        );
        documentSessionMock.Query<LeaveRequest>()
            .Returns(leaveRequestsMartenQueryable);
        var overlappingLeaveRequestsCount = await 
            leaveRequestsMartenQueryable.CountAsync(x => x.PeriodsOverlap(command) && x.CreatedBy.Id == command.UserId);
        leaveRequestRepositoryMock = Substitute.For<IRepository<LeaveRequest>>();
        var sut = GetSut();
        //When
        var result = await sut.Handle(command, CancellationToken.None);
        //Then
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        await workingHoursRepositoryMock.Received(1).Update(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).SaveChanges();
        await leaveRequestRepositoryMock.Received(overlappingLeaveRequestsCount).Update(Arg.Any<LeaveRequest>(), Arg.Any<CancellationToken>());
        await leaveRequestRepositoryMock.Received(overlappingLeaveRequestsCount).SaveChanges();
        fakeWorkingHours.DequeueUncommittedEvents().Last().Should().BeOfType<WorkingHoursModified>();
        result.Should().BeEquivalentTo(Unit.Value);
        leaveRequestsMartenQueryable.Any(x => x.PeriodsOverlap(command) && x.CreatedBy.Id == command.UserId && x.Status.IsValid())
            .Should().BeFalse();
    }

    [Fact]
    public async Task WhenRequestDateOverlapsOtherWorkingHours_ThenThrowInvalidOperationException()
    {
        //Given
        var command = ModifyWorkingHours.Create(
            Guid.NewGuid(),
            FakeUserProvider.FakseoslavId,
            DateTimeOffsetExtensions.CreateFromDate(2020, 1, 1),
            DateTimeOffsetExtensions.CreateFromDate(2023, 10, 11),
            TimeSpan.FromHours(8),
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration, command.ModifiedBy));
        workingHoursRepositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        documentSessionMock = Substitute.For<IDocumentSession>();
        var workingHoursEnumerable = FakeWorkingHoursProvider.GetDeprecated().ToList();
        var martenQueryable = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHoursEnumerable.ToList()
        );
        documentSessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryable);
        leaveRequestRepositoryMock = Substitute.For<IRepository<LeaveRequest>>();
        var sut = GetSut();
        //When
        var act = async () =>
        {
            await sut.Handle(command, CancellationToken.None);
        };
        //Then
        await act.Should().ThrowAsync<InvalidOperationException>();
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs()
            .Update(Arg.Any<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs().Add(default, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs().SaveChanges();
        await leaveRequestRepositoryMock.DidNotReceiveWithAnyArgs().Update(default);
        await leaveRequestRepositoryMock.DidNotReceiveWithAnyArgs().SaveChanges();
        var now = DateTimeOffset.Now.GetDayWithoutTime();
        martenQueryable.Any(x => x.UserId == command.UserId && x.GetStatus(now) == WorkingHoursStatus.Current)
            .Should().BeFalse();
    }

    public static IEnumerable<object[]>
        Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData()
    {
        var now = DateTimeOffset.Now;
        yield return new object[]
        {
            new[] { FakeWorkingHoursProvider.GetCurrentForBen(now), FakeWorkingHoursProvider.GetCurrentForPhilip(now) }
                .Union(FakeWorkingHoursProvider.GetDeprecated())
        };
        yield return new object[]
        {
            new[] { FakeWorkingHoursProvider.GetCurrentForBen(now), FakeWorkingHoursProvider.GetCurrentForPhilip(now) }
        };
    }
}