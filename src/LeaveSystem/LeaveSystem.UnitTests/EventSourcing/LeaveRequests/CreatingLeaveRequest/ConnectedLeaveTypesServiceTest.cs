using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Tests;

public class ConnectedLeaveTypesServiceTest
{
    [Fact]
    public async Task WhenDefault_ThenReturnDefault()
    {
        // Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        Guid nestedLeaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9330");
        Guid leaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9331");
        await AddLeaveType(dbContext, nestedLeaveTypeId, leaveTypeId);
        var sut = new ConnectedLeaveTypesService(dbContext);
        // When
        var result = await sut.GetConnectedLeaveTypeIds(nestedLeaveTypeId);
        // Then
        result.baseLeaveTypeId.Should().Be(leaveTypeId);
        result.nestedLeaveTypeIds.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenCannotFind_ThenReturnNull()
    {
        // Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        Guid nestedLeaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9332");
        Guid leaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9333");
        await AddLeaveType(dbContext, nestedLeaveTypeId, leaveTypeId);
        var sut = new ConnectedLeaveTypesService(dbContext);
        // When
        var result = await sut.GetConnectedLeaveTypeIds(Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9334"));
        // Then
        result.baseLeaveTypeId.Should().BeNull();
        result.nestedLeaveTypeIds.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenBaseLeaveTypeFound_ThenReturnNullAndNestedLeaveTypes()
    {
        // Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        Guid nestedLeaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9335");
        Guid leaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9336");
        await AddLeaveType(dbContext, nestedLeaveTypeId, leaveTypeId);
        var sut = new ConnectedLeaveTypesService(dbContext);
        // When
        var result = await sut.GetConnectedLeaveTypeIds(leaveTypeId);
        // Then
        result.baseLeaveTypeId.Should().BeNull();
        result.nestedLeaveTypeIds.Should().BeEquivalentTo(new[] { nestedLeaveTypeId });
    }
    [Fact]
    public async Task WhenAllFound_ThenReturnIdAndNestedLeaveTypes()
    {
        // Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        Guid nestedLeaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9337");
        Guid leaveTypeId = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9338");
        Guid baseLeaveType = Guid.Parse("3c503f12-60d9-4e7d-b950-28503b3d9339");
        var leaveType = new LeaveType
        {
            Id = leaveTypeId,
            Name = "BaseLeaveType",
            BaseLeaveTypeId = baseLeaveType
        };
        await dbContext.LeaveTypes.AddAsync(leaveType);
        await AddLeaveType(dbContext, nestedLeaveTypeId, leaveTypeId);
        var sut = new ConnectedLeaveTypesService(dbContext);
        // When
        var result = await sut.GetConnectedLeaveTypeIds(leaveTypeId);
        // Then
        result.baseLeaveTypeId.Should().Be(baseLeaveType);
        result.nestedLeaveTypeIds.Should().BeEquivalentTo(new[] { nestedLeaveTypeId });
    }

    private static async Task AddLeaveType(LeaveSystemDbContext dbContext, Guid leaveTypeId, Guid baseLeaveType)
    {
        var leaveType = new LeaveType
        {
            Id = leaveTypeId,
            Name = "ConnectedLeaveType",
            BaseLeaveTypeId = baseLeaveType,
        };
        await dbContext.LeaveTypes.AddAsync(leaveType);
        await dbContext.SaveChangesAsync();
    }
}