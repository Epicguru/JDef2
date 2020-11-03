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

        public bool IsArray;
        public bool IsList;
        public bool IsDictionary;
    }
}
