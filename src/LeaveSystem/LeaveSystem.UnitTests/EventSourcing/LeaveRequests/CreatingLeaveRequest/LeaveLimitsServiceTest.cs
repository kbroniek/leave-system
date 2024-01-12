using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Tests;

public class LeaveLimitsServiceTest
{
    [Theory]
    [InlineData("2023-07-16 +0", "2023-07-17 +0")]
    [InlineData("2023-01-01 +0", "2023-01-02 +0")]
    [InlineData("2023-12-30 +0", "2023-12-31 +0")]
    [InlineData("2023-01-01 +0", "2023-12-31 +0")]
    public async Task WhenValidDate_ThenReturnLimit(string dateFrom, string dateTo)
    {
        // Given
        var leaveTypeId = Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d01");
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        const string userId = "fakeUserId";
        var userLeaveLimit = await AddAndGetLimit(dbContext, leaveTypeId, userId);
        var sut = new LeaveLimitsService(dbContext);
        // When
        var result = await sut.GetLimit(
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            leaveTypeId,
            userId);
        // Then
        result.Should().BeEquivalentTo(userLeaveLimit);
    }

    [Theory]
    [InlineData("2022-07-12 +0", "2022-07-13 +0")]
    [InlineData("2024-01-01 +0", "2024-01-02 +0")]
    [InlineData("2022-12-30 +0", "2022-12-31 +0")]
    [InlineData("2022-01-01 +0", "2022-12-31 +0")]
    public async Task WhenInvalidDate_ThenThrowValidationException(string dateFrom, string dateTo)
    {
        // Given
        var leaveTypeId = Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d02");
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        const string userId = "fakeUserId";
        var userLeaveLimit = await AddAndGetLimit(dbContext, leaveTypeId, userId);
        var sut = new LeaveLimitsService(dbContext);
        // When
        var act = () => sut.GetLimit(
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            leaveTypeId,
            userId);
        // Then
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Cannot find limits for the leave type id: 82b52b33-4cbc-40d9-9414-116e5c918d02. Add limits for the user fakeUserId.");
    }

    [Fact]
    public async Task WhenWrongUser_ThenThrowValidationException()
    {
        // Given
        var leaveTypeId = Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d03");
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        const string userId = "fakeUserId";
        await AddAndGetLimit(dbContext, leaveTypeId, userId);
        var sut = new LeaveLimitsService(dbContext);
        // When
        var act = () => sut.GetLimit(
            DateTimeOffset.Parse("2023-08-12 +0"),
            DateTimeOffset.Parse("2023-08-13 +0"),
            leaveTypeId,
            "fakeAnotherId");
        // Then
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Cannot find limits for the leave type id: 82b52b33-4cbc-40d9-9414-116e5c918d03. Add limits for the user fakeAnotherId.");
    }

    [Fact]
    public async Task WhenWrongLeaveTypeId_ThenThrowValidationException()
    {
        // Given
        var leaveTypeId = Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d04");
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        const string userId = "fakeUserId";
        await AddAndGetLimit(dbContext, leaveTypeId, userId);
        var sut = new LeaveLimitsService(dbContext);
        // When
        var act = () => sut.GetLimit(
            DateTimeOffset.Parse("2023-08-12 +0"),
            DateTimeOffset.Parse("2023-08-13 +0"),
            Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d05"),
            userId);
        // Then
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Cannot find limits for the leave type id: 82b52b33-4cbc-40d9-9414-116e5c918d05. Add limits for the user fakeUserId.");
    }

    [Fact]
    public async Task WhenMoreThanOneValidLimits_ThenThrowValidationException()
    {
        // Given
        var leaveTypeId = Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d06");
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        const string userId = "fakeUserId";
        await dbContext.UserLeaveLimits.AddAsync(new UserLeaveLimit
        {
            Id = Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d10"),
            LeaveTypeId = leaveTypeId,
            Limit = TimeSpan.FromHours(24),
            AssignedToUserId = userId,
            ValidSince = DateTimeOffset.Parse("2023-05-01 +0"),
            ValidUntil = DateTimeOffset.Parse("2023-05-31 +0"),
        });
        await AddAndGetLimit(dbContext, leaveTypeId, userId);
        var sut = new LeaveLimitsService(dbContext);
        // When
        var act = () => sut.GetLimit(
            DateTimeOffset.Parse("2023-05-12 +0"),
            DateTimeOffset.Parse("2023-05-13 +0"),
            leaveTypeId,
            userId);
        // Then
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Two or more limits found which are the same for the leave type id: 82b52b33-4cbc-40d9-9414-116e5c918d06. User fakeUserId.");
    }

    private static async Task<UserLeaveLimit> AddAndGetLimit(LeaveSystemDbContext dbContext, Guid leaveTypeId, string userId)
    {
        var userLeaveLimit = new UserLeaveLimit
        {
            Id = Guid.Parse("82b52b33-4cbc-40d9-9414-116e5c918d00"),
            LeaveTypeId = leaveTypeId,
            Limit = TimeSpan.FromHours(24),
            AssignedToUserId = userId,
            ValidSince = DateTimeOffset.Parse("2023-01-01 +0"),
            ValidUntil = DateTimeOffset.Parse("2023-12-31 +0"),
        };
        await dbContext.UserLeaveLimits.AddAsync(userLeaveLimit);
        await dbContext.SaveChangesAsync();
        return userLeaveLimit;
    }
}