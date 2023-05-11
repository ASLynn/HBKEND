using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace HeyDoc.Web.Helpers
{
    public static class QueryDynamicOrdering
    {
        public static IOrderedQueryable<TSource> DynamicOrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool descendingOrder = false, bool primarySortKey = true)
        {
            IOrderedQueryable<TSource> resultQuery;

            if (primarySortKey)
            {
                if (descendingOrder)
                {
                    resultQuery = source.OrderByDescending(keySelector);
                }
                else
                {
                    resultQuery = source.OrderBy(keySelector);
                }
            }
            else // If primarySortKey is false but this query has no primary sort key, it will cause an exception
            {
                var orderedSource = (IOrderedQueryable<TSource>)source;
                if (descendingOrder)
                {
                    resultQuery = orderedSource.ThenByDescending(keySelector);
                }
                else
                {
                    resultQuery = orderedSource.ThenBy(keySelector);
                }
            }

            return resultQuery;
        }
    }
}