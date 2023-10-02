using System.Linq.Expressions;
using LamarCodeGeneration.Util;
using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Linq;
using LeaveSystem.Periods;
using LeaveSystem.Shared.WorkingHours;

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
    
    public static IQueryable<WorkingHours> WhereMatchAnyStatus(this IQueryable<WorkingHours> source, WorkingHoursStatus[]? statuses, DateTimeOffset currentDate) 
    {
        if (statuses is null or {Length: < 1})
        {
            return source;
        }
        var predicate = PredicateBuilder.False<WorkingHours>();
        predicate = statuses.Aggregate(predicate, (current, status) =>
        {
            var expressionForStatus = WorkingHoursExpression.GetExpressionForStatus(status, currentDate);
            return current.Or(expressionForStatus);
        });
        return source.Where(predicate);
    }

}