using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterLinq.Communication;

namespace InterLinq
{
    /// <summary>
    /// Class containing extension methods for IQueryable's.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Executes the provided query asynchronously.  If the IQueryable's 
        /// provider implements IAsyncQueryProvider, 
        /// IAsyncQueryProvider.ExecuteAsync will be called.  Otherwise, a new 
        /// task will be created to run the query using Task.Run().
        /// </summary>
        /// <typeparam name="TResult">The result type associated with the source</typeparam>
        /// <param name="source">The queryable source</param>
        /// <returns>
        /// A Task&lt;IEnumerable&lt;TResult&gt;&gt; that can be used to 
        /// retrieve the query result.
        /// </returns>
        public static Task<IEnumerable<TResult>> ExecuteAsync<TResult>(this IQueryable<TResult> source)
        {
            if (source.Provider is IAsyncQueryProvider)
                return ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(source.Expression);
            else
                return Task.Run(() => (IEnumerable<TResult>)source.ToList());
        }
    }
}
