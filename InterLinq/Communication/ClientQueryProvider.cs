using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Runtime.Serialization;
using InterLinq.Expressions;
using InterLinq.Types;
using System.Threading.Tasks;

namespace InterLinq.Communication
{
    /// <summary>
    /// Client implementation of the <see cref="InterLinqQueryProvider"/>.
    /// </summary>
    /// <seealso cref="InterLinqQueryProvider"/>
    /// <seealso cref="IQueryProvider"/>
    public class ClientQueryProvider : InterLinqQueryProvider
    {

        #region Property Handler

        /// <summary>
        /// Gets the <see cref="IQueryRemoteHandler"/>;
        /// </summary>
        public IQueryRemoteHandler Handler { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="queryRemoteHandler"><see cref="IQueryRemoteHandler"/> to communicate with the server.</param>
        public ClientQueryProvider(IQueryRemoteHandler queryRemoteHandler)
        {
            if (queryRemoteHandler == null)
            {
                throw new ArgumentNullException("queryRemoteHandler");
            }
            Handler = queryRemoteHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the query and returns the requested data.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value.</typeparam>
        /// <param name="expression"><see cref="Expression"/> tree to execute.</param>
        /// <returns>Returns the requested data of Type 'TResult'.</returns>
        /// <seealso cref="InterLinqQueryProvider.Execute"/>
        public override TResult Execute<TResult>(Expression expression)
        {
            var stream = (Stream)Execute(expression);

            // If we are expecting an IEnumerable<>, return an IEnumerable 
            // that enumerates objects as they are received in the stream.  
            // Otherwise, the stream should only contain a single object and 
            // we return that.
            var elementType = GetIEnumerableElementType(typeof(TResult));

            if (elementType != null)
            {
                var method = GetType().GetMethod("EnumerateStream").MakeGenericMethod(new[] { elementType });
                var result = method.Invoke(this, new object[] { elementType, stream });
                return (TResult)result;
            }
            else
            {
                var serializer = new NetDataContractSerializer();

                using (stream)
                {
                    return (TResult)TypeConverter.ConvertFromSerializable(
                        typeof(TResult),
                        serializer.Deserialize(stream));
                }
            }
        }

        private static System.Reflection.MethodInfo MethodOf<T1, T2, TResult>(Func<T1, T2, TResult> f, T1 t1, T2 t2)
        {
            return f.Method;
        }

        private static bool FilterPredicate(Type t, object o)
        {
            return t.IsGenericType && 
                t.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static Type GetIEnumerableElementType(Type type)
        {
            return type.FindInterfaces(FilterPredicate, null).FirstOrDefault();
        }

        private IEnumerable<T> EnumerateStream<T>(Type elementType, Stream stream)
        {
            var serializer = new NetDataContractSerializer();

            using (stream)
            {
                while (stream.CanRead)
                    yield return (T)TypeConverter.ConvertFromSerializable(
                        typeof(T), 
                        serializer.Deserialize(stream));
            }
        }

        /// <summary>
        /// Executes the query and returns the requested data.
        /// </summary>
        /// <param name="expression"><see cref="Expression"/> tree to execute.</param>
        /// <returns>Returns the requested data of Type <see langword="object"/>.</returns>
        /// <seealso cref="InterLinqQueryProvider.Execute"/>
        public override object Execute(Expression expression)
        {
            SerializableExpression serExp = expression.MakeSerializable();
            object receivedObject = Handler.Retrieve(serExp);
            return receivedObject;
        }

        #endregion

    }
}
