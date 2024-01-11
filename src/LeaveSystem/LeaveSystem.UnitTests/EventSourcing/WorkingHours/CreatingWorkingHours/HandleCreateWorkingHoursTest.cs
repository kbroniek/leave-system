using FluentAssertions;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.CreatingWorkingHours;

public class HandleCreateWorkingHoursTest
{
    private readonly IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursRepositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
    private readonly WorkingHoursFactory factoryMock = Substitute.For<WorkingHoursFactory>();
    private readonly IDocumentSession documentSessionMock = Substitute.For<IDocumentSession>();

    private HandleCreateWorkingHours GetSut() => new(workingHoursRepositoryMock, factoryMock, documentSessionMock);

    [Theory]
    [MemberData(nameof(Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData))]
    public async Task WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue(IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursEnumerable)
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForFakeoslav();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration, command.CreatedBy));
        factoryMock.Create(command).Returns(
            fakeWorkingHours
        );
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
        await workingHoursRepositoryMock.Received(1).AddAsync(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).SaveChangesAsync();
        result.Should().BeEquivalentTo(Unit.Value);
    }

    [Fact]
    public async Task WhenHandlingSuccessful_DeprecateOldWorkingHoursForUserCreateNewWorkingHoursAndReturnUnitValue()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration, command.CreatedBy));
        factoryMock.Create(command).Returns(
            fakeWorkingHours
        );
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
        await workingHoursRepositoryMock.DidNotReceiveWithAnyArgs().UpdateAsync(Arg.Any<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).AddAsync(fakeWorkingHours, Arg.Any<CancellationToken>());
        await workingHoursRepositoryMock.Received(1).SaveChangesAsync();
        result.Should().BeEquivalentTo(Unit.Value);
        var now = DateTimeOffset.Now.GetDayWithoutTime();
        martenQueryable.Any(x => x.UserId == command.UserId && x.GetStatus(now) == WorkingHoursStatus.Current)
            .Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Get_Failed_TestData))]
    public async Task WhenQueryReturnsTrue_ThenThrowError(CreateWorkingHours command)
    {
        //Given
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration, command.CreatedBy));
        factoryMock.Create(command).Returns(
            fakeWorkingHours
        );
        var martenQueryable = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            new LeaveSystem.EventSourcing.WorkingHours.WorkingHours[]
            {
                LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(WorkingHoursCreated.Create(
                    Guid.NewGuid(),
                    command.UserId,
                    DateTimeOffsetExtensions.CreateFromDate(2023, 1, 1),
                    DateTimeOffsetExtensions.CreateFromDate(2023, 6, 16),
                    TimeSpan.FromHours(8),
                    command.CreatedBy))
            }
        );
        documentSessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryable);
        var sut = GetSut();
        //When
        var act = () => sut.Handle(command, CancellationToken.None);
        //Then
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("You cant add working hours in this period, because other overlap it");
    }

    public static IEnumerable<object[]> Get_WhenHandlingSuccessful_PassAllCreateWorkingHoursAndReturnUnitValue_TestData()
    {
        var now = DateTimeOffset.Now;
        yield return new object[] { new[] { FakeWorkingHoursProvider.GetCurrentForBen(now), FakeWorkingHoursProvider.GetCurrentForPhilip(now) }.Union(FakeWorkingHoursProvider.GetDeprecated()) };
        yield return new object[] { new[] { FakeWorkingHoursProvider.GetCurrentForBen(now), FakeWorkingHoursProvider.GetCurrentForPhilip(now) } };
    }
    public static IEnumerable<object[]> Get_Failed_TestData()
    {
        var dateToEqualDateFrom = CreateWorkingHours.Create(
            Guid.Parse("9feda829-4c39-4410-97d8-da9dcd504a64"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2020, 12, 1),
            DateTimeOffsetExtensions.CreateFromDate(2023, 1, 1),
            TimeSpan.FromHours(1),
            FakeUserProvider.GetUserWithNameFakeoslav());
        var dateToEqualDateFromADayAfter = CreateWorkingHours.Create(
            Guid.Parse("9c597c0d-72d5-49a1-98d6-c929de475b61"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2020, 12, 1),
            DateTimeOffsetExtensions.CreateFromDate(2023, 1, 2),
            TimeSpan.FromHours(2),
            FakeUserProvider.GetUserWithNameFakeoslav());
        var dateFromEqualDateTo = CreateWorkingHours.Create(
            Guid.Parse("9ea988d3-25ae-43a1-9660-0c5aacba8777"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 6, 16),
            DateTimeOffsetExtensions.CreateFromDate(2024, 1, 1),
            TimeSpan.FromHours(3),
            FakeUserProvider.GetUserWithNameFakeoslav());
        var dateFromEqualDateToADayBefore = CreateWorkingHours.Create(
            Guid.Parse("02629e8b-d66e-4242-809d-3209eb2ef37a"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 6, 15),
            DateTimeOffsetExtensions.CreateFromDate(2024, 1, 1),
            TimeSpan.FromHours(4),
            FakeUserProvider.GetUserWithNameFakeoslav());
        var equal = CreateWorkingHours.Create(
            Guid.Parse("cf4cbdb8-a29a-4b28-b8c5-a1544cf85db1"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 1, 1),
            DateTimeOffsetExtensions.CreateFromDate(2023, 6, 16),
            TimeSpan.FromHours(5),
            FakeUserProvider.GetUserWithNameFakeoslav());
        var equalADayBeforeAndAfter = CreateWorkingHours.Create(
            Guid.Parse("cf4cbdb8-a29a-4b28-b8c5-a1544cf85db1"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 1, 2),
            DateTimeOffsetExtensions.CreateFromDate(2023, 6, 15),
            TimeSpan.FromHours(5),
            FakeUserProvider.GetUserWithNameFakeoslav());
        var largerTimeSpan = CreateWorkingHours.Create(
            Guid.Parse("a72c1c50-6182-419b-8f80-42c22f2b20ca"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2022, 12, 31),
            DateTimeOffsetExtensions.CreateFromDate(2023, 6, 17),
            TimeSpan.FromHours(6),
            FakeUserProvider.GetUserWithNameFakeoslav());
        var smallerTimeSpan = CreateWorkingHours.Create(
            Guid.Parse("38fc764e-e919-4733-9b78-2388ff922913"),
            FakeUserProvider.BenId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 01, 31),
            DateTimeOffsetExtensions.CreateFromDate(2023, 4, 17),
            TimeSpan.FromHours(7),
            FakeUserProvider.GetUserWithNameFakeoslav());
        return new CreateWorkingHours[]
        {
            dateToEqualDateFrom,
            dateToEqualDateFromADayAfter,
            dateFromEqualDateTo,
            dateFromEqualDateToADayBefore,
            equal,
            equalADayBeforeAndAfter,
            largerTimeSpan,
            smallerTimeSpan
        }.Select(x => new[] { x });
    }
}
