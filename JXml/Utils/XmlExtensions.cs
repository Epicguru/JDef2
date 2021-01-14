using System;
using System.Xml;

namespace JXml.Utils
{
    public static class XmlExtensions
    {
        private static void ReportAttributeError(XmlNode node, string attrName, string value, string expectedType)
        {
            XmlController.Log($"Attribute error: Node {node.Name} has attribute {attrName}=\"{value}\", but it should be {expectedType}.");
        }

        public static string TryGetAttribute(this XmlNode node, string attributeName)
        {
            if (node == null)
                return null;
            if (attributeName == null)
                return null;

            return node.Attributes?.GetNamedItem(attributeName)?.Value;
        }

        public static bool? TryParseAttributeBool(this XmlNode node, string attributeName)
        {
            string raw = node.TryGetAttribute(attributeName);
            if (raw == null)
                return null;

            if (bool.TryParse(raw, out var value))
                return value;

            ReportAttributeError(node, attributeName, raw, "true or false");
            return null;
        }

        public static int? TryParseAttributeInt(this XmlNode node, string attributeName)
        {
            string raw = node.TryGetAttribute(attributeName);
            if (raw == null)
                return null;

            if (int.TryParse(raw, out var value))
                return value;

            ReportAttributeError(node, attributeName, raw, "an integer");
            return null;
        }

        public static T? TryParseAttributeEnum<T>(this XmlNode node, string attributeName) where T : struct
        {
            string raw = node.TryGetAttribute(attributeName);
            if (raw == null)
                return null;

            raw = raw.Trim();

            Type enumType = typeof(T);

            if (Enum.TryParse(raw, true, out T res))
            {
                // TryParse will return true even for out of range integers (in string form, such as "1234").
                // So check that the returned value is in fact a real enum value.
                if (Enum.IsDefined(enumType, res.ToString()))
                {
                    return res;
                }
            }

            if (long.TryParse(raw, out long result))
            {
                var targetType = enumType.GetEnumUnderlyingType();
                object finalValue = result;
                if(targetType != typeof(long))
                {
                    try
                    {
                        finalValue = Convert.ChangeType(result, targetType);
                    }
                    catch
                    {
                        ReportAttributeError(node, attributeName, raw, $"a valid enum name or index. {finalValue} is not a valid index (index type is {targetType.Name})");
                        return null;
                    }
                }

                if (Enum.IsDefined(enumType, finalValue))
                {
                    return (T)Enum.ToObject(enumType, finalValue);
                }
                else
                {
                    ReportAttributeError(node, attributeName, raw, $"a valid enum name or index. {finalValue} is not a valid index");
                    return null;
                }
            }
            

            ReportAttributeError(node, attributeName, raw, "a valid enum name or index");
            return null;
        }

        public static bool IsNullable(this Type type)
        {
            if (!type.IsValueType)
                return true; // Structs, primitives...
            if (Nullable.GetUnderlyingType(type) != null)
                return true; // Nullable<T>

            return false; // value-type
        }

        public static bool IsRealNullable(this Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
                return true; // Nullable<T>
            return false;
        }

        public static string GetXPath(this XmlNode node)
        {
            if (node == null)
                return null;

            string path = "/" + node.Name;

            if (node.NodeType == XmlNodeType.Text)
            {
                var old = node;
                node = node.ParentNode;
                path = "/" + node.Name;
                path += "/" + old.Name;
            }

            if (node.ParentNode is XmlNode parentNode)
            {
                // Gets the position within the parent element.
                // However, this position is irrelevant if the element is unique under its parent:
                XmlNodeList siblings = parentNode.SelectNodes(node.Name);
                if (siblings != null && siblings.Count > 1) // There's more than 1 element with the same name
                {
                    int position = 0;
                    foreach (XmlElement sibling in siblings)
                    {
                        if (sibling == node)
                            break;

                        position++;
                    }

                    path = path + "[" + position + "]";
                }

                // Climbing up to the parent elements:
                if(parentNode.NodeType == XmlNodeType.Element)
                    path = parentNode.GetXPath() + path;
            }

            if (path.StartsWith("/"))
                path = path.Substring(1);
            return path;
        }

        public static XmlNode FirstContentNode(this XmlNode node)
        {
            if (node == null)
                return null;

            var children = node.ChildNodes;
            if (children.Count == 0)
                return null;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children.Item(i);
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Text)
                    return child;
            }

            return null;
        }
    }
}
