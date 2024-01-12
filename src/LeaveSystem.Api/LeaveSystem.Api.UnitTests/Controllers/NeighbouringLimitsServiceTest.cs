using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class NeighbouringLimitsServiceTest
{
    [Fact]
    public async Task WhenValidSinceHasValue_ThenClosePreviousPeriod()
    {
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var fakeLimit = new UserLeaveLimit
        {   
            Id = Guid.Parse("05db1d8e-cec7-4b13-bd2a-48a1deffbc49"),
            AssignedToUserId = "eb49a7a5-2c5c-4b80-99c2-de3d2a3823fc",
            LeaveTypeId = Guid.Parse("a266f646-70f3-4f10-a07c-65726aae8a76"),
            ValidSince = DateTimeOffset.Parse("2024-01-12 +0:00"),
            ValidUntil = null
        };
        var expectedLimitToClose = new UserLeaveLimit()
        {
            Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
            ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
            ValidUntil = null,
            AssignedToUserId = fakeLimit.AssignedToUserId,
            LeaveTypeId = fakeLimit.LeaveTypeId
        };
        var dbSetMock = new UserLeaveLimit[]
        {
            new()
            {
               Id = Guid.Parse("9c79c0c0-b780-43f2-b824-c8ec242ac24d"),
               ValidSince = DateTimeOffset.Parse("2022-01-01 +0:00"),
               ValidUntil = DateTimeOffset.Parse("2022-12-31 +0:00"),
               AssignedToUserId = fakeLimit.AssignedToUserId,
               LeaveTypeId = Guid.Parse("3a9f6616-eb2b-415f-bf1b-69900ad2a26b")
            },
            new()
            {
                Id = Guid.Parse("97d1485e-28fc-4f8b-8947-cf06d35e69c9"),
                ValidSince = DateTimeOffset.Parse("2023-01-01 +0:00"),
                ValidUntil = DateTimeOffset.Parse("2023-12-31 +0:00"),
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = fakeLimit.LeaveTypeId
            },
            expectedLimitToClose,
            new()
            {
                Id = Guid.Parse("c547b341-2e34-4b35-b4db-ac4297cd14a0"),
                ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
                ValidUntil = null,
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = Guid.Parse("242a5183-00a1-4244-8ca2-d69d259ccfa8")
            },
            new()
            {
                Id = Guid.Parse("36c80419-fc48-496f-aa8c-cbf023b6e4b6"),
                ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
                ValidUntil = null,
                AssignedToUserId = "9b16f8b5-fcd8-43b9-87f3-5826782eaf60",
                LeaveTypeId = Guid.Parse("242a5183-00a1-4244-8ca2-d69d259ccfa8")
            }
        }.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(m => m.UserLeaveLimits).Returns(dbSetMock.Object);
        var sut = new NeighbouringLimitsService(dbContextMock.Object);
        await sut.CloseNeighbourLimitsPeriodsAsync(fakeLimit);
        expectedLimitToClose.Should().BeEquivalentTo(
            new
            {
                Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
                ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
                ValidUntil = DateTimeOffset.Parse("2024-01-11 +0:00"),
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = fakeLimit.LeaveTypeId
            });
        dbContextMock.Verify(m => m.Update(It.Is<UserLeaveLimit>(ull => ull.Id == expectedLimitToClose.Id)), Times.Once);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
        [Fact]
    public async Task WhenValidUntilHasValue_ThenCloseNextPeriod()
    {
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var fakeLimit = new UserLeaveLimit
        {   
            Id = Guid.Parse("05db1d8e-cec7-4b13-bd2a-48a1deffbc49"),
            AssignedToUserId = "eb49a7a5-2c5c-4b80-99c2-de3d2a3823fc",
            LeaveTypeId = Guid.Parse("a266f646-70f3-4f10-a07c-65726aae8a76"),
            ValidSince = null,
            ValidUntil = DateTimeOffset.Parse("2024-01-12 +0:00")
        };
        var expectedLimitToClose = new UserLeaveLimit()
        {
            Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
            ValidSince = null,
            ValidUntil = DateTimeOffset.Parse("2024-03-16 +0:00"),
            AssignedToUserId = fakeLimit.AssignedToUserId,
            LeaveTypeId = fakeLimit.LeaveTypeId
        };
        var dbSetMock = new UserLeaveLimit[]
        {
            new()
            {
               Id = Guid.Parse("9c79c0c0-b780-43f2-b824-c8ec242ac24d"),
               ValidSince = DateTimeOffset.Parse("2022-01-01 +0:00"),
               ValidUntil = DateTimeOffset.Parse("2022-12-31 +0:00"),
               AssignedToUserId = fakeLimit.AssignedToUserId,
               LeaveTypeId = Guid.Parse("3a9f6616-eb2b-415f-bf1b-69900ad2a26b")
            },
            new()
            {
                Id = Guid.Parse("97d1485e-28fc-4f8b-8947-cf06d35e69c9"),
                ValidSince = DateTimeOffset.Parse("2023-01-01 +0:00"),
                ValidUntil = DateTimeOffset.Parse("2023-12-31 +0:00"),
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = fakeLimit.LeaveTypeId
            },
            expectedLimitToClose,
            new()
            {
                Id = Guid.Parse("c547b341-2e34-4b35-b4db-ac4297cd14a0"),
                ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
                ValidUntil = null,
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = Guid.Parse("242a5183-00a1-4244-8ca2-d69d259ccfa8")
            },
            new()
            {
                Id = Guid.Parse("36c80419-fc48-496f-aa8c-cbf023b6e4b6"),
                ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
                ValidUntil = null,
                AssignedToUserId = "9b16f8b5-fcd8-43b9-87f3-5826782eaf60",
                LeaveTypeId = Guid.Parse("242a5183-00a1-4244-8ca2-d69d259ccfa8")
            }
        }.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(m => m.UserLeaveLimits).Returns(dbSetMock.Object);
        var sut = new NeighbouringLimitsService(dbContextMock.Object);
        await sut.CloseNeighbourLimitsPeriodsAsync(fakeLimit);
        expectedLimitToClose.Should().BeEquivalentTo(
            new
            {
                Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
                ValidSince = DateTimeOffset.Parse("2024-01-13 +0:00"),
                ValidUntil = DateTimeOffset.Parse("2024-03-16 +0:00"),
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = fakeLimit.LeaveTypeId
            });
        dbContextMock.Verify(m => m.Update(It.Is<UserLeaveLimit>(ull => ull.Id == expectedLimitToClose.Id)), Times.Once);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task WhenValidSinceAndValidUntilHasValue_ThenClosePreviousAndNextPeriod()
    {
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var fakeLimit = new UserLeaveLimit
        {   
            Id = Guid.Parse("05db1d8e-cec7-4b13-bd2a-48a1deffbc49"),
            AssignedToUserId = "eb49a7a5-2c5c-4b80-99c2-de3d2a3823fc",
            LeaveTypeId = Guid.Parse("a266f646-70f3-4f10-a07c-65726aae8a76"),
            ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
            ValidUntil = DateTimeOffset.Parse("2024-01-12 +0:00")
        };
        var expectedNextLimitToClose = new UserLeaveLimit()
        {
            Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
            ValidSince = null,
            ValidUntil = DateTimeOffset.Parse("2024-03-16 +0:00"),
            AssignedToUserId = fakeLimit.AssignedToUserId,
            LeaveTypeId = fakeLimit.LeaveTypeId
        };
        var expectedPreviousLimitToClose = new UserLeaveLimit()
        {
            Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
            ValidSince = DateTimeOffset.Parse("2023-11-03 +0:00"),
            ValidUntil = null,
            AssignedToUserId = fakeLimit.AssignedToUserId,
            LeaveTypeId = fakeLimit.LeaveTypeId
        };
        var dbSetMock = new UserLeaveLimit[]
        {
            new()
            {
               Id = Guid.Parse("9c79c0c0-b780-43f2-b824-c8ec242ac24d"),
               ValidSince = DateTimeOffset.Parse("2022-01-01 +0:00"),
               ValidUntil = DateTimeOffset.Parse("2022-12-31 +0:00"),
               AssignedToUserId = fakeLimit.AssignedToUserId,
               LeaveTypeId = Guid.Parse("3a9f6616-eb2b-415f-bf1b-69900ad2a26b")
            },
            new()
            {
                Id = Guid.Parse("97d1485e-28fc-4f8b-8947-cf06d35e69c9"),
                ValidSince = DateTimeOffset.Parse("2023-01-01 +0:00"),
                ValidUntil = DateTimeOffset.Parse("2023-12-31 +0:00"),
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = fakeLimit.LeaveTypeId
            },
            expectedNextLimitToClose,
            new()
            {
                Id = Guid.Parse("c547b341-2e34-4b35-b4db-ac4297cd14a0"),
                ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
                ValidUntil = null,
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = Guid.Parse("242a5183-00a1-4244-8ca2-d69d259ccfa8")
            },
            new()
            {
                Id = Guid.Parse("36c80419-fc48-496f-aa8c-cbf023b6e4b6"),
                ValidSince = DateTimeOffset.Parse("2024-01-01 +0:00"),
                ValidUntil = null,
                AssignedToUserId = "9b16f8b5-fcd8-43b9-87f3-5826782eaf60",
                LeaveTypeId = Guid.Parse("242a5183-00a1-4244-8ca2-d69d259ccfa8")
            },
            expectedPreviousLimitToClose
        }.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(m => m.UserLeaveLimits).Returns(dbSetMock.Object);
        var sut = new NeighbouringLimitsService(dbContextMock.Object);
        await sut.CloseNeighbourLimitsPeriodsAsync(fakeLimit);
        expectedNextLimitToClose.Should().BeEquivalentTo(
            new
            {
                Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
                ValidSince = DateTimeOffset.Parse("2024-01-13 +0:00"),
                ValidUntil = DateTimeOffset.Parse("2024-03-16 +0:00"),
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = fakeLimit.LeaveTypeId
            });
        expectedPreviousLimitToClose.Should().BeEquivalentTo(
            new
            {
                Id = Guid.Parse("3b594687-b071-4c1e-94f5-5c73cd13988f"),
                ValidSince = DateTimeOffset.Parse("2023-11-03 +0:00"),
                ValidUntil = DateTimeOffset.Parse("2023-12-31 +0:00"),
                AssignedToUserId = fakeLimit.AssignedToUserId,
                LeaveTypeId = fakeLimit.LeaveTypeId
            });
        dbContextMock.Verify(m => m.Update(It.Is<UserLeaveLimit>(ull => ull.Id == expectedNextLimitToClose.Id)), Times.Exactly(2));
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}