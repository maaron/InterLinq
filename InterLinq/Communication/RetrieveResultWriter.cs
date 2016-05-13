using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.Xml;

namespace InterLinq.Communication
{
    public class RetrieveResultWriter : BodyWriter
    {
        public object Object { get; private set; }

        public RetrieveResultWriter(object o)
            : base(false)
        {
            Object = o;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (Object is IEnumerable)
            {
                var enumerable = (IEnumerable)Object;

                var serializer = new System.Runtime.Serialization.NetDataContractSerializer();
                serializer.WriteObject(writer, enumerable);
            }
        }
    }
}
