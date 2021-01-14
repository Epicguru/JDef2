using System.Xml;

namespace JXml
{
    public interface IXmlAware
    {
        void OnCreateFromNode(XmlNode node);
    }
}
