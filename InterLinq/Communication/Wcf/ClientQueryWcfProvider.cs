using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using InterLinq.Expressions;

namespace InterLinq.Communication.Wcf
{
    public class ClientQueryWcfProvider : ClientQueryProvider, IAsyncQueryProvider
    {
        public ClientQueryWcfProvider(IQueryRemoteWcfClientHandler queryRemoteHandler)
            : base(queryRemoteHandler)
        {
        }

        public Task<IEnumerable<object>> ExecuteAsync(Expression expression)
        {
            return ((IQueryRemoteWcfClientHandler)Handler).RetrieveAsync(new ExpressionMessage()
            {
                Expression = expression.MakeSerializable()
            })
            .ContinueWith((Task<System.ServiceModel.Channels.Message> task) => (IEnumerable<object>)task.Result);
        }

        public Task<IEnumerable<TResult>> ExecuteAsync<TResult>(Expression expression)
        {
            return ExecuteAsync(expression).ContinueWith(task =>
                task.Result.Cast<TResult>());
        }
    }
}
