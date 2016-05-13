using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
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
            var enumerable = Object is IEnumerable ?
                (IEnumerable)Object
                : new[] { Object };

            var serializer = new NetDataContractSerializer();

            foreach (var element in enumerable)
            {
                serializer.WriteObject(writer, element);
            }
        }
    }
}
