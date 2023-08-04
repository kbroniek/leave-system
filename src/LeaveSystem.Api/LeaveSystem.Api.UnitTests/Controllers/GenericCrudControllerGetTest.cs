// using FluentAssertions;
// using GoldenEye.Objects.General;
// using LeaveSystem.Api.Controllers;
// using LeaveSystem.Db;
// using LeaveSystem.UnitTests;
//
// namespace LeaveSystem.Api.UnitTests.Controllers;
//
// public abstract class GenericCrudControllerGetTest<TEntity, TId>
//     where TId : IComparable<TId>
//     where TEntity : class, IHaveId<TId>
// {
//     protected abstract GenericCrudController<TEntity, TId> GetSut(LeaveSystemDbContext dbContext);
//
//     [Fact]
//     public async Task WhenGetSet_ThenReturnSet()
//     {
//         await using var dbContext = await DbContextFactory.CreateDbContext();
//         var sut = GetSut(dbContext);
//         var set = sut.Get();
//         set.GetType().Should().BeOfType(typeof(TEntity));
//     }
// }