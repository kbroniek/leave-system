using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LimitValidatorTest : CreateLeaveRequestValidatorTest
{
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;
    private const string FakeOnDemandLeaveId = "f17b1956-0dd5-4972-bde9-851ee112a4e8";
    private const string FakeSickLeaveId = "0d3d94ac-e137-4c90-8485-af3ab3042547";
    private const string FakeUserLeaveLimitId = "1d19cb47-9e68-4945-a28c-380a48c19d6e";

    private static LeaveType GetFakeSickLeave() => new()
    {
        Id = Guid.Parse(FakeSickLeaveId),
        Name = "niezdolność do pracy z powodu choroby",
        Order = 3,
        Properties = new LeaveType.LeaveTypeProperties
        {
            DefaultLimit = WorkingHours * 5,
            IncludeFreeDays = true,
            Color = "red",
            Catalog = LeaveTypeCatalog.Sick,
        }
    };

    private IEnumerable<LeaveType> LeaveTypesTestData() => new[]
    {
        new LeaveType
        {
            Id = Guid.Parse(FakeHolidayLeaveGuid),
            Name = "urlop wypoczynkowy",
            Order = 1,
            BaseLeaveTypeId = Guid.Parse(FakeOnDemandLeaveId),
            Properties = new LeaveType.LeaveTypeProperties
            {
                DefaultLimit = WorkingHours * 26,
                IncludeFreeDays = false,
                Color = "blue",
                Catalog = LeaveTypeCatalog.Holiday,
            }
        },
        new LeaveType
        {
            Id = Guid.Parse(FakeOnDemandLeaveId),
            Name = "urlop na żądanie",
            Order = 2,
            Properties = new LeaveType.LeaveTypeProperties
            {
                DefaultLimit = WorkingHours * 4,
                IncludeFreeDays = false,
                Color = "yellow",
                Catalog = LeaveTypeCatalog.OnDemand,
            }
        },
        GetFakeSickLeave()
    };

    public static IEnumerable<object[]> GetFakeLeaveRequestCreatedTestData()
    {
        yield return new object[]
        {
            LeaveRequestCreated.Create(
                Guid.Parse(FakeLeaveRequestId),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                WorkingHours * 30,
                Guid.Parse(FakeHolidayLeaveGuid),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            )
        };
        yield return new object[]
        {
            LeaveRequestCreated.Create(
                Guid.Parse(FakeLeaveRequestId),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                WorkingHours * 30,
                Guid.Parse(FakeOnDemandLeaveId),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            )
        };
        yield return new object[]
        {
            LeaveRequestCreated.Create(
                Guid.Parse(FakeLeaveRequestId),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                WorkingHours * 10,
                Guid.Parse(FakeSickLeaveId),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            )
        };
    }

    [Theory]
    [MemberData(nameof(GetFakeLeaveRequestCreatedTestData))]
    public async Task
        WhenCreatedLeaveTypeOrIsOutOfBaseLeaveTypeOrNestedLeaveType_ThenThrowValidationException(LeaveRequestCreated @event)
    {
        //Given
        using var dbContext = await CreateAndFillDbAsync();
        var sut = GetSut(dbContext);
        //When

        var act = async () => { await sut.LimitValidator(@event); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
    }
    private async Task<LeaveSystemDbContext> CreateAndFillDbAsync()
    {
        var dbContext = await DbContextFactory.CreateDbContextAsync();
        dbContext.UserLeaveLimits.Local.CollectionChanged += Local_CollectionChanged;
        await AddLeaveTypesToDbAsync(dbContext);
        await dbContext.SaveChangesAsync();
        await AddUserLeaveLimitsToDbAsync(dbContext);
        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    private void Local_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
    }

    private async Task AddLeaveTypesToDbAsync(LeaveSystemDbContext dbContext)
    {
        await dbContext.LeaveTypes.AddRangeAsync(
            LeaveTypesTestData()
        );
    }

    private async Task AddUserLeaveLimitsToDbAsync(LeaveSystemDbContext dbContext)
    {
        var now = DateTimeOffset.Parse("2023-07-06T12:30:00");
        var leaveLimit = new UserLeaveLimit
        {
            Id = Guid.Parse(FakeUserLeaveLimitId),
            LeaveTypeId = Guid.Parse(FakeSickLeaveId),
            Limit = GetFakeSickLeave().Properties?.DefaultLimit,
            AssignedToUserId = FakeUser.Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
        await dbContext.UserLeaveLimits.AddAsync(leaveLimit);
    }

    [Fact]
    public async Task
        WhenCreatedLeaveTypeOrIsWithinTheLimit_ThenNotThrowValidationException()
    {
        //Given
        using var dbContext = await CreateAndFillDbAsync();
        var sut = GetSut(dbContext);
        var fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            FakeLeaveRequestCreatedEvent.LeaveRequestId,
            FakeLeaveRequestCreatedEvent.DateFrom,
            FakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours * 3,
            Guid.Parse(FakeSickLeaveId),
            FakeLeaveRequestCreatedEvent.Remarks,
            FakeUser
        );
        //When
        var act = async () => { await sut.LimitValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task
        WhenNoLimitForLeaveRequestCreated_ThenThrowValidationException()
    {
        //Given
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = GetSut(dbContext);
        var fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            FakeLeaveRequestCreatedEvent.LeaveRequestId,
            FakeLeaveRequestCreatedEvent.DateFrom,
            FakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours * 3,
            Guid.Parse(FakeSickLeaveId),
            FakeLeaveRequestCreatedEvent.Remarks,
            FakeUser
        );
        //When
        var act = async () => { await sut.LimitValidator(fakeLeaveRequestCreatedEvent); };
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Cannot find limits for the leave type id*");
    }

    [Fact]
    public async Task
        WhenMoreThanOneLimitForLeaveRequestCreated_ThenThrowValidationException()
    {
        //Given
        using var dbContext = await CreateAndFillDbAsync();
        var now = DateTimeOffset.Now;
        var secondLeaveLimitForSameUser = new UserLeaveLimit
        {
            Id = Guid.Parse("8e9709d4-c237-4043-bde3-bb58bca35c2e"),
            LeaveTypeId = Guid.Parse(FakeSickLeaveId),
            Limit = GetFakeSickLeave().Properties?.DefaultLimit,
            AssignedToUserId = FakeUser.Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
        await dbContext.UserLeaveLimits.AddAsync(secondLeaveLimitForSameUser);
        await dbContext.SaveChangesAsync();
        var sut = GetSut(dbContext);
        var fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            FakeLeaveRequestCreatedEvent.LeaveRequestId,
            FakeLeaveRequestCreatedEvent.DateFrom,
            FakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours * 3,
            Guid.Parse(FakeSickLeaveId),
            FakeLeaveRequestCreatedEvent.Remarks,
            FakeUser
        );
        //When
        var act = async () => { await sut.LimitValidator(fakeLeaveRequestCreatedEvent); };
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Two or more limits found which are the same for the leave type id*");
    }
}