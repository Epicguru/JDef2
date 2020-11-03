using System;
using System.Collections.Generic;
using JXml.Utils;
using Microsoft.Xna.Framework;

namespace JXml
{
    public class CustomType
    {
        public string Name;
        public int Number;
        public float Float;
        public Vector2 Obj;

        public ListMergeMode Mode;

        public CustomType Inner;

        public string[] Objects;
        public List<byte> Bytes = new List<byte>() { 3, 4, 5 };
        public List<string[]> Nested;
        public Dictionary<string, double> Dict = new Dictionary<string, double>
        {
            { "James", 6767676 },
            { "Bob", 55 }
        };
        public List<CustomType> Subs;

        public IDisposable Disposable;

        public CustomType()
        {

        }
    }

    public class SubType : CustomType, IDisposable
    {
        public void Dispose()
        {
        }
    }
}
