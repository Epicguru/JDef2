using JXml.Utils;
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

        protected Exception MakeException(XmlNode node, string message, Exception innerException = null)
        {
            if(node != null)
                return new XmlException($"[{node.GetXPath()}] {message}", innerException);
            else
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

            throw MakeException(node, $"Failed to parse '{text}' as an int (int32).");
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

            throw MakeException(node, $"Failed to parse '{text}' as a float.");
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

            throw MakeException(node, $"Failed to parse '{text}' as a bool.");
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

            throw MakeException(node, $"Failed to parse '{text}' as a double.");
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

            throw MakeException(node, $"Failed to parse '{text}' as a byte.");
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

            throw MakeException(node, $"Failed to parse '{text}' as a short (int16).");
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

            throw MakeException(node, $"Failed to parse '{text}' as a long (int64).");
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

            throw MakeException(node, $"Failed to parse '{text}' as a ushort (uint16).");
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

            throw MakeException(node, $"Failed to parse '{text}' as a ulong (uint64).");
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

            throw MakeException(node, $"Failed to parse '{text}' as a uint (uint32).");
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

            throw MakeException(node, $"Failed to parse '{text}' as an sbyte.");
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

            throw MakeException(node, $"Failed to parse '{text}' as a decimal.");
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

            throw MakeException(node, $"Failed to parse '{text}' as a char.");
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
                throw MakeException(node, "Incorrect format: expected (x, y)");

            string start = text.Substring(0, index).Trim();
            string end = text.Substring(index + 1).Trim();

            if (start.StartsWith("("))
                start = start.Substring(1);
            if (end.EndsWith(")"))
                end = end.Substring(0, end.Length - 1);

            if (!float.TryParse(start, out float x))
                throw MakeException(node, $"Failed to parse X value in Vector2: '{start}'");

            if (!float.TryParse(end, out float y))
                throw MakeException(node, $"Failed to parse Y value in Vector2: '{end}'");

            return new Vector2(x, y);
        }
    }

    public class ColorParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(Color);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value?.Trim();

            if (string.IsNullOrEmpty(text))
                throw MakeException(node, "Empty node, expected a color.");

            bool isHex = text[0] == '#';
            if (isHex)
            {
                Color? col = HexToColor(text);
                if(col == null)
                    throw MakeException(node, $"Found hexadecimal color representation, but it was not in the format #RRGGBB or #RRGGBBAA: '{text}'");
                return col.Value;
            }

            string[] parts = text.Split(',');
            if(parts.Length < 3 || parts.Length > 4)
                throw MakeException(node, $"Found RGB(A) color representation, but format was incorrect. Expected (R, G, B) or (R, G, B, A).");

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }
            if (parts[0].StartsWith("("))
                parts[0] = parts[0].Substring(1);
            int last = parts.Length - 1;
            if (parts[last].EndsWith(")"))
                parts[last] = parts[last].Substring(0, parts[last].Length - 1);

            if (!float.TryParse(parts[0], out float r))
                throw MakeException(node, $"Red value has bad format. Expected a float, got '{parts[0]}'");
            if (!float.TryParse(parts[1], out float g))
                throw MakeException(node, $"Green value has bad format. Expected a float, got '{parts[1]}'");
            if (!float.TryParse(parts[2], out float b))
                throw MakeException(node, $"Blue value has bad format. Expected a float, got '{parts[2]}'");
            float a = 1f;
            if(parts.Length == 4)
                if (!float.TryParse(parts[3], out a))
                    throw MakeException(node, $"Alpha value has bad format. Expected a float, got '{parts[3]}'");

            return new Color(r, g, b, a);
        }

        private Color? HexToColor(string str)
        {
            // In the format #RRGGBB(AA) where the AA is optional.
            if (str.Length != 7 && str.Length != 9)
                return null;

            str = str.ToUpper();
            Color color = new Color();

            bool foundA = false;
            for (int i = 0; i < (str.Length == 9 ? 4 : 3); i++)
            {
                char a = str[i * 2 + 1];
                char b = str[i * 2 + 2];

                int intA;
                int intB;

                if (a >= '0' && a <= '9')
                    intA = a - '0';
                else if (a >= 'A' && a <= 'F')
                    intA = 10 + (a - 'A');
                else
                    return null;

                if (b >= '0' && b <= '9')
                    intB = b - '0';
                else if (b >= 'A' && b <= 'F')
                    intB = 10 + (b - 'A');
                else
                    return null;

                int val = intB + intA * 16;
                if (val < 0 || val > 255)
                    return null;

                switch (i)
                {
                    case 0:
                        color.R = (byte)val;
                        break;
                    case 1:
                        color.G = (byte)val;
                        break;
                    case 2:
                        color.B = (byte)val;
                        break;
                    default:
                        foundA = true;
                        color.A = (byte)val;
                        break;
                }
            }

            if (!foundA)
                color.A = 255;

            return color;
        }
    }

    public class TypeParser : PrimitiveParser
    {
        public override Type TargetType { get; } = typeof(Type);

        public override object Deserialize(XmlNode node)
        {
            string text = node.Value;

            var found = TypeResolver.Resolve(text);

            if (found == null)
                Console.WriteLine($"[WARN] Failed to resolve type '{text}' for node {node.GetXPath()}");

            return found;
        }

        public override string Serialize(object o)
        {
            if (o is Type t)
                return t.FullName;
            return o?.ToString();
        }
    }
}
