using FluentAssertions;
using GoldenEye.Objects.General;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;

namespace LeaveSystem.Api.UnitTests.TestExtensions;

public static class GenericCrudControllerExtensions
{
    public static async Task CheckGetMethodAsync<TEntity,TId>(this GenericCrudController<TEntity, TId> source, LeaveSystemDbContext dbContext)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        //When
        var set = source.Get();
        //Then
        set.Should().BeEquivalentTo(dbContext.Set<LeaveType>());
    }
    
}