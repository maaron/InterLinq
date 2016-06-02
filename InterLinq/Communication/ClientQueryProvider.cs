using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using InterLinq.Expressions;
using InterLinq.Types;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
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
            return (TResult)Execute(expression);
        }

        private static bool FilterPredicate(Type t, object o)
        {
            return t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IQueryable<>);
        }

        private static bool IsIQueryable(Type type)
        {
            return type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IQueryable<>);
        }

        private static Type GetIQueryableElementType(Type type)
        {
            return IsIQueryable(type) ? type.GetGenericArguments()[0]
                : type.FindInterfaces(FilterPredicate, null)
                    .Select(i => i.GetGenericArguments().First())
                    .FirstOrDefault();
        }

        private IEnumerable<T> EnumerateMessage<T>(Type elementType, System.ServiceModel.Channels.Message message)
        {
            using (message)
            {
                var serializer = new NetDataContractSerializer();

                if (!message.IsEmpty)
                {
                    var reader = message.GetReaderAtBodyContents();

                    while (!reader.EOF && reader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        yield return (T)TypeConverter.ConvertFromSerializable(
                            typeof(T),
                            serializer.ReadObject(reader));
                    }
                }
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

            var message = Handler.Retrieve(new ExpressionMessage() { Expression = serExp });

            if (message.IsFault)
            {
                using (message)
                {
                    throw new System.ServiceModel.FaultException(
                        MessageFault.CreateFault(message, int.MaxValue));
                }
            }

            // If we are expecting an IEnumerable<>, return an IEnumerable 
            // that enumerates objects as they are received in the stream.  
            // Otherwise, the stream should only contain a single object and 
            // we return that.
            var elementType = GetIQueryableElementType(expression.Type);
            if (elementType != null)
            {
                var method = typeof(ClientQueryProvider).GetMethod("EnumerateMessage", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)
                        .MakeGenericMethod(new[] { elementType });

                var result = method.Invoke(this, new object[] { elementType, message });
                return result;
            }
            else
            {
                var serializer = new NetDataContractSerializer();

                var reader = message.GetReaderAtBodyContents();

                using (message)
                {
                    return TypeConverter.ConvertFromSerializable(
                        expression.Type,
                        serializer.ReadObject(reader));
                }
            }
        }

        #endregion
    }
}
