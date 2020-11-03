using System;

namespace JDef
{
    internal class PreProcessedDef
    {
        // Loaded stuff.
        public string Name;
        public string ClassName;
        public string ParentName;
        public bool IsAbstract;
        public string XmlData;

        // Generated stuff.
        public int[] InheritanceTree;
        public Type FinalType;
    }
}
