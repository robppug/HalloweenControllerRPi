using System.Xml.Linq;
using System.Xml.Serialization;

namespace HalloweenControllerRPi
{
    internal interface IXmlFunction : IXmlSerializable
    {
        void ReadXML(XElement element);
    }
}