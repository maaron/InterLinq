using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace InterLinq.Communication
{
    public interface IAsyncQueryProvider
    {
        Task<IEnumerable<TResult>> ExecuteAsync<TResult>(Expression expression);

        Task<IEnumerable<object>> ExecuteAsync(Expression expression);
    }
}
