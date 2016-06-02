using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
using System.Xml;

using InterLinq.Types;

namespace InterLinq.Communication
{
    public class QueryableBodyWriter : BodyWriter
    {
        public object Object { get; private set; }
        public IQueryHandler QueryHandler { get; private set; }

        public QueryableBodyWriter(object o, IQueryHandler queryHandler)
            : base(false)
        {
            Object = o;
            QueryHandler = queryHandler;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            try
            {
                Console.WriteLine("Result type: " + Object.GetType());

                var enumerable = Object is IEnumerable ?
                    (IEnumerable)Object
                    : new[] { Object };

                var serializer = new NetDataContractSerializer();

                foreach (var element in enumerable)
                {
                    serializer.WriteObject(writer,
                        TypeConverter.ConvertToSerializable(element));
                }
            }
            finally
            {
                QueryHandler.CloseSession();
            }
        }
    }
}
