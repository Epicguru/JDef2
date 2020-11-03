using System.Xml;

namespace JXml
{
    public struct CustomResolverArgs
    {
        public object RootObject;
        public object ExistingObject;
        public object ParentObject;
        public FieldWrapper Field;
        public XmlNode XmlNode;
    }
}
