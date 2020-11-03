using System;
using System.Xml;

namespace JXml
{
    public interface IRootTypeSerializer
    {
        Type TargetType { get; }

        object Deserialize(XmlNode node);

        string Serialize(object o);
    }
}
