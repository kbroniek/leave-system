using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LimitValidatorTest : CreateLeaveRequestValidatorTest
{
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;
    private const string FakeOnDemandLeaveGuid = "f17b1956-0dd5-4972-bde9-851ee112a4e8";
    private const string FakeSickLeaveGuid = "0d3d94ac-e137-4c90-8485-af3ab3042547";

    private static readonly LeaveType FakeSickLeave = new()
    {
        Id = Guid.Parse(FakeSickLeaveGuid),
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
            BaseLeaveTypeId = Guid.Parse(FakeOnDemandLeaveGuid),
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
            Id = Guid.Parse(FakeOnDemandLeaveGuid),
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
        FakeSickLeave
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
                Guid.Parse(FakeOnDemandLeaveGuid),
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
                Guid.Parse(FakeSickLeaveGuid),
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
        await RefillDbAsync();
        var sut = GetSut(DbContext);
        //When
        
        var act = async () => { await sut.LimitValidator(@event); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
    }
    private async Task RefillDbAsync()
    {
        await ReinitializeDbAsync();
        await AddLeaveTypesToDbAsync();
        await AddUserLeaveLimitsToDbAsync();
        await DbContext.SaveChangesAsync();
    }

    private async Task ReinitializeDbAsync()
    {
        DbContext = await DbContextFactory.CreateDbContextAsync();
    }
    private async Task AddLeaveTypesToDbAsync()
    {
        await DbContext.LeaveTypes.AddRangeAsync(
            LeaveTypesTestData()
        );
    }
    
    private async Task AddUserLeaveLimitsToDbAsync()
    {
        var now = DateTimeOffset.Now;
        var leaveLimit = new UserLeaveLimit
        {
            LeaveTypeId = Guid.Parse(FakeSickLeaveGuid),
            Limit = FakeSickLeave.Properties?.DefaultLimit,
            AssignedToUserId = FakeUser.Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
        await DbContext.UserLeaveLimits.AddAsync(leaveLimit);
    }

    [Fact]
    public async Task
        WhenCreatedLeaveTypeOrIsWithinTheLimit_ThenNotThrowValidationException()
    {
        //Given
        await RefillDbAsync();
        var sut = GetSut(DbContext);
        var fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            FakeLeaveRequestCreatedEvent.LeaveRequestId,
            FakeLeaveRequestCreatedEvent.DateFrom, 
            FakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours * 3,
            Guid.Parse(FakeSickLeaveGuid),
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
        await ReinitializeDbAsync();
        var sut = GetSut(DbContext);
        var fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            FakeLeaveRequestCreatedEvent.LeaveRequestId,
            FakeLeaveRequestCreatedEvent.DateFrom, 
            FakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours * 3,
            Guid.Parse(FakeSickLeaveGuid),
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
        await RefillDbAsync();
        var now = DateTimeOffset.Now;
        var secondLeaveLimitForSameUser = new UserLeaveLimit
        {
            LeaveTypeId = Guid.Parse(FakeSickLeaveGuid),
            Limit = FakeSickLeave.Properties?.DefaultLimit,
            AssignedToUserId = FakeUser.Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
        await DbContext.UserLeaveLimits.AddAsync(secondLeaveLimitForSameUser);
        await DbContext.SaveChangesAsync();
        var sut = GetSut(DbContext);
        var fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            FakeLeaveRequestCreatedEvent.LeaveRequestId,
            FakeLeaveRequestCreatedEvent.DateFrom, 
            FakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours * 3,
            Guid.Parse(FakeSickLeaveGuid),
            FakeLeaveRequestCreatedEvent.Remarks,
            FakeUser
        );
        //When
        var act = async () => { await sut.LimitValidator(fakeLeaveRequestCreatedEvent); };
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Two or more limits found which are the same for the leave type id*");
    }
}