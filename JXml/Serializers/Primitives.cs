using Microsoft.Xna.Framework;
using System;
using System.Xml;

namespace JXml.Serializers
{
    public abstract class PrimitiveParser : IRootTypeSerializer
    {
        public abstract Type TargetType { get; }
        public abstract object Deserialize(XmlNode node);
        public virtual string Serialize(object o)
        {
            return o?.ToString();
        }

        protected Exception MakeException(string message, Exception innerException = null)
        {
            return new XmlException(message, innerException);
        }
    }

    public class IntParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(int);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (int.TryParse(text, out int value))
                return value;

            throw MakeException($"Failed to parse '{text}' as an int (int32).");
        }
    }

    public class FloatParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(float);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (float.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a float.");
        }
    }

    public class BoolParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(bool);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value.ToLower();

            if (bool.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a bool.");
        }
    }

    public class DoubleParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(double);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (double.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a double.");
        }
    }

    public class ByteParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(byte);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (byte.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a byte.");
        }
    }

    public class ShortParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(short);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (short.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a short (int16).");
        }
    }

    public class LongParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(long);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (long.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a long (int64).");
        }
    }

    public class UShortParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(ushort);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (ushort.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a ushort (uint16).");
        }
    }

    public class ULongParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(ulong);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (ulong.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a ulong (uint64).");
        }
    }

    public class UIntParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(uint);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (uint.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a uint (uint32).");
        }
    }

    public class SByteParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(sbyte);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (sbyte.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as an sbyte.");
        }
    }

    public class StringParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(string);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            return text;
        }
    }

    public class DecimalParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(decimal);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (decimal.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a decimal.");
        }
    }

    public class CharParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(char);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            if (char.TryParse(text, out var value))
                return value;

            throw MakeException($"Failed to parse '{text}' as a char.");
        }
    }

    public class Vector2Parser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(Vector2);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            int index;
            if ((index = text.IndexOf(',')) == -1)
                throw MakeException("Incorrect format: expected (x, y)");

            string start = text.Substring(0, index).Trim();
            string end = text.Substring(index + 1).Trim();

            if (start.StartsWith("("))
                start = start.Substring(1);
            if (end.EndsWith(")"))
                end = end.Substring(0, end.Length - 1);

            if (!float.TryParse(start, out float x))
                throw MakeException($"Failed to parse X value in Vector2: '{start}'");

            if (!float.TryParse(end, out float y))
                throw MakeException($"Failed to parse Y value in Vector2: '{end}'");

            return new Vector2(x, y);
        }
    }
}
