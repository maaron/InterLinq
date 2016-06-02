using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Reflection;
using InterLinq.Types;
using InterLinq.Expressions;

namespace InterLinq.Communication
{
    /// <summary>
    /// Server implementation of the <see cref="IQueryRemoteHandler"/>.
    /// </summary>
    /// <seealso cref="IQueryRemoteHandler"/>
    public class ServerQueryHandler : IQueryRemoteHandler, IDisposable
    {

        #region Properties

        /// <summary>
        /// Gets the <see cref="IQueryHandler"/>.
        /// </summary>
        public IQueryHandler QueryHandler { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="queryHandler"><see cref="IQueryHandler"/> instance.</param>
        public ServerQueryHandler(IQueryHandler queryHandler)
        {
            if (queryHandler == null)
            {
                throw new ArgumentNullException("queryHandler");
            }
            QueryHandler = queryHandler;
        }

        #endregion

        #region IQueryRemoteHandler Members

        /// <summary>
        /// Retrieves data from the server by an <see cref="SerializableExpression">Expression</see> tree.
        /// </summary>
        /// <remarks>
        /// This method's return type depends on the submitted 
        /// <see cref="SerializableExpression">Expression</see> tree.
        /// Here some examples ('T' is the requested type):
        /// <list type="list">
        ///     <listheader>
        ///         <term>Method</term>
        ///         <description>Return Type</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Select(...)</term>
        ///         <description>T[]</description>
        ///     </item>
        ///     <item>
        ///         <term>First(...), Last(...)</term>
        ///         <description>T</description>
        ///     </item>
        ///     <item>
        ///         <term>Count(...)</term>
        ///         <description><see langword="int"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Contains(...)</term>
        ///         <description><see langword="bool"/></description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="expression">
        ///     <see cref="SerializableExpression">Expression</see> tree 
        ///     containing selection and projection.
        /// </param>
        /// <returns>Returns requested data.</returns>
        /// <seealso cref="IQueryRemoteHandler.Retrieve"/>
        public Message Retrieve(ExpressionMessage message)
        {
            try
            {
                var expression = message.Expression;
#if DEBUG
                Console.WriteLine(expression);
                Console.WriteLine();
#endif

                object returnValue = null;
                MethodInfo mInfo;
                Type realType = (Type)expression.Type.GetClrVersion();
                if (typeof(IQueryable).IsAssignableFrom(realType) &&
                    realType.GetGenericArguments().Length == 1)
                {
                    // The client is asking for an IQueryable<>, so start 
                    // serializing results as soon as we get them from the source.
                    QueryHandler.StartSession();
                    try
                    {
                        // Find Generic Retrieve Method
                        mInfo = GetType().GetMethod("RetrieveGeneric");
                        mInfo = mInfo.MakeGenericMethod(realType.GetGenericArguments()[0]);

                        returnValue = mInfo.Invoke(this, new object[] { expression });

                        return System.ServiceModel.Channels.Message.CreateMessage(
                            System.ServiceModel.OperationContext.Current.IncomingMessageVersion,
                            "http://tempuri.org/IQueryRemoteHandler/RetrieveResponse",
                            new QueryableBodyWriter(returnValue, QueryHandler));
                    }
                    catch (Exception)
                    {
                        // Only close the session if there was an exception, 
                        // otherwise the QueryableBodyWriter will do it once 
                        // the query results are completely written to the 
                        // stream.
                        QueryHandler.CloseSession();
                        throw;
                    }
                }
                else
                {
                    // The client is asking for some other type.  In this 
                    // case, we serialize the object in the message in a 
                    // single shot.
                    QueryHandler.StartSession();
                    try
                    {
                        // Find Non-Generic Retrieve Method
                        mInfo = GetType().GetMethod("RetrieveNonGenericObject");

                        returnValue = mInfo.Invoke(this, new object[] { expression });

                        return System.ServiceModel.Channels.Message.CreateMessage(
                            System.ServiceModel.OperationContext.Current.IncomingMessageVersion,
                            "http://tempuri.org/IQueryRemoteHandler/RetrieveResponse",
                            returnValue,
                            new System.Runtime.Serialization.NetDataContractSerializer());
                    }
                    finally
                    {
                        // In this case, always ensure the session is closed.
                        QueryHandler.CloseSession();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleExceptionInRetrieve(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves data from the server by an <see cref="SerializableExpression">Expression</see> tree.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IQueryable"/>.</typeparam>
        /// <param name="serializableExpression">
        ///     <see cref="SerializableExpression">Expression</see> tree 
        ///     containing selection and projection.
        /// </param>
        /// <returns>Returns requested data.</returns>
        /// <seealso cref="IQueryRemoteHandler.Retrieve"/>
        /// <remarks>
        /// This method's return type depends on the submitted 
        /// <see cref="SerializableExpression">Expression</see> tree.
        /// Here some examples ('T' is the requested type):
        /// <list type="list">
        ///     <listheader>
        ///         <term>Method</term>
        ///         <description>Return Type</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Select(...)</term>
        ///         <description>T[]</description>
        ///     </item>
        ///     <item>
        ///         <term>First(...), Last(...)</term>
        ///         <description>T</description>
        ///     </item>
        ///     <item>
        ///         <term>Count(...)</term>
        ///         <description><see langword="int"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Contains(...)</term>
        ///         <description><see langword="bool"/></description>
        ///     </item>
        /// </list>
        /// </remarks>
        public object RetrieveGeneric<T>(SerializableExpression serializableExpression)
        {
#if false
            try
            {
                QueryHandler.StartSession();
                IQueryable<T> query = serializableExpression.Convert(QueryHandler) as IQueryable<T>;
                var returnValue = query.ToArray();
                object convertedReturnValue = TypeConverter.ConvertToSerializable(returnValue);
                return convertedReturnValue;
            }
            finally
            {
                QueryHandler.CloseSession();
            }
#else
            return serializableExpression.Convert(QueryHandler);
#endif
        }

        /// <summary>
        /// Retrieves data from the server by an <see cref="SerializableExpression">Expression</see> tree.
        /// </summary>
        /// <param name="serializableExpression">
        ///     <see cref="SerializableExpression">Expression</see> tree 
        ///     containing selection and projection.
        /// </param>
        /// <returns>Returns requested data.</returns>
        /// <remarks>
        /// This method's return type depends on the submitted 
        /// <see cref="SerializableExpression">Expression</see> tree.
        /// Here some examples ('T' is the requested type):
        /// <list type="list">
        ///     <listheader>
        ///         <term>Method</term>
        ///         <description>Return Type</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Select(...)</term>
        ///         <description>T[]</description>
        ///     </item>
        ///     <item>
        ///         <term>First(...), Last(...)</term>
        ///         <description>T</description>
        ///     </item>
        ///     <item>
        ///         <term>Count(...)</term>
        ///         <description><see langword="int"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Contains(...)</term>
        ///         <description><see langword="bool"/></description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="IQueryRemoteHandler.Retrieve"/>
        public object RetrieveNonGenericObject(SerializableExpression serializableExpression)
        {
            try
            {
                QueryHandler.StartSession();
                object returnValue = serializableExpression.Convert(QueryHandler);
                object convertedReturnValue = TypeConverter.ConvertToSerializable(returnValue);
                return convertedReturnValue;
            }
            finally
            {
                QueryHandler.CloseSession();
            }
        }

        /// <summary>
        /// Handles an <see cref="Exception"/> occured in the 
        /// <see cref="IQueryRemoteHandler.Retrieve"/> Method.
        /// </summary>
        /// <param name="exception">
        /// Thrown <see cref="Exception"/> 
        /// in <see cref="IQueryRemoteHandler.Retrieve"/> Method.
        /// </param>
        protected virtual void HandleExceptionInRetrieve(Exception exception)
        {
            throw exception;
        }

#endregion

#region IDisposable Members

        /// <summary>
        /// Disposes the server instance.
        /// </summary>
        public virtual void Dispose() { }

#endregion
    }
}
