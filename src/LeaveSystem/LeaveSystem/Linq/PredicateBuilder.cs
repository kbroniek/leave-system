﻿using System.Linq.Expressions;

namespace LeaveSystem.Linq;
public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>() { return _ => true; }
    public static Expression<Func<T, bool>> False<T>() { return _ => false; }
    public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> func) => func;

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                        Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>
              (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }
    public static Expression<Func<T1, T2, bool>> Or<T1, T2>(this Expression<Func<T1, T2, bool>> expr1,
                                                        Expression<Func<T1, T2, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T1, T2, bool>>
              (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                         Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>
              (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }
    
    public static Expression<Func<T, bool>> MatchAny<T, TValue>(
        Expression<Func<T,TValue>> searchingExpression , 
        TValue[]? values) where T : class where TValue : IComparable
    {
        if (values is null or {Length: < 1})
        {
            return True<T>();
        }
        var predicate = False<T>();
        var param = Expression.Parameter(typeof(T));
        return values.Aggregate(predicate, (current, value) =>
        {
            var constValue = Expression.Constant(value);
            var equal = Expression.Equal(Expression.Invoke(searchingExpression, param), constValue);
            return current.Or(Expression.Lambda<Func<T, bool>>(equal, param));
        });
    }
}