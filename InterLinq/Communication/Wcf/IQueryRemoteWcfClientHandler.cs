﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using InterLinq.Expressions;
using InterLinq.Communication.Wcf.NetDataContractSerializer;

namespace InterLinq.Communication.Wcf
{
    [ServiceContract]
    public interface IQueryRemoteWcfClientHandler : IQueryRemoteHandler
    {
        [OperationContract(Action = "http://tempuri.org/IQueryRemoteHandler/Retrieve", ReplyAction = "http://tempuri.org/IQueryRemoteHandler/RetrieveResponse")]
        [NetDataContractFormat]
        System.Threading.Tasks.Task<object> RetrieveAsync(SerializableExpression expression);
    }

    public partial class WcfClientQueryRemoteHandler 
        : ClientBase<IQueryRemoteWcfClientHandler>, IQueryRemoteWcfClientHandler
    {

        public WcfClientQueryRemoteHandler()
        {
        }

        public WcfClientQueryRemoteHandler(string endpointConfigurationName) :
                base(endpointConfigurationName)
        {
        }

        public WcfClientQueryRemoteHandler(string endpointConfigurationName, string remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public WcfClientQueryRemoteHandler(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public WcfClientQueryRemoteHandler(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }

        public object Retrieve(SerializableExpression expression)
        {
            return base.Channel.Retrieve(expression);
        }

        public System.Threading.Tasks.Task<object> RetrieveAsync(SerializableExpression expression)
        {
            return base.Channel.RetrieveAsync(expression);
        }
    }
}
