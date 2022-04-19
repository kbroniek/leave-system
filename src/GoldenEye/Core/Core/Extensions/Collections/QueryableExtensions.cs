using System;
using System.Linq;

namespace GoldenEye.Extensions.Collections;

public static class QueryableExtensions
{
    public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int? page, int? pageSize)
    {
        if (page.HasValue && page < 0) throw new ArgumentException("Page number should be greater than or equal to 0");

        if (pageSize.HasValue && pageSize.Value < 0)
            throw new ArgumentException("Page size should be greater than or equal to 0");

        int pageOrDefault = page ?? 0;
        return pageSize.HasValue ? source.Skip((pageOrDefault - 1) * pageSize.Value).Take(pageSize.Value) : source;
    }
}