using System.Linq.Expressions;
using LeaveSystem.Linq;
using Marten;

namespace LeaveSystem.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereMatchAny<T, TValue>(this IQueryable<T> source, Expression<Func<T,TValue>> expression, TValue[]? values) where T : class where TValue : IComparable
    {
        if (values is null or {Length: < 1})
        {
            return source;
        }
        var predicate = PredicateBuilder.False<T>();
        var param = Expression.Parameter(typeof(T));
        predicate = values.Aggregate(predicate, (current, value) =>
        {
            var constValue = Expression.Constant(value);
            var equal = Expression.Equal(Expression.Invoke(expression, param), constValue);
            return current.Or(Expression.Lambda<Func<T, bool>>(equal, param));
        });
        return source.Where(predicate);
    }
}