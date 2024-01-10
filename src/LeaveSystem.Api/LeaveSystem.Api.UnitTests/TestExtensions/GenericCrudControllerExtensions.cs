using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;

namespace LeaveSystem.Api.UnitTests.TestExtensions;

public static class GenericCrudControllerExtensions
{
    public static void CheckGetMethod<TEntity, TId>(this GenericCrudController<TEntity, TId> source, LeaveSystemDbContext dbContext)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        //When
        var set = source.Get();
        //Then
        set.Should().BeEquivalentTo(dbContext.Set<LeaveType>());
    }
}
