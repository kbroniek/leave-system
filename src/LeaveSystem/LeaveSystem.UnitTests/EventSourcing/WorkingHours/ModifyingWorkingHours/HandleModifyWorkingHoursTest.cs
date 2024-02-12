using GoldenEye.Backend.Core.DDD.Events;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;
using LeaveSystem.Extensions;
using LeaveSystem.Linq;
using LeaveSystem.Periods;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using MediatR;
using NSubstitute;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.ModifyingWorkingHours;

public class HandleModifyWorkingHoursTest
{
    private readonly IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursRepositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
    private readonly IDocumentSession documentSessionMock = Substitute.For<IDocumentSession>();
    private readonly IRepository<LeaveRequest> leaveRequestRepositoryMock = Substitute.For<IRepository<LeaveRequest>>();

    private HandleModifyWorkingHours GetSut() =>
        new(workingHoursRepositoryMock, documentSessionMock, leaveRequestRepositoryMock);

    [Theory]
    [MemberData(nameof(Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData))]
    public async Task WhenHandlingSuccessful_PassAllMethodsDeprecateLeaveRequestsAndReturnUnitValue(
        IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursEnumerable)
    {
        //Given
        var command = ModifyWorkingHours.Create(
            Guid.Parse("95e4766c-0b2c-46ed-ad20-00fe76a11637"),
            FakeUserProvider.FakseoslavId,
            DateTimeOffsetExtensions.CreateFromDate(2020, 6, 2),
            DateTimeOffsetExtensions.CreateFromDate(2023, 10, 11),
            TimeSpan.FromHours(8),
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        var now = FakeDateServiceProvider.GetDateService().UtcNow();
        var fakeWorkingHours = FakeWorkingHoursProvider.GetCurrentForFakeoslav(now);
        workingHoursRepositoryMock.FindByIdAsync(command.WorkingHoursId).Returns(Task.FromResult(fakeWorkingHours));
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
        var overlapPeriodExp = PeriodExpressions.GetPeriodOverlapExp<LeaveRequest, ModifyWorkingHours>(command);
        var overlappingLeaveRequestsCount =
            await leaveRequestsMartenQueryable.CountAsync(overlapPeriodExp.And(x => x.CreatedBy.Id == command.UserId));
        var sut = GetSut();
        //When
        var result = await sut.Handle(command, CancellationToken.None);
        //Then
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        await workingHoursRepositoryMock.Received(1).UpdateAsync(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).SaveChangesAsync();
        await leaveRequestRepositoryMock.Received(overlappingLeaveRequestsCount).UpdateAsync(Arg.Any<LeaveRequest>(), Arg.Any<CancellationToken>());
        await leaveRequestRepositoryMock.Received(overlappingLeaveRequestsCount).SaveChangesAsync();
        (fakeWorkingHours as IEventSource).PendingEvents.Last().Should().BeOfType<WorkingHoursModified>();
        result.Should().BeEquivalentTo(Unit.Value);
        leaveRequestsMartenQueryable.Any(overlapPeriodExp.And(x => x.CreatedBy.Id == command.UserId && x.Status.IsValid()))
            .Should().BeFalse();
    }

    [Fact]
    public async Task WhenRequestDateOverlapsOtherWorkingHours_ThenThrowInvalidOperationException()
    {
        //Given
        var command = ModifyWorkingHours.Create(
            Guid.Parse("95e4766c-0b2c-46ed-ad20-00fe76a11638"),
            FakeUserProvider.FakseoslavId,
            DateTimeOffsetExtensions.CreateFromDate(2020, 1, 1),
            DateTimeOffsetExtensions.CreateFromDate(2023, 10, 11),
            TimeSpan.FromHours(8),
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration, command.ModifiedBy));
        var workingHoursEnumerable = FakeWorkingHoursProvider.GetDeprecated().ToList();
        var martenQueryable = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHoursEnumerable.ToList()
        );
        documentSessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryable);
        var sut = GetSut();
        //When
        var act = async () => await sut.Handle(command, CancellationToken.None);
        //Then
        await act.Should().ThrowAsync<InvalidOperationException>();
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs()
            .UpdateAsync(Arg.Any<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs().AddAsync(default!, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await leaveRequestRepositoryMock.DidNotReceiveWithAnyArgs().UpdateAsync(default!);
        await leaveRequestRepositoryMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        var now = FakeDateServiceProvider.GetDateService().UtcNowWithoutTime();
        martenQueryable.Any(x => x.UserId == command.UserId && x.GetStatus(now) == WorkingHoursStatus.Current)
            .Should().BeFalse();
    }

    public static IEnumerable<object[]>
        Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData()
    {
        var now = FakeDateServiceProvider.GetDateService().UtcNow();
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
