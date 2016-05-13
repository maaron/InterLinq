using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using InterLinq.Expressions;

namespace InterLinq
{
    [MessageContract]
    public struct ExpressionMessage
    {
        [MessageBodyMember]
        public SerializableExpression Expression;
    }
}
