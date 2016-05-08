using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterLinq.Communication;

namespace InterLinq
{
    public static class IQueryableExtensions
    {
        public static Task<IEnumerable<TResult>> ExecuteAsync<TResult>(this IQueryable<TResult> source)
        {
            if (source.Provider is IAsyncQueryProvider)
                return ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(source.Expression);
            else
                return Task.Run(() => (IEnumerable<TResult>)source.ToList());
        }
    }
}
